namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Immutable;

sealed partial class DiagnosticsModelBuilder
{
    private readonly ICollection<Diagnostic> _diagnostics = new List<Diagnostic>();
    private readonly Object _syncRoot = new();

    public Boolean IsError { get; private set; }

    private Boolean _reportDiagnostics;

    private DiagnosticsModelBuilder(ICollection<Diagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
        IsError = diagnostics.Any(d => d.IsError());
    }

    public DiagnosticsModelBuilder() : this(new List<Diagnostic>())
    {

    }

    public DiagnosticsModelBuilder ReportDiagnostics()
    {
        _reportDiagnostics = true;
        return this;
    }

    #region Auto Diagnosers
    private void DiagnoseBidirectionalRelation(TargetDataModel model)
    {
        var bidirectionalRelationNames = model.Annotations.Relations
            .Select(r => r.ExtractData(model))
            .Where(r => r.Annotations.Relations.Any(rr => SymbolEqualityComparer.Default.Equals(rr.RelatedTypeSymbol, model.Symbol)))
            .Select(r => r.Symbol.Name);

        if(!bidirectionalRelationNames.Any())
        {
            return;
        }

        var location = model.TargetDeclaration.GetLocation();
        foreach(var name in bidirectionalRelationNames)
        {
            var diagnostic = Diagnostics.BidirectionalRelation(location, name);
            _ = Add(diagnostic);
        }
    }
    private void DiagnoseDupicateRelation(TargetDataModel model)
    {
        var duplicateRelationNames = model.Annotations.Relations
            .GroupBy(r => r.RelatedTypeSymbol, SymbolEqualityComparer.Default)
            .Select(g => g.ToArray())
            .Where(g => g.Length > 1)
            .Select(g => g[0].RelatedTypeSymbol.Name);

        if(!duplicateRelationNames.Any())
        {
            return;
        }

        var location = model.TargetDeclaration.GetLocation();
        foreach(var name in duplicateRelationNames)
        {
            var diagnostic = Diagnostics.DuplicateRelation(location, name);
            _ = Add(diagnostic);
        }
    }
    private void DiagnoseGenericRelation(TargetDataModel model)
    {
        var target = model.Symbol;
        var relations = model.Annotations.Relations
            .Where(r => r.RelatedTypeSymbol.IsGenericType);

        if(!(target.IsGenericType && relations.Any()))
        {
            return;
        }

        var location = model.TargetDeclaration.GetLocation();
        var diagnostic = Diagnostics.GenericRelation(location);
        _ = Add(diagnostic);
    }
    private void DiagnoseStorageSelectionViolations(TargetDataModel model)
    {
        var violations = model.Annotations.AllRepresentableTypes
            .Where(d => d.Storage.Violation != StorageSelectionViolation.None);

        if(!violations.Any())
        {
            return;
        }

        var location = model.TargetDeclaration.GetLocation();
        foreach(var violation in violations)
        {
            var name = violation.Names.FullTypeName;
            var diagnostic = (violation.Storage.Violation switch
            {
                StorageSelectionViolation.PureValueReferenceSelection =>
                    (Func<Location, String, Diagnostic>)Diagnostics.BoxingStrategy,
                StorageSelectionViolation.PureValueValueSelectionGeneric =>
                    Diagnostics.GenericViolationStrategy,
                StorageSelectionViolation.ImpureValueReference =>
                    Diagnostics.BoxingStrategy,
                StorageSelectionViolation.ImpureValueValue =>
                    Diagnostics.TleStrategy,
                StorageSelectionViolation.ReferenceValue =>
                    Diagnostics.TleStrategy,
                StorageSelectionViolation.UnknownReference =>
                    Diagnostics.PossibleBoxingStrategy,
                StorageSelectionViolation.UnknownValue =>
                    Diagnostics.PossibleTleStrategy,
                _ => null
            })?.Invoke(location, name);

            if(diagnostic != null)
            {
                _ = Add(diagnostic);
            }
        }
    }
    private void DiagnoseSmallGenericUnion(TargetDataModel model)
    {
        if(!model.Symbol.IsGenericType ||
            model.Annotations.Settings.Layout != LayoutSetting.Small)
        {
            return;
        }

        var location = model.TargetDeclaration.GetLocation();
        var diagnostic = Diagnostics.SmallGenericUnion(location);
        _ = Add(diagnostic);
    }
    private void DiagnoseUnknownGenericParameterName(TargetDataModel model)
    {
        var available = model.Symbol.TypeParameters
            .Select(p => p.Name)
            .ToImmutableHashSet();
        var unknowns = model.Annotations.AllRepresentableTypes
            .Where(a => a.Attribute.RepresentableTypeIsGenericParameter)
            .Where(a => !available.Contains(a.Names.SimpleTypeName))
            .Select(a => a.Names.SimpleTypeName)
            .ToArray();

        if(unknowns.Length == 0)
        {
            return;
        }

        var location = model.TargetDeclaration.GetLocation();

        foreach(var unknown in unknowns)
        {
            var diagnostic = Diagnostics.UnknownGenericParameterName(location, unknown);
            _ = Add(diagnostic);
        }
    }
    private void DiagnoseReservedGenericParameterName(TargetDataModel model)
    {
        var collisions = model.Symbol.TypeParameters
            .Select(p => p.Name)
            .Where(model.Annotations.Settings.IsReservedGenericTypeName)
            .ToArray();

        if(collisions.Length == 0)
        {
            return;
        }

        var location = model.TargetDeclaration.GetLocation();

        foreach(var collision in collisions)
        {
            var diagnostic = Diagnostics.ReservedGenericParameterName(location, collision);
            _ = Add(diagnostic);
        }
    }
    private void DiagnoseOperatorOmissions(TargetDataModel model)
    {
        var omissions = model.OperatorOmissions;
        var location = model.TargetDeclaration.GetLocation();

        foreach(var interfaceOmission in omissions.Interfaces)
        {
            var diagnostic = Diagnostics.RepresentableTypeIsInterface(
                location,
                interfaceOmission.Names.SimpleTypeName);
            _ = Add(diagnostic);
        }

        foreach(var supertypes in omissions.Supertypes)
        {
            var diagnostic = Diagnostics.RepresentableTypeIsSupertype(
                location,
                supertypes.Names.SimpleTypeName);
            _ = Add(diagnostic);
        }
    }
    private void DiagnoseUnionTypeSettingsOnNonUnionType(TargetDataModel model)
    {
        var representableTypes = model.Annotations.AllRepresentableTypes;

        if(representableTypes.Count > 0 ||
           !model.Symbol
            .GetAttributes()
            .OfUnionTypeSettingsAttribute()
            .Any())
        {
            return;
        }

        var location = model.TargetDeclaration.GetLocation();
        var diagnostics = Diagnostics.UnionTypeSettingsOnNonUnionType(location);
        _ = Add(diagnostics);
    }
    private void DiagnoseImplicitConversionIfSolitary(TargetDataModel model)
    {
        var attributes = model.Annotations.AllRepresentableTypes;
        var location = model.TargetDeclaration.GetLocation();

        if(attributes.Count == 1 &&
           attributes[0].Attribute.Options.HasFlag(UnionTypeOptions.ImplicitConversionIfSolitary))
        {
            var diagnostic = Diagnostics.ImplicitConversionOptionOnSolitary(
                model.Symbol.Name,
                attributes[0].Names.SimpleTypeName,
                location);
            _ = Add(diagnostic);
        } else if(attributes.Count > 1 &&
           attributes.Any(a => a.Attribute.Options.HasFlag(UnionTypeOptions.ImplicitConversionIfSolitary)))
        {
            var diagnostic = Diagnostics.ImplicitConversionOptionOnNonSolitary(location);
            _ = Add(diagnostic);
        }
    }
    private void DiagnoseUnionTypeCount(TargetDataModel model)
    {
        var count = model.Annotations.AllRepresentableTypes.Count;
        if(count <= Byte.MaxValue)
            return;

        var location = model.TargetDeclaration.GetLocation();
        var diagnostics = Diagnostics.TooManyTypes(location);
        _ = Add(diagnostics);
    }
    private void DiagnosePartiality(TargetDataModel model)
    {
        if(model.TargetDeclaration.IsPartial())
            return;

        var location = model.TargetDeclaration.Identifier.GetLocation();
        var diagnostics = Diagnostics.NonPartialDeclaration(location);
        _ = Add(diagnostics);
    }
    private void DiagnoseNonStatic(TargetDataModel model)
    {
        if(!model.TargetDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            return;

        var location = model.TargetDeclaration.Identifier.GetLocation();
        var diagnostics = Diagnostics.StaticTarget(location);
        _ = Add(diagnostics);
    }
    private void DiagnoseNonRecord(TargetDataModel model)
    {
        if(!model.TargetDeclaration.IsKind(SyntaxKind.RecordDeclaration) &&
            !model.TargetDeclaration.IsKind(SyntaxKind.RecordStructDeclaration))
        {
            return;
        }

        var location = model.TargetDeclaration.Identifier.GetLocation();
        var diagnostics = Diagnostics.RecordTarget(location);
        _ = Add(diagnostics);
    }
    private void DiagnoseUnionTypeAttribute(TargetDataModel model)
    {
        if(model.Annotations.AllRepresentableTypes.Count > 0)
            return;

        var location = model.TargetDeclaration.Identifier.GetLocation();
        var diagnostics = Diagnostics.MissingUnionTypeAttribute(location);
        _ = Add(diagnostics);
    }
    private void DiagnoseUniqueUnionTypeAttributes(TargetDataModel model)
    {
        _ = model.Annotations.AllRepresentableTypes
            .GroupBy(t => t.Names.FullTypeName)
            .Select(g => (Name: g.Key, Locations: g.Select(t => model.TargetDeclaration.GetLocation()).ToArray()))
            .Where(t => t.Locations.Length > 1)
            .SelectMany(t => t.Locations.Skip(1).Select(l => Diagnostics.DuplicateUnionTypeAttributes(t.Name, l)))
            .Aggregate(this, (b, d) => b.Add(d));
    }
    private void DiagnoseAliasCollisions(TargetDataModel parameters)
    {
        var duplicates = parameters.Annotations.AllRepresentableTypes
            .GroupBy(a => a.Names.SafeAlias)
            .Where(g => g.Skip(1).Any())
            .Select(g => g.First().Names.SimpleTypeName);
        if(!duplicates.Any())
        {
            return;
        }

        var location = parameters.TargetDeclaration.GetLocation();
        _ = duplicates.Select(d => Diagnostics.AliasCollision(location, d))
            .Aggregate(this, (b, d) => b.Add(d));
    }
    #endregion

    private static readonly IEnumerable<Action<DiagnosticsModelBuilder, TargetDataModel, CancellationToken>> _diagnosers = GetDiagnosers();
    private static List<Action<DiagnosticsModelBuilder, TargetDataModel, CancellationToken>> GetDiagnosers()
    {
        var result = typeof(DiagnosticsModelBuilder)
                .GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly |
                    BindingFlags.NonPublic)
                .Where(m => m.ReturnType == typeof(void))
                .Where(m =>
                {
                    var parameters = m.GetParameters();
                    var result = parameters.Length == 1 &&
                        parameters[0].ParameterType == typeof(TargetDataModel);

                    return result;
                })
                .Select(diagnoseMethod =>
                {
                    var instanceParam = Expression.Parameter(typeof(DiagnosticsModelBuilder));
                    var parametersParam = Expression.Parameter(typeof(TargetDataModel));
                    var tokenParam = Expression.Parameter(typeof(CancellationToken));

                    var throwMethod = typeof(CancellationToken).GetMethod(nameof(CancellationToken.ThrowIfCancellationRequested));
                    var throwExpr = Expression.Call(tokenParam, throwMethod);

                    var callExpr = Expression.Call(instanceParam, diagnoseMethod, parametersParam);

                    var body = Expression.Block(throwExpr, callExpr);

                    var lambda = Expression.Lambda(body, instanceParam, parametersParam, tokenParam);
                    var result = lambda.Compile();

                    return result;
                })
                .Cast<Action<DiagnosticsModelBuilder, TargetDataModel, CancellationToken>>()
                .ToList();

        return result;
    }
    internal DiagnosticsModelBuilder Diagnose(TargetDataModel parameters, CancellationToken token)
    {
        foreach(var d in _diagnosers)
            d.Invoke(this, parameters, token);

        return this;
    }

    public DiagnosticsModelBuilder Add(Diagnostic diagnostic)
    {
        lock(_syncRoot)
        {
            if(diagnostic.IsError())
            {
                IsError = true;
            }

            _diagnostics.Add(diagnostic);
        }

        return this;
    }
    public DiagnosticsModelBuilder Clone()
    {
        ICollection<Diagnostic> diagnostics;
        lock(_syncRoot)
        {
            diagnostics = _diagnostics.ToList();
        }

        var result = new DiagnosticsModelBuilder(diagnostics);
        if(_reportDiagnostics)
        {
            _ = result.ReportDiagnostics();
        }

        return result;
    }

    public Model Build(UnionTypeSettingsAttribute settings)
    {
        if(!_reportDiagnostics)
        {
            return IsError ?
                Model.Error :
                Model.NoError;
        }

        IEnumerable<Diagnostic> reportableDiagnostics;
        lock(_syncRoot)
        {
            reportableDiagnostics = _diagnostics
                .Where(d => d.ShouldReport(settings.DiagnosticsLevel))
                .ToList();
        }

        return new Model(reportableDiagnostics);
    }
}
