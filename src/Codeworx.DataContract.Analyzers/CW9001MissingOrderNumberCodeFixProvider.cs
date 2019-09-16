using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Codeworx.DataContract.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CW9001MissingOrderNumberCodeFixProvider)), Shared]
    public class CW9001MissingOrderNumberCodeFixProvider : CodeFixProvider
    {
        private const string _title = "Add order number";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CW9001MissingOrderNumberAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            ////return WellKnownFixAllProviders.BatchFixer;
            return null;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {

                context.RegisterCodeFix(
                    CodeAction.Create(
                        _title,
                        cancellationToken => this.GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
                        nameof(CW9001MissingOrderNumberCodeFixProvider)),
                    diagnostic);
            }
            return Task.CompletedTask;
        }


        private async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var declaration = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent.AncestorsAndSelf().OfType<AttributeSyntax>().First();

            var property = declaration.FirstAncestorOrSelf<PropertyDeclarationSyntax>();

            var maxOrderNumber = property.GetMaxOrderNumber();

            var newList = declaration.ArgumentList ?? SyntaxFactory.AttributeArgumentList();

            newList = newList.WithArguments(
                SyntaxFactory.SeparatedList(newList.Arguments.Add(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals("Order"),
                        null,
                        SyntaxFactory.ParseExpression($"{maxOrderNumber + 1}")))));

            var newDeclaration = declaration.WithArgumentList(newList);

            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
