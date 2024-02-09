namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System;

internal abstract partial class StorageStrategy
{
    private sealed class FieldContainerStrategy : StorageStrategy
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
            String instance) => throw new NotImplementedException();
        //only commented out because of breaking rewrite changes
            //UtilUnsafeConvert(builder, FullTypeName, targetType, $"{instance}.{_fieldName}");
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
