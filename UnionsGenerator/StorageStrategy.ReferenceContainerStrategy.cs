namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using System;

abstract partial class StorageStrategy
{
    sealed class ReferenceContainerStrategy(
        String safeAlias,
        String fullTypeName,
        StorageOption selectedOption,
        RepresentableTypeNature typeNature,
        StorageSelectionViolation violation)
        : StorageStrategy(safeAlias, fullTypeName, selectedOption, typeNature, violation)
    {
        public override String GetConvertedInstanceVariableExpression(String targetType, String instance = "this") =>
            $"(({targetType}){instance}.__referenceTypeContainer)";
        public override String GetInstanceVariableExpression(String instance = "this") =>
            $"(({FullTypeName}){instance}.__referenceTypeContainer)";
        public override String GetInstanceVariableAssignmentExpression(String valueExpression, String instance = "this") =>
            $"{instance}.__referenceTypeContainer = {valueExpression}";
        public override void Visit(StrategySourceHost host) => host.AddReferenceTypeContainerField();
    }
}
