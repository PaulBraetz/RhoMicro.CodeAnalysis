namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class Factories(TargetDataModel model) : ExpansionBase(model, Macro.Factories)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = (builder % "#region Factories")
            .AppendJoin(
                representableTypes,
                (b, a, t) => b.WithOperators(t) *
                    (Docs.Summary, b => _ = b * "Creates a new instance of " * target.CommentRef * ".") /
                    "public static " * target.ToMinimalOpenString() * " Create(" * a.Names.FullTypeName * " value) => new(value);" /
                    (Docs.Summary, b => _ = b * "Creates a new instance of " * target.CommentRef * ".") /
                    "public static " * target.ToMinimalOpenString() * ' ' * a.Names.CreateFromFunctionName * '(' * a.Names.FullTypeName % " value) => new(value);")
            .WithOperators(builder.CancellationToken) *
            "public static Boolean TryCreate<" * settings.GenericTValueName * ">(" *
            settings.GenericTValueName * " value, out " * target.ToMinimalOpenString() %
            " instance){" * (b => Extensions.TypeSwitchStatement(
                b,
                representableTypes,
                b => _ = b * (b => Extensions.UtilFullString(b, (b) => _ = b * "typeof(" * settings.GenericTValueName * ')')),
                t => t.Names,
                (b, v) => _ = b * "instance = new(" * (b => Extensions.UtilUnsafeConvert(b, settings.GenericTValueName, v.Names.FullTypeName, "value")) * ");return true;",
                b => _ = b * "instance = default; return false;")) * '}' /
            "public static " * target.ToMinimalOpenString() * " Create<" * settings.GenericTValueName * ">(" *
            settings.GenericTValueName * " value){" *
            (b => Extensions.TypeSwitchStatement(
                b,
                representableTypes,
                b => _ = b * (b => Extensions.UtilFullString(b, b => _ = b * "typeof(" * settings.GenericTValueName * ')')),
                t => t.Names,
                (b, v) => _ = b * "return new(" * (b => Extensions.UtilUnsafeConvert(b, settings.GenericTValueName, v.Names.FullTypeName, "value")) * ");",
                b => _ = b * (b => Extensions.InvalidCreationThrow(b, $"\"{target.ToFullOpenString()}\"", "value")) * ';')) %
            '}' %
            "#endregion";
    }
}
