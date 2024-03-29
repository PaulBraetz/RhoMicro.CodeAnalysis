﻿namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System;
using System.Text;

#if DSL_GENERATOR
[IncludeFile]
#endif
[UnionType<String, Stream>]
readonly partial struct SourceText : IDisposable
{
    public String ToString(CancellationToken cancellationToken) =>
        Match(
            s => s,
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
            });
    public static SourceText Empty { get; } = String.Empty;
    public void Dispose()
    {
        if(TryAsStream(out var s))
            s.Dispose();
    }
}
