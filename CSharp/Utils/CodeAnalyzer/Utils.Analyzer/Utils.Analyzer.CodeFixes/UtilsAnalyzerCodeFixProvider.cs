using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Utils.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UtilsAnalyzerCodeFixProvider)), Shared]
    public class UtilsAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UtilsAnalyzer.DiagnosticId);

        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        //No fix currently suggested
        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context) =>
            Task.CompletedTask;
    }
}
