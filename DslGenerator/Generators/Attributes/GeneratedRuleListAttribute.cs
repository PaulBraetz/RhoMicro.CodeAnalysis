namespace RhoMicro.CodeAnalysis;

using System;

#if DSL_GENERATOR
[GenerateFactory]
#endif
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
sealed partial class GeneratedRuleListAttribute(String source) : Attribute
{
    public String Source { get; } = source;
}
