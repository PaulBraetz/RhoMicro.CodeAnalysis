namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Reflection;
using System.Threading;

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
        public override void AppendConvertedInstanceVariableExpression(
            IExpandingMacroStringBuilder<Macro> builder,
            (String targetType, String instance) model,
            CancellationToken cancellationToken) =>
            builder.Append("((").Append(model.targetType).Append(')').Append(model.instance).Append(".__referenceTypeContainer)");
        public override void TypesafeInstanceVariableExpressionAppendix(
            IExpandingMacroStringBuilder<Macro> builder,
            String instance,
            CancellationToken cancellationToken) =>
            builder.Append("((").Append(FullTypeName).Append(')').Append(instance).Append(".__referenceTypeContainer)");
        public override void InstanceVariableAssignmentExpressionAppendix(
            IExpandingMacroStringBuilder<Macro> builder,
            (String valueExpression, String instance) model,
            CancellationToken cancellationToken) =>
            builder.Append(model.instance).Append(".__referenceTypeContainer = ").Append(model.valueExpression);
        public override void InstanceVariableExpressionAppendix(
            IExpandingMacroStringBuilder<Macro> builder,
            String instance,
            CancellationToken cancellationToken) =>
            builder.Append('(').Append(instance).Append(".__referenceTypeContainer)");
        public override void Visit(StrategySourceHost host) => host.AddReferenceTypeContainerField();
    }
}
