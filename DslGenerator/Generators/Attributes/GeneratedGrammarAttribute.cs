namespace RhoMicro.CodeAnalysis;

using System;

#if DSL_GENERATOR
[GenerateFactory]
#endif
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
sealed partial class GeneratedGrammarAttribute : Attribute;
