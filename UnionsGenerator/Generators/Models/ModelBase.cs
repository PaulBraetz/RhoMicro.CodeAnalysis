namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.Library.Text;

using System;
using System.Collections.Generic;
using System.Threading;

abstract class ModelBase : ISourceModel, IIndentedStringBuilderAppendable
{
    protected ModelBase(INamedTypeSymbol target) => Target = PartialClassModel.Create(target);

    protected PartialClassModel Target { get; }

    public virtual void AppendTo(IndentedStringBuilder builder, CancellationToken cancellationToken) =>
        cancellationToken.ThrowIfCancellationRequested();
    public SourceModelResult GetSourceResult(CancellationToken cancellationToken)
    {
        var options = IndentedStringBuilderOptions.GeneratedFile with
        {
            GeneratorName = "RhoMicro.CodeAnalysis.UnionsGenerator"
        };
        var sourceText = new IndentedStringBuilder(options).Append(this, cancellationToken).ToString();
        var hint = Target.Name;
        var result = SourceModelResult.Create(hint, sourceText);

        return result;
    }
}
