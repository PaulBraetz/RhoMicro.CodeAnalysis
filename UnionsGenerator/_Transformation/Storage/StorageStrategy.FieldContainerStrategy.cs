namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models.Storage;

using RhoMicro.CodeAnalysis.Library.Text;
using static RhoMicro.CodeAnalysis.Library.Text.IndentedStringBuilder.Appendables;

using System;
using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Storage;

abstract partial class StorageStrategy
{
    private sealed class FieldContainerStrategy(
        UnionTypeModel targetType,
        RepresentableTypeModel unionTypeAttribute,
        StorageOption selectedOption,
        StorageSelectionViolation violation)
        : StorageStrategy(targetType, unionTypeAttribute, selectedOption, violation)
    {
        public override StorageOption ActualOption => StorageOption.Field;
        public override IndentedStringBuilderAppendable ConvertedInstanceVariableExpression(
            String targetType,
            String instance) =>
            UtilUnsafeConvert(RepresentableType.Signature.Names.FullGenericName, targetType, $"{instance}.{FieldName}{NullableFieldBang}");
        public override IndentedStringBuilderAppendable StrongInstanceVariableExpression(
            String instance) =>
            new(b => _ = b.Operators + '(' + instance + '.' + FieldName + NullableFieldBang + ')');
        public override IndentedStringBuilderAppendable InstanceVariableAssignmentExpression(
            String valueExpression,
            String instance) =>
            new(b => _ = b.Operators + instance + '.' + FieldName + " = " + valueExpression);
        public override IndentedStringBuilderAppendable InstanceVariableExpression(
            String instance) =>
            StrongInstanceVariableExpression(instance);

        public override void Visit(StrategySourceHost host) => host.AddDedicatedField(this);
    }
}
