namespace RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

static class StringExtensions
{
    //source: https://stackoverflow.com/a/58853591
    private static readonly Regex _camelCasePattern =
        new(@"([A-Z])([A-Z]+|[a-z0-9_]+)($|[A-Z]\w*)", RegexOptions.Compiled);
    public static String ToCamelCase(this String value)
    {
        if(value.Length == 0)
            return value;

        if(value.Length == 1)
            return value.ToLowerInvariant();

        //source: https://stackoverflow.com/a/58853591
        var result = _camelCasePattern.Replace(
            value,
            static m => $"{m.Groups[1].Value.ToLowerInvariant()}{m.Groups[2].Value.ToLowerInvariant()}{m.Groups[3].Value}");

        return result;
    }
    public static String ToGeneratedCamelCase(this String value)
    {
        var result = $"__{value.ToCamelCase()}";

        return result;
    }
}
