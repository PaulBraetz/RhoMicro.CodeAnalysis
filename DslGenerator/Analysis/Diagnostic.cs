#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Analysis;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Analysis;
#endif

using System;

#if DSL_GENERATOR
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

#if DSL_GENERATOR
[IncludeFile]
#endif
sealed record Diagnostic(DiagnosticDescriptor Descriptor, Location Location, IReadOnlyCollection<Object> MessageArgs)
{
    public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, params Object[] messageArgs) =>
        new(descriptor, location, messageArgs);
#if DSL_GENERATOR
    public Microsoft.CodeAnalysis.Diagnostic ToMsDiagnostic() =>
        Microsoft.CodeAnalysis.Diagnostic.Create(
            Descriptor.ToMsDescriptor(),
            Location.ToMsLocation(),
            MessageArgs.ToArray());
#endif
}
