using AccountProcessor.Components.Services;
using NUnit.Framework;

namespace AccountProcessor.Tests;

public class SerialisationTests
{
    [Test]
    public void Wrapped_result()
    {
        AssertRoundTrip("Hey");
        AssertRoundTrip(new byte[] { 0, 1, 5 });
        AssertRoundTrip(7);
        AssertRoundTrip(15.0);
        AssertRoundTrip(new { A = "A", B = 3 });
        
        static void AssertRoundTrip<T>(T val)
        {
            var inst = WrappedResult.Create(val);
            var roundTrip = _RoundTrip(inst);
            Assert.That(roundTrip.Result, Is.EqualTo(val));
        }
    }

    [Test]
    public void Struct_wont_roundtrip()
    {
        var val = (A: "A", B: 3);
        var rt = _RoundTrip(val);
        Assert.That(rt, Is.Not.EqualTo(val));
    }

    private static T _RoundTrip<T>(T inst)
    {
        var json = JsonHelper.Serialise(inst);
        return JsonHelper.Deserialise<T>(json)!;
    }
}