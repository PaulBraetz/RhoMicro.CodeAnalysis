namespace RhoMicro.CodeAnalysis.Library.Text;

partial record IndentedStringBuilderOptions
{
    public StringOrChar DefaultIndentation { get; init; } = StringOrChar.Tab;
    public StringOrChar NewLine { get; init; } = StringOrChar.NewLine;
    public String GeneratorName { get; init; } = String.Empty;
    public Boolean PrependMarkerComment { get; init; } = false;
    public Boolean PrependWarningDisablePragma { get; init; } = false;
    public Boolean PrependNullableEnable { get; init; } = false;
    public String License { get; init; } = String.Empty;
    public CancellationToken AmbientCancellationToken { get; init; } = CancellationToken.None;

    private const String _defaultLicense = 
        """
        // The tool used to generate this code may be subject to license terms;
        // this generated code is however not subject to those terms, instead it is
        // subject to the license (if any) applied to the containing project.
        """;

    public static IndentedStringBuilderOptions GeneratedFile = new()
    {
        PrependMarkerComment = true,
        PrependWarningDisablePragma = true,
        PrependNullableEnable = true,
        License = _defaultLicense
    };
    public static IndentedStringBuilderOptions Default = new();
}
