namespace RhoMicro.CodeAnalysis.UnionsGenerator.Analyzers;

using System.Xml.Linq;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

internal static class Providers
{
    private static void Add(
        UnionTypeModel model,
        IDiagnosticsAccumulator<UnionTypeModel> diagnostics,
        Func<Location, Diagnostic> factory) => diagnostics.AddRange(model.Locations.Value.Select(factory));
    private static void Add(
        UnionTypeModel model,
        IDiagnosticsAccumulator<UnionTypeModel> diagnostics,
        Func<Location, IEnumerable<Diagnostic>> factory) => diagnostics.AddRange(model.Locations.Value.SelectMany(factory));
    public static readonly IDiagnosticProvider<UnionTypeModel> NullableOptionOnValueType =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var nullableValueTypes = model.RepresentableTypes
                .Where(t => t.Signature.IsNullableAnnotated);

            if(!nullableValueTypes.Any())
                return;

            Add(model, diagnostics,
                l => nullableValueTypes.Select(t => Diagnostics.NullableOptionOnValueType(l, t.Alias)));
        });
    public static readonly IDiagnosticProvider<UnionTypeModel> BidirectionalRelation =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var bidirectionalRelationNames = model.Relations
                .Where(r => r.RelatedType.Signature.Names.FullGenericName == model.Signature.Names.FullGenericName)
                .Select(r => r.RelatedType.Signature.Names.FullGenericName);

            if(!bidirectionalRelationNames.Any())
                return;

            Add(model, diagnostics,
                l => bidirectionalRelationNames.Select(n => Diagnostics.BidirectionalRelation(l, n)));
        });
    public static readonly IDiagnosticProvider<UnionTypeModel> DuplicateRelation =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var duplicateRelationNames = model.Relations
            .GroupBy(r => r.RelatedType.Signature.Names.FullGenericName)
            .Select(g => g.ToArray())
            .Where(g => g.Length > 1)
            .Select(g => g[0].RelatedType.Signature.Names.FullGenericName);

            if(!duplicateRelationNames.Any())
                return;

            Add(model, diagnostics,
                l => duplicateRelationNames.Select(n => Diagnostics.DuplicateRelation(l, n)));
        });
    public static IDiagnosticProvider<UnionTypeModel> GenericRelation =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var relations = model.Relations
                .Where(r => r.RelatedType.Signature.IsGenericType);

            if(!( model.Signature.IsGenericType && relations.Any() ))
                return;

            Add(model, diagnostics,
                Diagnostics.GenericRelation);
        });
    public static IDiagnosticProvider<UnionTypeModel> StorageSelectionViolations =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var offendingRepresentables = model.RepresentableTypes
                .Where(d => d.StorageStrategy.Value.Violation != StorageSelectionViolation.None);

            foreach(var representableType in offendingRepresentables)
            {
                var name = representableType.Signature.Names.FullGenericNullableName;
                var violation = representableType.StorageStrategy.Value.Violation;

                var factory = violation switch
                {
                    StorageSelectionViolation.PureValueReferenceSelection =>
                        //cast once, infer following exprs
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
                };

                if(factory != null)
                {
                    Add(model, diagnostics,
                        l => factory.Invoke(l, name));
                }
            }
        });
    public static IDiagnosticProvider<UnionTypeModel> SmallGenericUnion =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            if(!model.Signature.IsGenericType || model.Settings.Layout != LayoutSetting.Small)
            {
                return;
            }

            var newLocations = model.Locations.Value
                .Select(Diagnostics.SmallGenericUnion);
            diagnostics.AddRange(newLocations);
        });
    //public static IDiagnosticProvider<UnionTypeModel> UnknownGenericParameterName =
    //    DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
    //    {
    //        var available = model.Symbol.TypeParameters
    //            .Select(p => p.Name)
    //            .ToImmutableHashSet();
    //        var unknowns = model.Annotations.AllRepresentableTypes
    //            .Where(a => a.Attribute.RepresentableTypeIsGenericParameter)
    //            .Where(a => !available.Contains(a.Names.SimpleTypeName))
    //            .Select(a => a.Names.SimpleTypeName)
    //            .ToArray();

    //        if(unknowns.Length == 0)
    //            return;

    //        var location = model.TargetDeclaration.GetLocation();

    //        foreach(var unknown in unknowns)
    //        {
    //            var diagnostic = Diagnostics.UnknownGenericParameterName(location, unknown);
    //            _ = diagnostics.Add(diagnostic);
    //        }
    //    });
    public static IDiagnosticProvider<UnionTypeModel> ReservedGenericParameterName =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var collisions = model.Signature.TypeArgs
                .Select(p => p.Names.Name)
                .Where(model.Settings.IsReservedGenericTypeName);

            if(!collisions.Any())
                return;

            var newDiagnostics = model.Locations.Value
                .SelectMany(l => collisions
                    .Select(c => Diagnostics.ReservedGenericParameterName(l, c)));
            diagnostics.AddRange(newDiagnostics);
        });
    public static IDiagnosticProvider<UnionTypeModel> OperatorOmissions =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var omissions = model.RepresentableTypes
                .Where(t => t.OmitConversionOperators);

            var interfaceOmissions = omissions.Where(o => o.Signature.IsInterface);

            foreach(var interfaceOmission in interfaceOmissions)
            {
                Add(model, diagnostics,
                    l => Diagnostics.RepresentableTypeIsInterface(
                        l,
                        interfaceOmission.Signature.Names.FullGenericNullableName));
            }

            var supertypeOmissions = omissions.Where(o => o.IsBaseClassToUnionType);

            foreach(var supertype in supertypeOmissions)
            {
                Add(model, diagnostics,
                    l => Diagnostics.RepresentableTypeIsSupertype(
                        l,
                        supertype.Signature.Names.FullGenericNullableName));
            }
        });
    //public static IDiagnosticProvider<UnionTypeModel> UnionTypeSettingsOnNonUnionType =
    //    DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
    //    {
    //        TODO
    //        var representableTypes = model.RepresentableTypes;

    //        if(representableTypes.Count > 0 || /*settings not located ?*/)
    //        {
    //            return;
    //        }

    //        var location = model.TargetDeclaration.GetLocation();
    //        var diagnostic = Diagnostics.UnionTypeSettingsOnNonUnionType(location);
    //        _ = diagnostics.Add(diagnostic);
    //    });
    public static IDiagnosticProvider<UnionTypeModel> ImplicitConversionIfSolitary =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            if(model.RepresentableTypes.Count > 1 &&
               model.RepresentableTypes.Any(a => a.Options.HasFlag(UnionTypeOptions.ImplicitConversionIfSolitary)))
            {
                Add(model, diagnostics,
                    Diagnostics.ImplicitConversionOptionOnNonSolitary);
            }
        });
    public static IDiagnosticProvider<UnionTypeModel> UnionTypeCount =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var count = model.RepresentableTypes.Count;
            if(count <= Qualifications.MaxRepresentableTypesCount)
                return;

            Add(model, diagnostics,
                Diagnostics.TooManyTypes);
        });
    //public static IDiagnosticProvider<UnionTypeModel> Partiality =
    //    DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
    //    {
    //        if(model.TargetDeclaration.IsPartial())
    //            return;

    //        var location = model.TargetDeclaration.Identifier.GetLocation();
    //        var diagnostic = Diagnostics.NonPartialDeclaration(location);
    //        _ = diagnostics.Add(diagnostic);
    //    });
    public static IDiagnosticProvider<UnionTypeModel> NonStatic =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            if(!model.Signature.IsStatic)
                return;

            Add(model, diagnostics,
                Diagnostics.StaticTarget);
        });
    public static IDiagnosticProvider<UnionTypeModel> NonRecord =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            if(!model.Signature.IsRecord)
                return;

            Add(model, diagnostics,
                Diagnostics.RecordTarget);
        });
    /*
    public static IDiagnosticProvider<UnionTypeModel> UnionTypeAttribute =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            if(model.Annotations.AllRepresentableTypes.Count > 0)
                return;

            var location = model.TargetDeclaration.Identifier.GetLocation();
            var diagnostic = Diagnostics.MissingUnionTypeAttribute(location);
            _ = diagnostics.Add(diagnostic);
        });
    */
    public static IDiagnosticProvider<UnionTypeModel> UniqueUnionTypeAttributes =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var duplicateRepresentableTypeNames = model.RepresentableTypes
                .Select(t => t.Signature.Names.FullGenericName)
                .GroupBy(n => n)
                .Where(g => g.Skip(1).Any())
                .Select(g => g.First());

            if(!duplicateRepresentableTypeNames.Any())
                return;

            Add(model, diagnostics,
                l => duplicateRepresentableTypeNames.Select(t =>
                    Diagnostics.DuplicateUnionTypeAttributes(l, t)));
        });
    public static IDiagnosticProvider<UnionTypeModel> AliasCollisions =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
    {

        var duplicateRepresentableTypeAliae = model.RepresentableTypes
            .Select(t => t.Alias)
            .GroupBy(n => n)
            .Where(g => g.Skip(1).Any())
            .Select(g => g.First());

        if(!duplicateRepresentableTypeAliae.Any())
            return;

        Add(model, diagnostics,
            l => duplicateRepresentableTypeAliae.Select(t =>
                Diagnostics.AliasCollision(l, t)));
    });
    public static IDiagnosticProvider<UnionTypeModel> SelfReference =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            var hasSelfReference = model.RepresentableTypes
                .Any(t => t.Signature.Equals(model.Signature));

            if(!hasSelfReference)
                return;

            Add(model, diagnostics,
                Diagnostics.SelfReference);
        });
    public static IDiagnosticProvider<UnionTypeModel> Inheritance =
        DiagnosticProvider.Create<UnionTypeModel>(static (model, diagnostics) =>
        {
            if(model.Signature.HasNoBaseClass)
                return;

            Add(model, diagnostics,
                Diagnostics.Inheritance);
        });

    public static IEnumerable<IDiagnosticProvider<UnionTypeModel>> All = new[]
    {
            BidirectionalRelation,
            DuplicateRelation,
            GenericRelation,
            StorageSelectionViolations,
            SmallGenericUnion,
            //UnknownGenericParameterName,
            ReservedGenericParameterName,
            OperatorOmissions,
            //UnionTypeSettingsOnNonUnionType,
            ImplicitConversionIfSolitary,
            UnionTypeCount,
            //Partiality,
            NonStatic,
            NonRecord,
            //UnionTypeAttribute,
            UniqueUnionTypeAttributes,
            AliasCollisions,
            SelfReference,
            Inheritance,
            NullableOptionOnValueType
    };
}
