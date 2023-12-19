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
        public override void ConvertedInstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String targetType,
            String instance) =>
            _ = builder * "((" * targetType * ')' * instance * ".__referenceTypeContainer)";
        public override void TypesafeInstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String instance) =>
            _ = builder * "((" * FullTypeName * ')' * instance * ".__referenceTypeContainer)";
        public override void InstanceVariableAssignmentExpression(
            ExpandingMacroBuilder builder,
            String valueExpression,
            String instance) =>
            _ = builder * instance * ".__referenceTypeContainer = " * valueExpression;
        public override void InstanceVariableExpression(
            ExpandingMacroBuilder builder,
            String instance) =>
            _ = builder * '(' * instance * ".__referenceTypeContainer)";
        public override void Visit(StrategySourceHost host) => host.AddReferenceTypeContainerField();
    }
}
