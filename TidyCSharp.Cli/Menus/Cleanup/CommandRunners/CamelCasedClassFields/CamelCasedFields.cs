using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedClassFields.Option;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedClassFields;

public class CamelCasedFields : VariableRenamingBase
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

    private class Rewriter : VariableRenamingBaseRewriter
    {
        public Rewriter(Document workingDocument, bool isReportOnlyMode, ICleanupOption options) : base(workingDocument, isReportOnlyMode, options) { }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ClassDeclarationSyntax classDeclarationSyntax = null;

            Task.Run(async delegate
                {
                    classDeclarationSyntax = await RenameDeclarationsAsync(node);
                }).GetAwaiter().GetResult();

            return base.VisitClassDeclaration(classDeclarationSyntax as ClassDeclarationSyntax);
        }

        private async Task<ClassDeclarationSyntax> RenameDeclarationsAsync(ClassDeclarationSyntax classNode)
        {
            if (CheckOption((int)CleanupTypes.NormalFields))
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

            if (CheckOption((int)CleanupTypes.ConstFields))
            {
                var renamingResult = await new ConstRenamer(WorkingDocument).RenameDeclarationsAsync(classNode);

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