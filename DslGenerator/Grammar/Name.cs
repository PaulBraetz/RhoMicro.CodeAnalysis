#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
sealed partial record Name : SyntaxNode
{
    public Name(String value)
    {
        for(var i = 0; i < value.Length; i++)
        {
            if(!Utils.IsValidNameChar(value[i]))
            {
                throw new ArgumentException($"Invalid character at index {i}: '{value[i]}'. Rule names must contain letters only.)", nameof(value));
            }
        }

        Value = value;
    }

    public String Value { get; }
    public override String ToDisplayString() => Value;
    public override String ToMetaString() => $"new {nameof(Name)}(\"{Value}\")";
}
