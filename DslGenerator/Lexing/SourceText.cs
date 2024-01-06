namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using MsSourceText = Microsoft.CodeAnalysis.Text.SourceText;

using System;
using Microsoft.CodeAnalysis;
using System.Text;

[UnionType(typeof(String))]
[UnionType(typeof(Stream))]
[UnionType(typeof(AdditionalText))]
readonly partial struct SourceText : IDisposable
{
    public Location MatchLocation(Func<AdditionalText, Location> onAdditionalText) =>
        Match(onAdditionalText, s => Location.None, s => Location.None);
    public String ToString(CancellationToken cancellationToken) =>
        Match(
            s => s.GetText(cancellationToken)?.ToString() ?? String.Empty,
            s =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var reader = new StreamReader(s);
                var resultBuilder = new StringBuilder();
                var line = reader.ReadLine();
                while(line != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _ = resultBuilder.AppendLine(line);
                    line = reader.ReadLine();
                }

                var result = resultBuilder.ToString();

                return result;
            },
            s => s);
    public static SourceText Empty { get; } = String.Empty;
    public void Dispose()
    {
        if(IsStream)
            AsStream.Dispose();
    }
}
