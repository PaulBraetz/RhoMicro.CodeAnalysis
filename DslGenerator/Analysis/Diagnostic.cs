namespace RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using System;

using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

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
