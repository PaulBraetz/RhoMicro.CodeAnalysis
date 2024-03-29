﻿namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;

using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;

partial class StorageStrategy
{
    private sealed class ReferenceContainerStrategy(
        SettingsModel settings,
        PartialRepresentableTypeModel representableType,
        StorageOption selectedOption,
        StorageSelectionViolation violation) : StorageStrategy(settings, representableType, selectedOption, violation)
    {
        public override StorageOption ActualOption { get; } = StorageOption.Reference;

        public override IndentedStringBuilderAppendable ConvertedInstanceVariableExpression(String targetType, String instance) =>
            new(b => b.UtilUnsafeConvert(RepresentableType.Signature.Names.FullGenericNullableName, targetType, $"({RepresentableType.Signature.Names.FullGenericNullableName}){instance}.{Settings.ReferenceTypeContainerName}{NullableFieldBang}"));
        public override IndentedStringBuilderAppendable InstanceVariableAssignmentExpression(String valueExpression, String instance) =>
            new(b => b.Append(instance).Append('.').Append(Settings.ReferenceTypeContainerName).Append(" = ").Append(valueExpression));
        public override IndentedStringBuilderAppendable InstanceVariableExpression(String instance) =>
            new(b => b.Append('(').Append(instance).Append('.').Append(Settings.ReferenceTypeContainerName).Append(NullableFieldBang).Append(')'));
        public override IndentedStringBuilderAppendable StrongInstanceVariableExpression(String instance) =>
            new(b => b.Append("((").Append(RepresentableType.Signature.Names.FullGenericNullableName).Append(')').Append(instance).Append('.').Append(Settings.ReferenceTypeContainerName).Append(NullableFieldBang).Append(')'));
        public override void Visit(StrategySourceHost host) => host.AddReferenceTypeContainerField();
    }
}