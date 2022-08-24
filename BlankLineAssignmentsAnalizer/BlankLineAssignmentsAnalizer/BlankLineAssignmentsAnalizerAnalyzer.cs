using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace BlankLineAssignmentsAnalizer
{
    /// <summary>
    /// The analyzer provides warning messages for blocks of code with variable assignments, which doesn't have blank lines before and after itself.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BlankLineAssignmentsAnalizerAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Analyzer identifier
        /// </summary>
        public const string DiagnosticIdBefore = "BLAA_1";

        /// <summary>
        /// Analyzer identifier
        /// </summary>
        public const string DiagnosticIdAfter = "BLAA_2";

        /// <summary>
        /// Title
        /// </summary>
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalizerResources.AnalyzerTitle), AnalizerResources.ResourceManager, typeof(AnalizerResources));

        /// <summary>
        /// Description
        /// </summary>
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalizerResources.AnalyzerDescription), AnalizerResources.ResourceManager, typeof(AnalizerResources));

        /// <summary>
        /// Message text
        /// </summary>
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalizerResources.AnalyzerMessageFormat), AnalizerResources.ResourceManager, typeof(AnalizerResources));

        /// <summary>
        /// Category
        /// </summary>
        private const string Category = "Formatting Style";

        /// <summary>
        /// Rule for a blocks of assignment without blank lines
        /// </summary>
        public static readonly DiagnosticDescriptor AssignmentsRuleBefore = new DiagnosticDescriptor(DiagnosticIdBefore, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        public static readonly DiagnosticDescriptor AssignmentsRuleAfter = new DiagnosticDescriptor(DiagnosticIdAfter, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        /// <summary>
        /// Custom rules registration
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AssignmentsRuleBefore, AssignmentsRuleAfter);

        /// <summary>
        /// Analyzer initialize
        /// </summary>
        /// <param name="context"> Context </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCodeBlockAction(AnalizeCodeBlock);
        }

        /// <summary>
        /// Code block analyze
        /// </summary>
        /// <param name="context"> CodeBlock context </param>
        private static void AnalizeCodeBlock(CodeBlockAnalysisContext context)
        {
            var previousNode = default(SyntaxNode);
            var blockNodes = context.CodeBlock.ChildNodes();

            foreach (var blockNode in blockNodes)
            {
                if (blockNode.Kind() != SyntaxKind.Block)
                {
                    continue;
                }

                var childNodes = blockNode.ChildNodes();
                foreach (var childNode in childNodes)
                {
                    var currentType = GetNodeKind(childNode);

                    var currentLineSpan = childNode.SyntaxTree.GetLineSpan(childNode.Span);
                    if (previousNode == null)
                    {
                        previousNode = childNode;
                        continue;
                    }

                    var previousLineSpan = previousNode.SyntaxTree.GetLineSpan(previousNode.Span);
                    var previousType = GetNodeKind(previousNode);

                    if (currentLineSpan.StartLinePosition.Line - previousLineSpan.EndLinePosition.Line == 1)
                    {
                        var isDiffOnBegin = (currentType == SyntaxKind.SimpleAssignmentExpression || currentType == SyntaxKind.LocalDeclarationStatement) && (previousType != SyntaxKind.SimpleAssignmentExpression && previousType != SyntaxKind.LocalDeclarationStatement);
                        var isDiffOnEnd = (previousType == SyntaxKind.SimpleAssignmentExpression || previousType == SyntaxKind.LocalDeclarationStatement) && (currentType != SyntaxKind.SimpleAssignmentExpression && currentType != SyntaxKind.LocalDeclarationStatement);

                        if (isDiffOnBegin)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(AssignmentsRuleBefore, childNode.GetLocation(), DiagnosticSeverity.Warning));
                        }
                        else if (isDiffOnEnd)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(AssignmentsRuleAfter, previousNode.GetLocation(), DiagnosticSeverity.Warning));
                        }
                    }

                    previousNode = childNode;
                }

                break;
            }
        }

        /// <summary>
        /// Get SyntaxNode kind
        /// </summary>
        /// <param name="node"> Node </param>
        /// <returns> Syntax kind </returns>
        private static SyntaxKind GetNodeKind(SyntaxNode node)
        {
            if (node is ExpressionStatementSyntax expressionNode)
            {
                return expressionNode.Expression?.Kind() ?? expressionNode.Kind();
            }

            return node.Kind();
        }
    }
}