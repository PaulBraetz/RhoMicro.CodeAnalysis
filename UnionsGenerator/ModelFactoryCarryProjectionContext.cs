namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using System.Threading;

readonly struct ModelCreationContext(TargetDataModel parameters, CancellationToken cancellationToken)
{
    public readonly TargetDataModel TargetData = parameters;
    public readonly CancellationToken CancellationToken = cancellationToken;
}
