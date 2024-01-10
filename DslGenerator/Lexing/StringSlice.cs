#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{DebuggerDisplayString()}")]
readonly record struct StringSlice(String Source, Int32 Start, Int32 Length) : IEquatable<String>, IEquatable<Char>
{
    public override String ToString() => Source.Substring(Start, Length);
    private String DebuggerDisplayString() => ToString().Replace("\n", "\\n").Replace("\r", "\\r");
    public Boolean Equals(String other)
    {
        if(Length != other.Length)
            return false;
        for(var i = 0; i < Length; i++)
        {
            if(Source[i + Start] != other[i])
                return false;
        }

        return true;
    }
    public Boolean Equals(Char other) => Length == 1 && Source[Start] == other;
    public Boolean Equals(StringSlice other)
    {
        if(Length != other.Length)
            return false;
        for(var i = 0; i < Length; i++)
        {
            if(Source[i + Start] != other.Source[i + other.Start])
                return false;
        }

        return true;
    }
    public override Int32 GetHashCode()
    {
        var result = EqualityComparer<Int32>.Default.GetHashCode(Length);
        for(var i = Start; i < Start + Length; i++)
        {
            result = result * -1521134295 + EqualityComparer<Char>.Default.GetHashCode(Source[i]);
        }

        return result;
    }
}