#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

#if DSL_GENERATOR
[IncludeFile]
#endif
sealed record Location(
    TextSpan TextSpan,
    LinePositionSpan LinePositionSpan
#if DSL_GENERATOR
    , String FilePath
#endif
    )
{
    public static Location None { get; } = new(default, default
#if DSL_GENERATOR
        , String.Empty
#endif
        );
    public static Location Create(Int32 line, Int32 character, Int32 position
#if DSL_GENERATOR
            , String filePath
#endif
        ) =>
        new(new(position, 0), new(new(line, character), new(line, character)) //TODO: implement location spans
#if DSL_GENERATOR
            , filePath
#endif
            );
#if DSL_GENERATOR
    public Microsoft.CodeAnalysis.Location ToMsLocation() =>
        Equals(None) ?
        Microsoft.CodeAnalysis.Location.None :
        Microsoft.CodeAnalysis.Location.Create(
            FilePath,
            TextSpan.ToMsTextSpan(),
            LinePositionSpan.ToMsLinePositionSpan());
#endif
}

readonly record struct LinePosition(Int32 Line, Int32 Character)
{
#if DSL_GENERATOR
    public Microsoft.CodeAnalysis.Text.LinePosition ToMsLinePosition() =>
        new(Line, Character);
#endif
}
readonly record struct LinePositionSpan(LinePosition Start, LinePosition End)
{
#if DSL_GENERATOR
    public Microsoft.CodeAnalysis.Text.LinePositionSpan ToMsLinePositionSpan() =>
        new(Start.ToMsLinePosition(), End.ToMsLinePosition());
#endif
}
readonly record struct TextSpan(Int32 Position, Int32 Length)
{
#if DSL_GENERATOR
    public Microsoft.CodeAnalysis.Text.TextSpan ToMsTextSpan() =>
        new(Position, Length);
#endif
}