namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System;
using System.Collections.Generic;

internal static class Docs
{
    public static void Inherit(ExpandingMacroBuilder builder) => _ = builder % "/// <inheritdoc/>";

    public static void Summary(
        ExpandingMacroBuilder builder,
        Action<ExpandingMacroBuilder> summary)
    {
        _ = summary ?? throw new ArgumentNullException(nameof(summary));

        _ = builder *
            "/// <summary>" /
            "/// " % summary %
            "/// </summary>";
    }
    public static void Summary(
        ExpandingMacroBuilder builder,
        Action<ExpandingMacroBuilder> summary,
        IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> typeParameters)
    {
        _ = builder *
            (Summary, summary) %
            typeParameters.Select(p =>
                (Action<ExpandingMacroBuilder>)(b => _ = b *
                "/// <typeparam name=\"" * p.Name * "\">" /
                "/// " % p.Summary %
                "/// </typeparam>"));
    }

    public static void MethodSummary(
        ExpandingMacroBuilder builder,
        Action<ExpandingMacroBuilder> summary,
        IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> parameters)
    {
        _ = builder *
            (Summary, summary) %
            parameters.Select(p =>
                (Action<ExpandingMacroBuilder>)(b => _ = b *
                "/// <param name=\"" * p.Name * "\">" /
                "/// " % p.Summary %
                "/// </param>"));
    }
    public static void MethodSummary(
        ExpandingMacroBuilder builder,
        Action<ExpandingMacroBuilder> summary,
        IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> parameters,
        Action<ExpandingMacroBuilder> returns)
    {
        _ = builder *
            (MethodSummary, summary, parameters) *
            "/// <returns>" /
            "/// " % returns %
            "/// </returns>";
    }
    public static void MethodSummary(
        ExpandingMacroBuilder builder,
        Action<ExpandingMacroBuilder> summary,
        IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> typeParameters,
        IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> parameters)
    {
        _ = builder *
            (Summary, summary, typeParameters) %
            parameters.Select(p =>
                (Action<ExpandingMacroBuilder>)(b => _ = b *
                "/// <param name=\"" * p.Name * "\">" /
                "/// " % p.Summary %
                "/// </param>"));
    }
    public static void MethodSummary(
        ExpandingMacroBuilder builder,
        Action<ExpandingMacroBuilder> summary,
        IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> typeParameters,
        IEnumerable<(String Name, Action<ExpandingMacroBuilder> Summary)> parameters,
        Action<ExpandingMacroBuilder> returns)
    {
        _ = builder *
            (MethodSummary, summary, typeParameters, parameters) *
            "/// <returns>" /
            "/// " % returns %
            "/// </returns>";
    }
}
