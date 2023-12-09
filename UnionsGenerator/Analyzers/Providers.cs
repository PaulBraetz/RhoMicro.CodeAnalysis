namespace RhoMicro.CodeAnalysis.UnionsGenerator.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal static class Providers
{
    public static readonly IDiagnosticProvider<TargetDataModel> BidirectionalRelation =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            var bidirectionalRelationNames = model.Annotations.Relations
                .Select(r => r.ExtractData(model))
                .Where(r => r.Annotations.Relations.Any(rr => SymbolEqualityComparer.Default.Equals(rr.RelatedTypeSymbol, model.Symbol)))
                .Select(r => r.Symbol.Name);

            if(!bidirectionalRelationNames.Any())
                return;

            var location = model.TargetDeclaration.GetLocation();
            foreach(var name in bidirectionalRelationNames)
            {
                var diagnostic = Diagnostics.BidirectionalRelation(location, name);
                _ = diagnostics.Add(diagnostic);
            }
        });
    public static readonly IDiagnosticProvider<TargetDataModel> DuplicateRelation =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            var duplicateRelationNames = model.Annotations.Relations
            .GroupBy(r => r.RelatedTypeSymbol, SymbolEqualityComparer.Default)
            .Select(g => g.ToArray())
            .Where(g => g.Length > 1)
            .Select(g => g[0].RelatedTypeSymbol.Name);

            if(!duplicateRelationNames.Any())
                return;

            var location = model.TargetDeclaration.GetLocation();
            foreach(var name in duplicateRelationNames)
            {
                var diagnostic = Diagnostics.DuplicateRelation(location, name);
                _ = diagnostics.Add(diagnostic);
            }
        });
    public static IDiagnosticProvider<TargetDataModel> GenericRelation =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            var target = model.Symbol;
            var relations = model.Annotations.Relations
                .Where(r => r.RelatedTypeSymbol.IsGenericType);

            if(!(target.IsGenericType && relations.Any()))
                return;

            var location = model.TargetDeclaration.GetLocation();
            var diagnostic = Diagnostics.GenericRelation(location);
            _ = diagnostics.Add(diagnostic);
        });
    public static IDiagnosticProvider<TargetDataModel> StorageSelectionViolations =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            var violations = model.Annotations.AllRepresentableTypes
                .Where(d => d.Storage.Violation != StorageSelectionViolation.None);

            if(!violations.Any())
                return;

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
                    _ = diagnostics.Add(diagnostic);
            }
        });
    public static IDiagnosticProvider<TargetDataModel> SmallGenericUnion =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            if(!model.Symbol.IsGenericType ||
                model.Annotations.Settings.Layout != LayoutSetting.Small)
            {
                return;
            }

            var location = model.TargetDeclaration.GetLocation();
            var diagnostic = Diagnostics.SmallGenericUnion(location);
            _ = diagnostics.Add(diagnostic);
        });
    public static IDiagnosticProvider<TargetDataModel> UnknownGenericParameterName =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
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
                return;

            var location = model.TargetDeclaration.GetLocation();

            foreach(var unknown in unknowns)
            {
                var diagnostic = Diagnostics.UnknownGenericParameterName(location, unknown);
                _ = diagnostics.Add(diagnostic);
            }
        });
    public static IDiagnosticProvider<TargetDataModel> ReservedGenericParameterName =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            var collisions = model.Symbol.TypeParameters
                .Select(p => p.Name)
                .Where(model.Annotations.Settings.IsReservedGenericTypeName)
                .ToArray();

            if(collisions.Length == 0)
                return;

            var location = model.TargetDeclaration.GetLocation();

            foreach(var collision in collisions)
            {
                var diagnostic = Diagnostics.ReservedGenericParameterName(location, collision);
                _ = diagnostics.Add(diagnostic);
            }
        });
    public static IDiagnosticProvider<TargetDataModel> OperatorOmissions =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            var omissions = model.OperatorOmissions;
            var location = model.TargetDeclaration.GetLocation();

            foreach(var interfaceOmission in omissions.Interfaces)
            {
                var diagnostic = Diagnostics.RepresentableTypeIsInterface(
                    location,
                    interfaceOmission.Names.SimpleTypeName);
                _ = diagnostics.Add(diagnostic);
            }

            foreach(var supertypes in omissions.Supertypes)
            {
                var diagnostic = Diagnostics.RepresentableTypeIsSupertype(
                    location,
                    supertypes.Names.SimpleTypeName);
                _ = diagnostics.Add(diagnostic);
            }
        });
    public static IDiagnosticProvider<TargetDataModel> UnionTypeSettingsOnNonUnionType =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
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
            var diagnostic = Diagnostics.UnionTypeSettingsOnNonUnionType(location);
            _ = diagnostics.Add(diagnostic);
        });
    public static IDiagnosticProvider<TargetDataModel> ImplicitConversionIfSolitary =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
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
                _ = diagnostics.Add(diagnostic);
            } else if(attributes.Count > 1 &&
               attributes.Any(a => a.Attribute.Options.HasFlag(UnionTypeOptions.ImplicitConversionIfSolitary)))
            {
                var diagnostic = Diagnostics.ImplicitConversionOptionOnNonSolitary(location);
                _ = diagnostics.Add(diagnostic);
            }
        });
    public static IDiagnosticProvider<TargetDataModel> UnionTypeCount =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            var count = model.Annotations.AllRepresentableTypes.Count;
            if(count <= Byte.MaxValue)
                return;

            var location = model.TargetDeclaration.GetLocation();
            var diagnostic = Diagnostics.TooManyTypes(location);
            _ = diagnostics.Add(diagnostic);
        });
    public static IDiagnosticProvider<TargetDataModel> Partiality =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            if(model.TargetDeclaration.IsPartial())
                return;

            var location = model.TargetDeclaration.Identifier.GetLocation();
            var diagnostic = Diagnostics.NonPartialDeclaration(location);
            _ = diagnostics.Add(diagnostic);
        });
    public static IDiagnosticProvider<TargetDataModel> NonStatic =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            if(!model.TargetDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                return;

            var location = model.TargetDeclaration.Identifier.GetLocation();
            var diagnostic = Diagnostics.StaticTarget(location);
            _ = diagnostics.Add(diagnostic);
        });
    public static IDiagnosticProvider<TargetDataModel> NonRecord =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            if(!model.TargetDeclaration.IsKind(SyntaxKind.RecordDeclaration) &&
                !model.TargetDeclaration.IsKind(SyntaxKind.RecordStructDeclaration))
            {
                return;
            }

            var location = model.TargetDeclaration.Identifier.GetLocation();
            var diagnostic = Diagnostics.RecordTarget(location);
            _ = diagnostics.Add(diagnostic);
        });
    /*
    public static IDiagnosticProvider<TargetDataModel> UnionTypeAttribute =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            if(model.Annotations.AllRepresentableTypes.Count > 0)
                return;

            var location = model.TargetDeclaration.Identifier.GetLocation();
            var diagnostic = Diagnostics.MissingUnionTypeAttribute(location);
            _ = diagnostics.Add(diagnostic);
        });
    */
    public static IDiagnosticProvider<TargetDataModel> UniqueUnionTypeAttributes =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
        {
            _ = model.Annotations.AllRepresentableTypes
                .GroupBy(t => t.Names.FullTypeName)
                .Select(g => (Name: g.Key, Locations: g.Select(t => model.TargetDeclaration.GetLocation()).ToArray()))
                .Where(t => t.Locations.Length > 1)
                .SelectMany(t => t.Locations.Skip(1).Select(l => Diagnostics.DuplicateUnionTypeAttributes(t.Name, l)))
                .Aggregate(diagnostics, (b, d) => b.Add(d));
        });
    public static IDiagnosticProvider<TargetDataModel> AliasCollisions =
        DiagnosticProvider.Create<TargetDataModel>(static (model, diagnostics) =>
    {
        var duplicates = model.Annotations.AllRepresentableTypes
            .GroupBy(a => a.Names.SafeAlias)
            .Where(g => g.Skip(1).Any())
            .Select(g => g.First().Names.SimpleTypeName);
        if(!duplicates.Any())
            return;

        var location = model.TargetDeclaration.GetLocation();
        _ = duplicates.Select(d => Diagnostics.AliasCollision(location, d))
                .Aggregate(diagnostics, (b, d) => b.Add(d));
    });

    public static IEnumerable<IDiagnosticProvider<TargetDataModel>> All = new[]
    {
            BidirectionalRelation,
            DuplicateRelation,
            GenericRelation,
            StorageSelectionViolations,
            SmallGenericUnion,
            UnknownGenericParameterName,
            ReservedGenericParameterName,
            OperatorOmissions,
            UnionTypeSettingsOnNonUnionType,
            ImplicitConversionIfSolitary,
            UnionTypeCount,
            Partiality,
            NonStatic,
            NonRecord,
            //UnionTypeAttribute,
            UniqueUnionTypeAttributes,
            AliasCollisions
    };
}
