namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

internal sealed class NestedTypes(TargetDataModel model)
    : ExpansionBase(model, Macro.NestedClasses)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        //commented out because of breaking rewrite changes
        return;
        _ = builder /
            "#region Nested Types" /
            SerializationConverter;

        var representableTypes = Model.Annotations.AllRepresentableTypes;
        if(representableTypes.Count > 1)
        {
            _ = (builder *
                (Docs.Summary, b => _ = b * "Defines tags to discriminate between representable types.") *
                ConstantSources.EditorBrowsableNever /
                "enum " * Model.TagTypeName % " : Byte {")
                .AppendJoin(
                    ',',
                    representableTypes,
                    (b, a, t) => b.WithOperators(t) *
                        (Docs.Summary, b => _ = b *
                        "Used when representing an instance of " * a.DocCommentRef * '.') *
                        a.Names.SafeAlias)
                .Append('}');
        }

        var host = new StrategySourceHost(Model);
        representableTypes.ForEach(t => t.Storage.Visit(host));

        _ = builder *
            host.ValueTypeContainerType %
            "#endregion";
    }

    private void SerializationConverter(ExpandingMacroStringBuilder.OperatorsDecorator<Macro, TargetDataModel> builder)
    {
        //commented out because of breaking rewrite changes
        return;
        //if(!Model.Annotations.Settings.GenerateJsonConverter.Value)
        //    return;

        //_ = builder *
        //    (Docs.Summary, b => _ = b * "Implements json conversion logic for the " * (b => Extensions.CommentRef(Model, b)) * " type.") /
        //    "public sealed class JsonConverter : System.Text.Json.Serialization.JsonConverter<" * Model.Symbol.ToMinimalOpenString() * '>' /
        //    '{' /
        //    "sealed class Dto" /
        //    '{' /
        //    "public static Dto Create(" * Model.Symbol.ToMinimalOpenString() * " value) => new()" /
        //    '{' /
        //    "RepresentedType = value.RepresentedType.Names.FullGenericName!," /
        //    "RepresentedValue = " * "value.__tag switch {" *
        //        (b => b.AppendJoin(
        //            Model.Annotations.AllRepresentableTypes,
        //            (b, a, t) => b.WithOperators(t) *
        //                a.GetCorrespondingTag(Model) * " => " * (a.Storage.InstanceVariableExpression, "value") * ',',
        //        b.CancellationToken)) *
        //        "_ => " * ConstantSources.InvalidTagStateThrow /
        //        "}" /
        //    "};" /
        //    "public " * Model.Symbol.ToMinimalOpenString() * " Reconstitute() =>" /
        //    "RepresentedType switch {" *
        //        (b => b.AppendJoin(
        //            Model.Annotations.AllRepresentableTypes,
        //            (b, a, t) =>
        //            {
        //                var builder = b.WithOperators(t);
        //                _ = builder *
        //                '"' * a.Names.TypeStringName * "\" => " * a.Factory.Name * '(';

        //                _ = a.Nature == RepresentableTypeNature.ReferenceType ?
        //                    builder * "RepresentedValue != null ? System.Text.Json.JsonSerializer.Deserialize<" *
        //                        a.Names.FullTypeName * ">((System.Text.Json.JsonElement)RepresentedValue) : (" *
        //                        a.Names.FullTypeName * ")RepresentedValue" :
        //                    builder * "System.Text.Json.JsonSerializer.Deserialize<" *
        //                        a.Names.FullTypeName * ">((System.Text.Json.JsonElement)RepresentedValue)";

        //                _ = builder * "),";

        //                return builder;
        //            },
        //        b.CancellationToken)) *
        //        "_ => throw new System.Text.Json.JsonException($\"Unable to deserialize a union instance representing an instance of {RepresentedType} as an instance of " * Model.Symbol.ToFullOpenString() * "\")" /
        //        "};" /
        //    "public required String RepresentedType { get; set; }" /
        //    "public required Object RepresentedValue { get; set; }" /
        //    '}' /
        //    "public override " * Model.Symbol.ToMinimalOpenString() * (Model.Symbol.IsReferenceType ? "?" : String.Empty) * " Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options) => System.Text.Json.JsonSerializer.Deserialize<Dto>(ref reader, options)?.Reconstitute();" /
        //    "public override void Write(System.Text.Json.Utf8JsonWriter writer, " * Model.Symbol.ToMinimalOpenString() * " value, System.Text.Json.JsonSerializerOptions options) => System.Text.Json.JsonSerializer.Serialize(writer, Dto.Create(value), options);" /
        //    '}';
    }
}
