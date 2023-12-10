namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

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

        public override void AppendConvertedInstanceVariableExpression(
            IExpandingMacroStringBuilder<Macro> builder,
            (String targetType, String instance) model,
            CancellationToken cancellationToken) =>
            builder.AppendUnsafeConvert(FullTypeName, model.targetType, $"{model.instance}.{_fieldName}", cancellationToken);
        public override void TypesafeInstanceVariableExpressionAppendix(
            IExpandingMacroStringBuilder<Macro> builder,
            String instance,
            CancellationToken cancellationToken) =>
            builder.Append('(')
                .Append(instance)
                .Append('.')
                .Append(_fieldName)
                .Append(')');
        public override void InstanceVariableAssignmentExpressionAppendix(
            IExpandingMacroStringBuilder<Macro> builder,
            (String valueExpression, String instance) model,
            CancellationToken cancellationToken) =>
            builder.Append(model.instance)
                .Append('.')
                .Append(_fieldName)
                .Append(" = ")
                .Append(model.valueExpression);
        public override void InstanceVariableExpressionAppendix(
            IExpandingMacroStringBuilder<Macro> builder,
            String instance,
            CancellationToken cancellationToken) => 
            TypesafeInstanceVariableExpressionAppendix(builder, instance, cancellationToken);

        public override void Visit(StrategySourceHost host) => host.AddDedicatedField(this);
    }
}
