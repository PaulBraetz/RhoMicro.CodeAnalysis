namespace RhoMicro.CodeAnalysis.Library.Text;

partial record IndentedStringBuilderOptions
{
    public StringOrChar DefaultIndentation { get; init; } = StringOrChar.Tab;
    public StringOrChar NewLine { get; init; } = StringOrChar.NewLine;
    public String GeneratorName { get; init; } = String.Empty;
    public Boolean PrependMarkerComment { get; init; } = false;
    public Boolean PrependWarningDisablePragma { get; init; } = false;
    public Boolean PrependNullableEnable { get; init; } = false;
    public CancellationToken AmbientCancellationToken { get; init; } = CancellationToken.None;

    public static IndentedStringBuilderOptions GeneratedFile = new()
    {
        PrependMarkerComment = true,
        PrependWarningDisablePragma = true,
        PrependNullableEnable = true
    };
    public static IndentedStringBuilderOptions Default = new();
}
