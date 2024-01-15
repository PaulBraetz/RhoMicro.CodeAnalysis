namespace RhoMicro.CodeAnalysis.Library.Text;

sealed record IndentedStringBuilderOptions
{
    public StringOrChar Indentation { get; init; } = '\t';
    public StringOrChar NewLine { get; init; } = '\n';
    public Block DefaultBlock { get; init; } = Block.BracesBreak;
    public String GeneratorName { get; init; } = String.Empty;
    public Boolean PrependMarkerComment { get; init; } = false;
    public Boolean PrependWarningDisablePragma { get; init; } = false;
    public Boolean PrependNullableEnable { get; init; } = false;

    public static IndentedStringBuilderOptions GeneratedFile = new()
    {
        PrependMarkerComment = true,
        PrependWarningDisablePragma = true,
        PrependNullableEnable = true
    };
    public static IndentedStringBuilderOptions Default = new();
}
