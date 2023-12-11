﻿namespace RhoMicro.CodeAnalysis.UtilityGenerators;

using System;

public sealed partial class AttributeFactoryGenerator
{
    private const String _sourceTemplate =
""""
// <auto-generated>
// This file has been auto generated using the RhoMicro.CodeAnalysis.AttributeFactoryGenerator.
// </auto-generated>

#nullable enable
#pragma warning disable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace {NAMESPACE}
{
    {ACCESSIBILITY} partial class {NAME}{GENERICPARAMLIST}
    {
        {CONTAINERS}

        {SYMBOLS}

        /// <summary>
        /// Attempts to create an instance of <see cref="{NAME}"/> based on an 
        /// instance of <see cref="AttributeData"/>.
        /// </summary>
        /// <param name="data">The attribute data to try and create an instance from.</param>
        /// <param name="result">
        /// Will contain the created instance, if one could be created; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an instance could be created; otherwise, <see langword="false"/>.
        /// </returns>
        public static Boolean TryCreate(AttributeData data, out {NAME}{GENERICPARAMLIST}? result)
        {
            result = null;

            if(data.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != "global::{NAMESPACE}.{NAME}")
            {
                return false;
            }

            var ctorArgs = data.ConstructorArguments;
            
            switch(ctorArgs.Length)
            {
                {CTORCASES}
            }

            var propArgs = data.NamedArguments;
            foreach(var propArg in propArgs)
            {
                switch(propArg.Key)
                {
                    {PROPCASES}
                    default:
                        return false;
                }
            }

            return true;

            static Object[] getValues(TypedConstant constant) =>
                constant.Value != null ?
                    new Object[] { constant.Value } :
                    constant.IsNull ?
                    null :
                    constant.Values.Select(getValues).ToArray();
        }

        public const String MetadataName = "{METADATANAME}";
        public const String SourceText = 
{SOURCETEXT};
    }

    /// <summary>
    /// Contains extension methods pertaining to the <see cref="{NAME}"/> type.
    /// </summary>
    {ACCESSIBILITY} static class {NAME}Extensions
    {
        /// <summary>
        /// Filters and projects an enumeration of <see cref="AttributeData"/> onto 
        /// instances of <see cref="{NAME}{GENERICPARAMCOMMENT}"/>.
        /// </summary>
        /// <param name="data">The attribute data to filter.</param>
        /// <returns>The filtered and projected data.</returns>
        public static IEnumerable<{NAME}{GENERICPARAMLIST}> Of{NAME}{GENERICPARAMLIST}(this IEnumerable<AttributeData> data) =>
            data.Select(d => (Success: {NAME}{GENERICPARAMLIST}.TryCreate(d, out var a), Attribute:a))
                .Where(t=>t.Success)
                .Select(t=>t.Attribute);

        /// <summary>
        /// Attempts to retrieve the first instance of <see cref="{NAME}{GENERICPARAMCOMMENT}"/> from a symbol.
        /// </summary>
        /// <param name="symbol">The symbol whose attributes to scan for an instance of <see cref="{NAME}{GENERICPARAMCOMMENT}"/>.</param>
        /// <param name="attribute">The attribute retrieved, if one could be located; otherwise, <see langword="null"/>.</param>
        public static Boolean TryGetFirst{NAME}{GENERICPARAMLIST}(this ISymbol symbol, out {NAME}{GENERICPARAMLIST} attribute)
        {
            attribute = symbol.GetAttributes().Of{NAME}{GENERICPARAMLIST}().FirstOrDefault();
            return attribute != null;
        }

        public static IncrementalValuesProvider<T> For{NAME}{GENERICPARAMNAMES}<T>(this SyntaxValueProvider provider, Func<SyntaxNode, CancellationToken, bool> predicate, Func<GeneratorAttributeSyntaxContext, CancellationToken, T> transform) =>
            provider.ForAttributeWithMetadataName("{METADATANAME}", predicate, transform);
    }
}
"""";
    private const String _genericParamLisTMacro = "{GENERICPARAMLIST}";
    private const String _genericParamNamesPlaceholder = "{GENERICPARAMNAMES}";
    private const String _genericParamCommenTMacro = "{GENERICPARAMCOMMENT}";
    private const String _targetCtorCasesPlaceholder = "{CTORCASES}";
    private const String _targetPropCasesPlaceholder = "{PROPCASES}";
    private const String _targetNamePlaceholder = "{NAME}";
    private const String _targetMetadataNamePlaceholder = "{METADATANAME}";
    private const String _targetContainersPlaceholder = "{CONTAINERS}";
    private const String _targetSymbolsPlaceholder = "{SYMBOLS}";
    private const String _targetAccessibilityPlaceholder = "{ACCESSIBILITY}";
    private const String _targetNamespacePlaceholder = "{NAMESPACE}";
    private const String _sourceTexTMacro = "{SOURCETEXT}";

    private const String _generateFactoryAttributeName = "GenerateFactoryAttribute";
    private const String _excludeConstructorAttributeName = "ExcludeFromFactoryAttribute";
    private const String _attributeNamespace = "RhoMicro.CodeAnalysis";
    private const String _generateFactoryAttributeFullyQualifiedName = _attributeNamespace + "." + _generateFactoryAttributeName;
    private const String _attributesHint = "Attributes.g.cs";
    private const String _attributeSource =
"""
// <generated>
// This file has been auto generated using the RhoMicro.CodeAnalysis.AttributeFactoryGenerator.
// </generated>

#pragma warning disable
using System;
namespace RhoMicro.CodeAnalysis
{
    /// <summary>
    /// Marks the target type for factory generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class GenerateFactoryAttribute : Attribute { }
    /// <summary>
    /// Marks the target constructor to be excluded from factory instantiation attempts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class ExcludeFromFactoryAttribute : Attribute { }
}
""";
}
