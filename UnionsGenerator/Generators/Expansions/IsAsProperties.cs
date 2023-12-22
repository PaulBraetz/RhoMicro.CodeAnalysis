namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class IsAsProperties(TargetDataModel model) : ExpansionBase(model, Macro.IsAsProperties)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var attributes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder % "#region IsAsProperties";
#pragma warning restore IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder.AppendJoin(
                 attributes,
                 (b, a, t) => b.WithOperators(builder.CancellationToken) *
                 (Docs.Summary, b => _ = b *
                    "Gets a value indicating whether this instance is representing a value of type " * a.DocCommentRef * '.') *
                 "public global::System.Boolean " * a.Names.IsPropertyName *
                 " => __tag == " * a.GetCorrespondingTag(Model) * ';' /
                 (Docs.Summary, b => _ = b *
                 "Attempts to retrieve the value represented by this instance as a " * a.DocCommentRef * '.') *
                 "/// <exception cref=\"global::System.InvalidOperationException\">Thrown if the instance is not representing a value of type " * a.DocCommentRef * ".</exception>" /
                 "public " * a.Names.FullTypeName * ' ' * a.Names.AsPropertyName * " => __tag == " *
                 a.GetCorrespondingTag(Model) * '?' *
                 a.Storage.TypesafeInstanceVariableExpression * ':' *
                 (InvalidConversionThrow, $"typeof({a.Names.FullTypeName}).Name") % ';',
                 builder.CancellationToken);
        } else
        {
            var attribute = attributes[0];

            _ = builder *
                (Docs.Summary, b => _ = b *
                "Gets a value indicating whether this instance is representing a value of type " * attribute.DocCommentRef * ".") /
                 "public global::System.Boolean Is" * attribute.Names.SafeAlias * " => true;" /

                 (Docs.Summary, b => _ = b *
                 "Retrieves the value represented by this instance as a " * attribute.DocCommentRef * ".") /
                 "public " * attribute.Names.FullTypeName * " As" * attribute.Names.SafeAlias * " => " *
                 attribute.Storage.TypesafeInstanceVariableExpression % ';';
        }

        _ = builder % "#endregion";
    }
}
