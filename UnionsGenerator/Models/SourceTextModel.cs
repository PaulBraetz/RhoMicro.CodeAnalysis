namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;

readonly partial struct SourceTextModel(String sourceText)
{
    public readonly String SourceText = sourceText;
}
