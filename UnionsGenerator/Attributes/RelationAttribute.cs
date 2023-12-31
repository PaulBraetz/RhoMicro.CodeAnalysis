﻿// <auto-generated>
// This file was generated using the RhoMicro.CodeAnalysis.UnionsGenerator.
// </auto-generated>
#nullable enable
#pragma warning disable

namespace RhoMicro.CodeAnalysis
{
    using System;

    /// <summary>
    /// Marks the target type to be related to another union type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
#if UNIONS_GENERATOR
    [GenerateFactory]
#endif
    sealed partial class RelationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="relatedType">
        /// The type to register as related to the target union type.
        /// </param>
        public RelationAttribute(Type relatedType)
        {
            RelatedType = relatedType;
        }

        /// <summary>
        /// Gets the type to register as related to the target union type.
        /// </summary>
        public Type RelatedType { get; }
    }
}
