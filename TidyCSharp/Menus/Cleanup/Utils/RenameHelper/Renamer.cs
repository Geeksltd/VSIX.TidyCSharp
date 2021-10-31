using Geeks.GeeksProductivityTools.Menus.Cleanup.Renaming;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
	abstract partial class Renamer
	{
		SemanticModel _semanticModel;
		Document WorkingDocument { get; set; }

		public Renamer(Document document)
		{
			WorkingDocument = document;
			Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				_semanticModel = await WorkingDocument.GetSemanticModelAsync();
			});
		}

		public async Task<RenameResult> RenameDeclarations(SyntaxNode containerNode)
		{
			SyntaxNode currentNode;
			var newNode = containerNode;
			RenameResult renamingResult = null;
			RenameResult workingRenamingResult = null;
			do
			{
				currentNode = newNode;
				workingRenamingResult = await RenameDeclarationNodeOfContainerNode(currentNode);

				if (workingRenamingResult != null)
				{
					renamingResult = workingRenamingResult;
					if (workingRenamingResult.Node != null)
					{
						newNode = renamingResult.Node;
						WorkingDocument = renamingResult.Document;
						_semanticModel = await renamingResult.Document.GetSemanticModelAsync();
					}
				}
			}
			while (workingRenamingResult != null && newNode != currentNode);
			return renamingResult;
		}

		IList<string> VisitedTokens = new List<string>();

		async Task<RenameResult> RenameDeclarationNodeOfContainerNode(SyntaxNode containerNode)
		{
			var filteredItemsToRenmae = GetItemsToRename(containerNode).Where(i => VisitedTokens.Contains(i.ValueText) == false);

			foreach (var identifierToRename in filteredItemsToRenmae)
			{
				var currentName = identifierToRename.ValueText;
				var newNames = GetNewName(currentName);

				if (newNames == null) continue;

				var selectedName = currentName;
				RenameResult result = null;

				foreach (var newName in newNames)
				{
					var renameResult = await RenameIdentifierOfContainerNode(containerNode, identifierToRename, newName);

					if (renameResult == null) continue;
					result = renameResult.Value.Key;
					selectedName = renameResult.Value.Value;
				}

				VisitedTokens.Add(selectedName);

				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		async Task<KeyValuePair<RenameResult, string>?> RenameIdentifierOfContainerNode(SyntaxNode containerNode, SyntaxToken identifierToRename, string newVarName)
		{
			RenameResult result = null;

			var currentName = identifierToRename.ValueText;
			var selectedName = currentName;


			if (string.Compare(newVarName, currentName, false) == 0) return null;
			if (ValidateNewName(newVarName) == false) return null;


			var identifierDeclarationNode = identifierToRename.Parent;


			var identifierSymbol = _semanticModel.GetDeclaredSymbol(identifierDeclarationNode);


			var validateNameResult = await RenameHelper.IsValidNewMemberNameAsync(_semanticModel, identifierSymbol, newVarName);


			if (validateNameResult == false) return null;


			result = await RenameSymbol(WorkingDocument, await WorkingDocument.GetSyntaxRootAsync(), containerNode, identifierDeclarationNode, newVarName);


			if (result != null) selectedName = newVarName;

			return new KeyValuePair<RenameResult, string>(result, selectedName);
		}

		const string KEYWORD = "Keyword";
		Lazy<string[]> keywords =
			new Lazy<string[]>(
			() =>
				Enum.GetNames(typeof(SyntaxKind))
					.Where(k => k.EndsWith(KEYWORD))
					.Select(k => k.Remove(k.Length - KEYWORD.Length).ToLower())
					.ToArray()
			);

		protected virtual bool ValidateNewName(string newVarName)
		{
			bool isValid = true;
			isValid &= SyntaxFacts.IsValidIdentifier(newVarName);
			isValid &= !keywords.Value.Contains(newVarName);
			return isValid;
		}

		protected abstract IEnumerable<SyntaxToken> GetItemsToRename(SyntaxNode currentNode);

		protected abstract string[] GetNewName(string currentName);

		protected static string GetCamelCased(string variableName)
		{

			var noneLetterCount = variableName.TakeWhile(x => char.IsLetter(x) == false).Count();

			if (noneLetterCount >= variableName.Length) return variableName;

			if (char.IsUpper(variableName[noneLetterCount]) == false) return variableName;

			var newVarName =
				variableName.Substring(0, noneLetterCount) +
				variableName.Substring(noneLetterCount, 1).ToLower() +
				variableName.Substring(noneLetterCount + 1);

			return newVarName;
		}

		protected static string GetPascalCased(string variableName)
		{

			var noneLetterCount = variableName.TakeWhile(x => char.IsLetter(x) == false).Count();

			if (noneLetterCount >= variableName.Length) return variableName;

			if (char.IsUpper(variableName[noneLetterCount]) == false) return variableName;

			var newVarName =
				variableName.Substring(0, noneLetterCount) +
				variableName.Substring(noneLetterCount, 1).ToUpper() +
				variableName.Substring(noneLetterCount + 1);

			return newVarName;
		}

		protected static bool IsPrivate(FieldDeclarationSyntax x)
		{
			return
				x.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)) ||
				x.Modifiers
					.Any(
						m =>
							m.IsKind(SyntaxKind.PublicKeyword) ||
							m.IsKind(SyntaxKind.ProtectedKeyword) ||
							m.IsKind(SyntaxKind.InternalKeyword)
					) == false;
		}

		const string SELECTED_METHOD_ANNOTATION = "SELECTED_METHOD_ANNOTATION";
		static async Task<RenameResult> RenameSymbol(Document document, SyntaxNode root, SyntaxNode startNode, ParameterSyntax declarationNode, string newName)
		{

			var identifierToken = declarationNode.Identifier;

			var methodAnnotation = new SyntaxAnnotation(SELECTED_METHOD_ANNOTATION);
			var changeDic = new Dictionary<SyntaxNode, SyntaxNode>();

			if (startNode != null)
			{

				changeDic.Add(startNode, startNode.WithAdditionalAnnotations(methodAnnotation));
			}


			changeDic.Add(declarationNode, declarationNode.WithIdentifier(identifierToken.WithAdditionalAnnotations(RenameAnnotation.Create())));


			var annotatedRoot = root.ReplaceNodes(changeDic.Keys, (x, y) => changeDic[x]);

			var newSolution = await RenameSymbol(document, annotatedRoot, identifierToken, methodAnnotation, newName);

			return await GetNewStartNode(newSolution, document, methodAnnotation, startNode);
		}

		static async Task<RenameResult> RenameSymbol(Document document, SyntaxNode root, SyntaxNode startNode, VariableDeclaratorSyntax declarationNode, string newName)
		{

			var identifierToken = declarationNode.Identifier;

			var methodAnnotation = new SyntaxAnnotation(SELECTED_METHOD_ANNOTATION);
			var changeDic = new Dictionary<SyntaxNode, SyntaxNode>();

			if (startNode != null)
			{
				changeDic.Add(startNode, startNode.WithAdditionalAnnotations(methodAnnotation));
			}


			changeDic.Add(declarationNode, declarationNode.WithIdentifier(identifierToken.WithAdditionalAnnotations(RenameAnnotation.Create())));

			var annotatedRoot = root.ReplaceNodes(changeDic.Keys, (x, y) => changeDic[x]);

			var newSolution = await RenameSymbol(document, annotatedRoot, identifierToken, methodAnnotation, newName);

			return await GetNewStartNode(newSolution, document, methodAnnotation, startNode);
		}

		static async Task<RenameResult> GetNewStartNode(Solution newSolution, Document document, SyntaxAnnotation methodAnnotation, SyntaxNode startNode)
		{

			var newDocument =
			   newSolution.Projects.FirstOrDefault(x => x.Name == document.Project.Name)
			   .Documents.FirstOrDefault(x => x.FilePath == document.FilePath);

			var newRoot = await
				newSolution.Projects.FirstOrDefault(x => x.Name == document.Project.Name)
				.Documents.FirstOrDefault(x => x.FilePath == document.FilePath).GetSyntaxRootAsync();

			return new RenameResult
			{
				Node = newRoot.GetAnnotatedNodes(methodAnnotation).FirstOrDefault(),
				Solution = newSolution,
				Document = newDocument
			};
		}

		public static async Task<RenameResult> RenameSymbol(Document document, SyntaxNode root, SyntaxNode startNode, SyntaxToken identifierToken, string newName)
		{

			return await RenameSymbol(document, root, startNode, identifierToken.Parent, newName);
		}

		public static async Task<RenameResult> RenameSymbol(Document document, SyntaxNode root, SyntaxNode startNode, SyntaxNode declarationNode, string newName)
		{

			if (declarationNode is VariableDeclaratorSyntax variableNode)
			{

				return await RenameSymbol(document, root, startNode, variableNode, newName);
			}

			if (declarationNode is ParameterSyntax parameterNode)
			{

				return await RenameSymbol(document, root, startNode, parameterNode, newName);
			}

			return null;
		}

		static async Task<Solution> RenameSymbol(Document document, SyntaxNode annotatedRoot, SyntaxToken identifierNode, SyntaxAnnotation methodAnnotation, string newName, CancellationToken cancellationToken = default(CancellationToken))
		{

			var annotatedSolution = document.Project.Solution.WithDocumentSyntaxRoot(document.Id, annotatedRoot);

			var annotatedDocument = annotatedSolution.GetDocument(document.Id);

			annotatedRoot = await annotatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			var annotatedToken = annotatedRoot.FindToken(identifierNode.SpanStart);

			var semanticModel = await annotatedDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			var symbol = semanticModel.GetDeclaredSymbol(annotatedToken.Parent, cancellationToken);

			//await Microsoft.CodeAnalysis.Rename.Renamer.RenameSymbolAsync(annotatedSolution, symbol, newName, annotatedSolution.Workspace.Options, cancellationToken);
			var renamingresult = await Microsoft.CodeAnalysis.Rename.Renamer.RenameSymbolAsync(annotatedSolution, symbol, newName, annotatedSolution.Workspace.Options);

			return renamingresult;
		}
	}
}