namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;

using System;

using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;

partial class StorageStrategy
{
    private sealed class ValueContainerStrategy(
        SettingsModel settings,
        PartialRepresentableTypeModel representableType,
        StorageOption selectedOption,
        StorageSelectionViolation violation)
        : StorageStrategy(settings, representableType, selectedOption, violation)
    {
        public override StorageOption ActualOption => StorageOption.Value;
        public override IndentedStringBuilderAppendable ConvertedInstanceVariableExpression(
            String targetType,
            String instance) =>
            IndentedStringBuilder.Appendables.UtilUnsafeConvert(
                RepresentableType.Signature.Names.FullGenericNullableName,
                targetType,
                $"{instance}.{Settings.ValueTypeContainerName}.{RepresentableType.Alias}");
        public override IndentedStringBuilderAppendable StrongInstanceVariableExpression(
            String instance) =>
            new(b => _ = b.Operators + '(' + instance + '.' + Settings.ValueTypeContainerName + '.' + RepresentableType.Alias + ')');
        public override IndentedStringBuilderAppendable InstanceVariableAssignmentExpression(
            String valueExpression,
            String instance) =>
            new(b => _ = b.Operators + instance + '.' + Settings.ValueTypeContainerName + " = new(" + valueExpression + ')');
        public override IndentedStringBuilderAppendable InstanceVariableExpression(
            String instance) =>
            StrongInstanceVariableExpression(instance);

        public override void Visit(StrategySourceHost host)
        {
            host.AddValueTypeContainerField();
            host.AddValueTypeContainerType();
            host.AddValueTypeContainerInstanceFieldAndCtor(this);
        }
    }
}
