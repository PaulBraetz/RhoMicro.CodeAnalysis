﻿#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Analysis;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Analysis;
#endif

using System;

#if DSL_GENERATOR
[IncludeFile]
#endif
readonly record struct DiagnosticDescriptor(Int32 Id, String Title, String Message)
{
#if DSL_GENERATOR
    private const String _diagnosticsCategory = "RhoMicro.CodeAnalysis.DslGenerator";
    public Microsoft.CodeAnalysis.DiagnosticDescriptor ToMsDescriptor() =>
        new(
            $"DSLG{Id:0000}",
            Title,
            Message,
            _diagnosticsCategory,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Error,
            isEnabledByDefault: true);
#endif
}
