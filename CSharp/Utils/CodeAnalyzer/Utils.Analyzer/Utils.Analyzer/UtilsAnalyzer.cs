using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Utils.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UtilsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UtilsAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Ctor access";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression, SyntaxKind.WithExpression);
        }

        private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            var type = context.SemanticModel.GetTypeInfo(node).Type;
            if (type is null)
            {
                return;
            }

            var attributes = type.GetAttributes();
            if (!attributes.Any(x => x.AttributeClass?.Name == nameof(PrivateCtorAttribute)))
            {
                return; //exit if missing target attribute
            }

            const int recursionLimit = 1000;
            var recursionCount = 0;
            var parent = node.Parent;
            while (parent != null) //Parent should become null once reach end of scope
            {
                if (parent is BaseTypeDeclarationSyntax) //Is the Class/Record/Struct of the scope invoking the "ObjectCreation" ctor
                {
                    var decSymbol = context.SemanticModel.GetDeclaredSymbol(parent);
                    var origDef = decSymbol?.OriginalDefinition;
                    var isPrivate = origDef != null && origDef.Equals(type, SymbolEqualityComparer.Default);
                    if (isPrivate)
                    {
                        return;
                    }
                }
                if (recursionCount++ > recursionLimit)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), $"{type.Name}: REACHED SCOPE RECURSION LIMIT: {recursionLimit}"));
                }
                parent = parent.Parent; //recurse up a scope
            }

            //Report Diagnostic if parent null and type NOT in scope hierarchy
            context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), type.Name));
        }
    }
}
