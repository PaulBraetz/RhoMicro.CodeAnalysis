namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class IsAsProperties(TargetDataModel model) : ExpansionBase(model, Macro.IsAsProperties)
{
    public override void Expand(ExpandingMacroBuilder builder)
    {
        var attributes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder.AppendLine("#region IsAsProperties");
#pragma warning restore IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder.AppendJoin(
                 attributes,
                 (b, a, t) => b.AppendLine("/// <summary>") *
                 "/// Gets a value indicating whether this instance is representing a value of type "
                 .Append(a.DocCommentRef).AppendLine('.') /
                 "/// </summary>" *
                 "public global::System.Boolean ".Append(a.Names.IsPropertyName) *
                 " => __tag == ".Append(a.CorrespondingTag).AppendLine(';') /
                 "/// <summary>" *
                 "/// Attempts to retrieve the value represented by this instance as a "
                 .Append(a.DocCommentRef).AppendLine('.') /
                 "/// </summary>" *
                 "/// <exception cref=\"global::System.InvalidOperationException\">Thrown if the instance is not representing a value of type "
                 .Append(a.DocCommentRef).AppendLine(".</exception>") *
                 "public ".AppendFull(a)
                 .Append(' ').Append(a.Names.AsPropertyName) *
                 " => __tag == ".Append(a.CorrespondingTag).Append('?')
                 .Append(a.Storage.TypesafeInstanceVariableExpressionAppendix, t)
                 .Append(':')
                 .AppendInvalidConversionThrow($"typeof({a.Names.FullTypeName}).Name", t).AppendLine(';'),
                 cancellationToken);
        } else
        {
            var attribute = attributes[0];

            _ = builder.AppendLine("/// <summary>") *
                 "/// Gets a value indicating whether this instance is representing a value of type <c>"
                 .Append(attribute.DocCommentRef).AppendLine("</c>.") /
                 "/// </summary>" *
                 "public global::System.Boolean Is".Append(attribute.Names.SafeAlias) /
                 " => true;" /

                 "/// <summary>" *
                 "/// Retrieves the value represented by this instance as a <c>"
                 .Append(attribute.DocCommentRef).AppendLine("</c>.") /
                 "/// </summary>" *
                 "public ".AppendFull(attribute) *
                 " As".Append(attribute.Names.SafeAlias) *
                 " => ".Append(attribute.Storage.TypesafeInstanceVariableExpressionAppendix, cancellationToken).AppendLine(';');
        }

        _ = builder.AppendLine("#endregion");
    }
}
