namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using System;

abstract partial class StorageStrategy
{
    sealed class ValueContainerStrategy(
        String safeAlias,
        String fullTypeName,
        StorageOption selectedOption,
        RepresentableTypeNature typeNature,
        StorageSelectionViolation violation)
        : StorageStrategy(safeAlias, fullTypeName, selectedOption, typeNature, violation)
    {
        public override String GetConvertedInstanceVariableExpression(String targetType, String instance = "this") =>
            ConstantSources.UnsafeConvert(FullTypeName, targetType, $"{instance}.__valueTypeContainer.{SafeAlias}");
        public override String GetInstanceVariableExpression(String instance = "this") =>
           $"({instance}.__valueTypeContainer.{SafeAlias})";
        public override String GetInstanceVariableAssignmentExpression(String valueExpression, String instance = "this") =>
            $"{instance}.__valueTypeContainer = new({valueExpression})";
        public override void Visit(StrategySourceHost host)
        {
            host.AddValueTypeContainerField();
            host.AddValueTypeContainerType();
            host.AddValueTypeVontainerInstanceFieldAndCtor(this);
        }
    }
}
