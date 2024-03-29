﻿namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract partial record Rule
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Any : Rule
    {
        public static Any Instance { get; } = new();
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.Append('.');
        }

        protected override void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken) =>
            cancellationToken.ThrowIfCancellationRequested();
        public override void Receive(SyntaxNodeVisitor visitor) => visitor.Visit(this);
    }
}
