using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class CamelCasedFields : VariableRenamingBase, ICodeCleaner
    {
        protected override SyntaxNode GetWorkingNode(SyntaxNode initialSourceNode, SyntaxAnnotation annotationForSelectedNodes)
        {
            return
                initialSourceNode
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(m => !m.HasAnnotation(annotationForSelectedNodes));
        }

        protected override VariableRenamingBaseRewriter GetRewriter(Document workingDocument)
        {
            return new Rewriter(workingDocument, IsReportOnlyMode, Options);
        }

        class Rewriter : VariableRenamingBaseRewriter
        {
            public Rewriter(Document workingDocument, bool isReportOnlyMode, ICleanupOption options) : base(workingDocument, isReportOnlyMode, options) { }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                ClassDeclarationSyntax classDeclarationSyntax = null;

                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory
                    .Run(async delegate
                {
                    classDeclarationSyntax = await RenameDeclarationsAsync(node);
                });

                return base.VisitClassDeclaration(classDeclarationSyntax as ClassDeclarationSyntax);
            }

            async Task<ClassDeclarationSyntax> RenameDeclarationsAsync(ClassDeclarationSyntax classNode)
            {
                if (CheckOption((int)CamelCasedClassFields.CleanupTypes.NormalFields))
                {
                    var renamingResult = await new FieldRenamer(WorkingDocument).RenameDeclarationsAsync(classNode);

                    if (renamingResult != null && renamingResult.Node != null)
                    {
                        classNode = renamingResult.Node as ClassDeclarationSyntax;
                        WorkingDocument = renamingResult.Document;
                    }

                    if (renamingResult != null)
                    {
                        var lineSpan = classNode.GetFileLinePosSpan();

                        AddReport(new ChangesReport(classNode)
                        {
                            LineNumber = lineSpan.StartLinePosition.Line,
                            Column = lineSpan.StartLinePosition.Character,
                            Message = "Camel Cased Fields",
                            Generator = nameof(CamelCasedFields),
                        });
                    }
                }

                if (CheckOption((int)CamelCasedClassFields.CleanupTypes.ConstFields))
                {
                    var renamingResult = await new CONSTRenamer(WorkingDocument).RenameDeclarationsAsync(classNode);

                    if (renamingResult != null && renamingResult.Node != null)
                    {
                        classNode = renamingResult.Node as ClassDeclarationSyntax;
                        WorkingDocument = renamingResult.Document;
                    }

                    if (renamingResult != null)
                    {
                        var lineSpan = classNode.GetFileLinePosSpan();

                        AddReport(new ChangesReport(classNode)
                        {
                            LineNumber = lineSpan.StartLinePosition.Line,
                            Column = lineSpan.StartLinePosition.Character,
                            Message = "CamelCasedFields",
                            Generator = nameof(CamelCasedFields)
                        });
                    }
                }

                return classNode;
            }
        }
    }
}