using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace BlankLineAssignmentsAnalyzer
{
    /// <summary>
    /// CodeFix provider for a BlankLineAssignmentsAnalyzer
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BlankLineAssignmentsAnalyzerCodeFixProvider)), Shared]
    public class BlankLineAssignmentsAnalyzerCodeFixProvider : CodeFixProvider
    {
        /// <summary>
        /// A list of diagnostic IDs that this provider can provide fixes for.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BlankLineAssignmentsAnalyzer.DiagnosticIdBefore, BlankLineAssignmentsAnalyzer.DiagnosticIdAfter);

        /// <summary>
        /// Gets an optional <see cref="FixAllProvider"/> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
        /// Return null if the provider doesn't support fix all/multiple occurrences.
        /// Otherwise, you can return any of the well known fix all providers from <see cref="WellKnownFixAllProviders"/> or implement your own fix all provider.
        /// </summary>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        /// Computes one or more fixes for the specified <see cref="CodeFixContext"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="CodeFixContext"/> containing context information about the diagnostics to fix.
        /// The context must only contain diagnostics with a <see cref="Diagnostic.Id"/> included in the <see cref="FixableDiagnosticIds"/> for the current provider.
        /// </param>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                var problemNode = root.FindNode(diagnosticSpan);

                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: CodeFixResources.CodeFixTitle,
                        createChangedDocument: cancellationToken => AddBlankLineAsync(context.Document, diagnostic.Id, problemNode, cancellationToken),
                        equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                    diagnostic);
            }
        }


        /// <summary>
        /// Problem resolver
        /// </summary>
        /// <param name="document"> Document </param>
        /// <param name="problemID"> Diagnostic identifier </param>
        /// <param name="problemNode"> Node with problem </param>
        /// <param name="cancellationToken"> Cancellation token </param>
        /// <returns> Fixed document </returns>
        private static async Task<Document> AddBlankLineAsync(Document document, string problemID, SyntaxNode problemNode, CancellationToken cancellationToken)
        {
            var lastToken = problemNode.GetLastToken();
            var replacedNode = problemNode.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine + Environment.NewLine)));

            // Replace the old local problemNode with the new local problemNode.
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(problemNode, replacedNode);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}