namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

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
        public override void ConvertedInstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String targetType,
            String instance) =>
            UtilUnsafeConvert(builder, FullTypeName, targetType, $"{instance}.__valueTypeContainer.{SafeAlias}");
        public override void TypesafeInstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String instance) =>
            _ = builder * '(' * instance * ".__valueTypeContainer." * SafeAlias * ')';
        public override void InstanceVariableAssignmentExpression(
            ExpandingMacroBuilder builder,
            String valueExpression,
            String instance) =>
            _ = builder * instance * ".__valueTypeContainer = new(" * valueExpression * ')';
        public override void InstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String instance) =>
            TypesafeInstanceVariableExpression(builder, instance);

        public override void Visit(StrategySourceHost host)
        {
            host.AddValueTypeContainerField();
            host.AddValueTypeContainerType();
            host.AddValueTypeVontainerInstanceFieldAndCtor(this);
        }
    }
}
