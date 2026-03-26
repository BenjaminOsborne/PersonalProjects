using System;

namespace Utils.Analyzer
{
    /// <summary> Detected by "Utils.Analyzer" package. Attribute prevents types instantiating it. </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PrivateCtorAttribute : Attribute
    {
    }
}