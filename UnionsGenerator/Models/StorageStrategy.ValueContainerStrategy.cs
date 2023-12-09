namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

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
        public override void AppendConvertedInstanceVariableExpression(
            IExpandingMacroStringBuilder<Macro> builder,
            (String targetType, String instance) model,
            CancellationToken cancellationToken) =>
            builder.AppendUnsafeConvert(FullTypeName, model.targetType, $"{model.instance}.__valueTypeContainer.{SafeAlias}", cancellationToken);
        public override void InstanceVariableExpressionAppendix(
            IExpandingMacroStringBuilder<Macro> builder,
            String instance,
            CancellationToken cancellationToken) =>
            builder.Append('(').Append(instance).Append(".__valueTypeContainer.").Append(SafeAlias).Append(')');
        public override void InstanceVariableAssignmentExpressionAppendix(
            IExpandingMacroStringBuilder<Macro> builder,
            (String valueExpression, String instance) model,
            CancellationToken cancellationToken) => 
            builder.Append(model.instance).Append(".__valueTypeContainer = new(").Append(model.valueExpression).Append(')');
        
        public override void Visit(StrategySourceHost host)
        {
            host.AddValueTypeContainerField();
            host.AddValueTypeContainerType();
            host.AddValueTypeVontainerInstanceFieldAndCtor(this);
        }
    }
}
