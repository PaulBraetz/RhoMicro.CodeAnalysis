namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Net.Http;

internal abstract partial class UnionDataModel
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
}
