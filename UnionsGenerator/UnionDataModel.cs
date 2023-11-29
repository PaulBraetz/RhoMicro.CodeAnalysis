namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Net.Http;

internal abstract partial class UnionDataModel : IEquatable<UnionDataModel?>
{
    protected UnionDataModel(AnnotationDataModel annotations, OperatorOmissionModel operatorOmissions, INamedTypeSymbol symbol)
    {
        Annotations = annotations;
        OperatorOmissions = operatorOmissions;
        Symbol = symbol;
    }

    public readonly AnnotationDataModel Annotations;
    public readonly OperatorOmissionModel OperatorOmissions;
    public readonly INamedTypeSymbol Symbol;

    protected static (AnnotationDataModel Annotations, OperatorOmissionModel Omissions) CreateModels(INamedTypeSymbol targetSymbol)
    {
        var annotations = AnnotationDataModel.Create(targetSymbol);
        var omissions = OperatorOmissionModel.Create(targetSymbol, annotations);

        return (annotations, omissions);
    }

    public override Boolean Equals(Object? obj) =>
        Equals(obj as UnionDataModel);
    public Boolean Equals(UnionDataModel? other) =>
        other is not null &&
        Annotations.Equals(other.Annotations);
    public override Int32 GetHashCode() =>
        1792208713 + Annotations.GetHashCode();
}
