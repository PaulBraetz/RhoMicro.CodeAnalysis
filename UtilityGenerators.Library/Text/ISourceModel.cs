namespace RhoMicro.CodeAnalysis.Library.Text;
interface ISourceModel
{
    SourceModelResult GetSourceResult(CancellationToken cancellationToken);
}
