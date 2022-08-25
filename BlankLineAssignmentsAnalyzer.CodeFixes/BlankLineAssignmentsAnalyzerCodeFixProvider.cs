using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedDocument: c => AddBlankLine(context, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Problem resolver
        /// </summary>
        /// <param name="context"> Fix context </param>
        /// <param name="cancellationToken"> Cancellation token </param>
        /// <returns></returns>
        private static async Task<Document> AddBlankLine(CodeFixContext context, CancellationToken cancellationToken)
        {
            var oldRoot = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var problemNode = oldRoot.FindNode(diagnosticSpan);

            SyntaxNode replacedNode;
            if (diagnostic.Id.Equals(BlankLineAssignmentsAnalyzer.DiagnosticIdBefore))
            {
                var firstToken = problemNode.GetFirstToken();
                replacedNode = problemNode.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxFactory.Whitespace(Environment.NewLine + Environment.NewLine)));
            }
            else
            {

                var lastToken = problemNode.GetLastToken();
                replacedNode = problemNode.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(SyntaxFactory.Whitespace(Environment.NewLine + Environment.NewLine)));
            }

            // Replace the old local problemNode with the new local problemNode.
            var newRoot = oldRoot.ReplaceNode(problemNode, replacedNode);

            // Return document with transformed tree.
            return context.Document.WithSyntaxRoot(newRoot);
        }
    }
}