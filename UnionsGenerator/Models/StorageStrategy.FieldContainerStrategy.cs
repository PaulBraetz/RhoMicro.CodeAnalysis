namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

abstract partial class StorageStrategy
{
    sealed class FieldContainerStrategy : StorageStrategy
    {
        public FieldContainerStrategy(
        String safeAlias,
        String fullTypeName,
        StorageOption selectedOption,
        RepresentableTypeNature typeNature,
        StorageSelectionViolation violation)
        : base(safeAlias, fullTypeName, selectedOption, typeNature, violation) =>
            _fieldName = SafeAlias.ToGeneratedCamelCase();

        private readonly String _fieldName;

        public override void ConvertedInstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String targetType,
            String instance) =>
            Extensions.UtilUnsafeConvert(builder, FullTypeName, targetType, $"{instance}.{_fieldName}");
        public override void TypesafeInstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String instance) =>
            _ = builder * '(' * instance * '.' * _fieldName * ')';
        public override void InstanceVariableAssignmentExpression(
            ExpandingMacroBuilder builder,
            String valueExpression,
            String instance) =>
            _ = builder * instance * '.' * _fieldName * " = " * valueExpression;
        public override void InstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String instance) =>
            TypesafeInstanceVariableExpression(builder, instance);

        public override void Visit(StrategySourceHost host) => host.AddDedicatedField(this);
    }
}
