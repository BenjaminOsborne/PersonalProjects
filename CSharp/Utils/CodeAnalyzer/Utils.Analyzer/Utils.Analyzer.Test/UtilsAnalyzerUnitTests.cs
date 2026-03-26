using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Utils.Analyzer.Test.CSharpCodeFixVerifier<
    Utils.Analyzer.UtilsAnalyzer,
    Utils.Analyzer.UtilsAnalyzerCodeFixProvider>;

namespace Utils.Analyzer.Test;

[TestClass]
public class UtilsAnalyzerUnitTest
{
    [TestMethod]
    public Task No_code() =>
        VerifyCS.VerifyAnalyzerAsync(@"");
        
    [TestMethod]
    public Task No_violation_vanilla() =>
        VerifyCS.VerifyAnalyzerAsync(_noViolationVanilla);
        
    [TestMethod]
    public Task No_violation_static_ctor() =>
        VerifyCS.VerifyAnalyzerAsync(_noViolationStaticCtor);
        
    [TestMethod]
    public Task No_violation_nest_static() =>
        VerifyCS.VerifyAnalyzerAsync(_noViolationNestedStaticCtor);
        
    [TestMethod]
    public Task Violates_record_scope_class() =>
        VerifyCS.VerifyAnalyzerAsync(_violatesRecord_ScopeClass, _GetDiagnostic());

    [TestMethod]
    public Task Violates_record_with_scope_class() =>
        VerifyCS.VerifyAnalyzerAsync(_violatesRecord_With_ScopeClass, _GetWithDiagnostic());
    
    [TestMethod]
    public Task No_violation_with_in_type() =>
        VerifyCS.VerifyAnalyzerAsync(_noViolationWithInType);

    [TestMethod]
    public Task Violates_class_scope_class() =>
        VerifyCS.VerifyAnalyzerAsync(_violatesClass_ScopeClass, _GetDiagnostic());
    
    [TestMethod]
    public Task Violates_record_scope_struct() =>
        VerifyCS.VerifyAnalyzerAsync(_violatesRecord_ScopeStruct, _GetDiagnostic());

    private static DiagnosticResult _GetDiagnostic()
    {
        var expected = VerifyCS.Diagnostic("UtilsAnalyzer")
            .WithLocation(new LinePosition(7, 16))
            .WithArguments("TargetType");
        return expected;
    }
    private static DiagnosticResult _GetWithDiagnostic()
    {
        var expected = VerifyCS.Diagnostic("UtilsAnalyzer")
            .WithSpan(9, 17, 9, 27)
            .WithArguments("TargetType");
        return expected;
    }

    private const string _noViolationVanilla = @"
namespace ConsoleApp;
class Foo
{
    public void Some()
    {
        int a = 4;
    }
}
";

    private const string _noViolationStaticCtor = @"
using System;
namespace ConsoleApp;
public class Foo
{
    public void Bar()
    {
        var a = TargetType.Create();
    }
}
" + _recordDefinition;

    private static readonly string _violatesRecord_ScopeClass = @"
using System;
namespace ConsoleApp;
public class Foo
{
    public void Bar()
    {
        var a = new TargetType();
        var b = TargetType.Create();
    }
}
" + _recordDefinition;

    private static readonly string _violatesRecord_With_ScopeClass = @"
using System;
namespace ConsoleApp;
public class Foo
{
    public void Bar()
    {
        var a = TargetType.Create();
        var b = a with { }; //Will trigger just using the with expression
    }
}
" + _recordDefinition;

    private static readonly string _violatesClass_ScopeClass = @"
using System;
namespace ConsoleApp;
public class Foo
{
    public void Bar()
    {
        var a = new TargetType();
        var b = TargetType.Create();
    }
}
" + _classDefinition;

    private static readonly string _violatesRecord_ScopeStruct = @"
using System;
namespace ConsoleApp;
public struct Foo
{
    public void Bar()
    {
        var a = new TargetType();
        var b = TargetType.Create();
    }
}
" + _recordDefinition;

    private const string _recordDefinition = @"
[PrivateCtor]
public record TargetType
{
    public static TargetType Create() => new ();
}" + _attributeDefinition;

    private const string _classDefinition = @"
[PrivateCtor]
public class TargetType
{
    public static TargetType Create() => new ();
}" + _attributeDefinition;

    private const string _noViolationNestedStaticCtor = @"
using System;
namespace ConsoleApp;

[PrivateCtor]
public class TargetType
{
    public static class InnerType
    {
        public static TargetType Create() => new ();
    }
}
" + _attributeDefinition;

    private const string _noViolationWithInType = @"
using System;
namespace ConsoleApp;

[PrivateCtor]
public record TargetType
{
    public void UseWith()
    {
        var a = new TargetType();
        var b = a with { };
    }
}
" + _attributeDefinition;

    private const string _attributeDefinition = @"
[AttributeUsage(AttributeTargets.Class)]
public class PrivateCtorAttribute : Attribute;
";
}