namespace RhoMicro.CodeAnalysis.UnionsGenerator.Analyzers;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

internal static partial class Diagnostics
{
    readonly struct DiagnosticInfo(String title, String message, DiagnosticSeverity severity) : IEquatable<DiagnosticInfo>
    {
        public readonly String Title = title;
        public readonly String Message = message;
        public readonly DiagnosticSeverity Severity = severity;

        public override Boolean Equals(Object? obj) => obj is DiagnosticInfo item && Equals(item);
        public Boolean Equals(DiagnosticInfo other) => Title == other.Title && Message == other.Message && Severity == other.Severity;

        public override Int32 GetHashCode()
        {
            var hashCode = -940627307;
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Message);
            hashCode = hashCode * -1521134295 + EqualityComparer<DiagnosticSeverity>.Default.GetHashCode(Severity);
            return hashCode;
        }

        public static Boolean operator ==(DiagnosticInfo left, DiagnosticInfo right) => left.Equals(right);
        public static Boolean operator !=(DiagnosticInfo left, DiagnosticInfo right) => !(left == right);
    }

    private static readonly IReadOnlyDictionary<Id, DiagnosticInfo> _infos = new Dictionary<Id, DiagnosticInfo>()
    {
{Id.AliasCollision , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"An alias collision has been detected for the representable type `{0}`. Make sure that for colliding type names (excluding generic arguments) you provide a unique alias. If no explicit alias is provided, the simple type name of the representable type will be used, e.g.: `List` will be used for variables of the `List<T>` type. Aliae are used for parameter, variable, enum and field names.",title:"Alias Collision")},
{Id.BoxingStrategy , new DiagnosticInfo(severity: DiagnosticSeverity.Warning,message:"Boxing will occur for the representable type {0}. Consider using a different storage option.",title:"Boxing Storage Detected")},
{Id.DuplicateUnionTypeAttributes , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"Union types may not be declared using duplicate 'UnionType' attributes of the same representable type ({0}).",title:"Duplicate Union Type Declaration")},
{Id.TooManyTypes, new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"Union types may represent no more that 256 types.",title:"Too many Union Types")},
{Id.GeneratorException , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"The generator encountered an unexpected error: {0}",title:"Unexpected Generator Error")},
{Id.GenericViolationStrategy , new DiagnosticInfo(severity: DiagnosticSeverity.Warning,message:"The selected storage option for {0} was ignored because the target union type is generic.",title:"Generic Storage Violation Detected")},
{Id.ImplicitConversionOptionOnNonSolitary , new DiagnosticInfo(severity: DiagnosticSeverity.Warning,message:"The `ImplicitConversionIfSolitary` option will be ignored because the target union type may represent more than one type.",title:"Union Type Option Ignored")},
{Id.ImplicitConversionOptionOnSolitary , new DiagnosticInfo(severity: DiagnosticSeverity.Info,message:"The interface implementation for `{0}` was omitted because the `ImplicitConversionIfSolitary` option was used.",title:"Omitting Interface Implementation")},
{Id.InvalidAttributeTarget , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"Only type declarations may be annotated with union type attributes.",title:"Invalid Attribute Target")},
//{Id.MissingUnionTypeAttribute , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"Union types must be declared using at least one instance of the 'UnionType' attribute.",title:"Missing 'UnionType' Attribute")},
{Id.NonPartialDeclaration , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"Union types must be declared using the 'partial' keyword.",title:"Nonpartial Union Declaration")},
{Id.PossibleBoxingStrategy , new DiagnosticInfo(severity: DiagnosticSeverity.Warning,message:"Boxing may occur for the representable type {0}. Consider constraining it to either `class` or `struct` in order to help the generator emit an efficient implementation. Alternatively, use the `StorageOption.Field` option to generate dedicated field for the type.",title:"Possible Boxing Storage Detected")},
{Id.PossibleTleStrategy , new DiagnosticInfo(severity: DiagnosticSeverity.Warning,message:"The selected storage option for {0} was ignored because it could cause a `TypeLoadException` to be thrown.",title:"Possible TypeLoadException Detected")},
{Id.RecordTarget , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"Union types may not be declared as 'record' types.",title:"Record Union Type Declaration")},
{Id.RepresentableTypeIsInterface , new DiagnosticInfo(severity: DiagnosticSeverity.Info,message:"No conversion operators will be generated for the representable type {0} because it is an interface.",title:"Conversion Operators Omitted")},
{Id.RepresentableTypeIsSupertype , new DiagnosticInfo(severity: DiagnosticSeverity.Info,message:"No conversion operators will be generated for the representable type {0} because it is a supertype of the target union type.",title:"Conversion Operators Omitted")},
{Id.ReservedGenericParameterName , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"The targeted union type contains a generic parameter named `{0}` which is reserved for generated code. Either change its name or use the `UnionTypeSettings` attribute to set generated generic type parameter names.",title:"Reserved Generic Parameter Name")},
{Id.SmallGenericUnion , new DiagnosticInfo(severity: DiagnosticSeverity.Info,message:"The small layout setting was ignored because the target union type is generic.",title:"Ignoring Small Layout")},
{Id.StaticTarget , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"Union types may not be declared using the 'static' modifier.",title:"Static Union Type Declaration")},
{Id.TleStrategy , new DiagnosticInfo(severity: DiagnosticSeverity.Warning,message:"The selected storage option for {0} was ignored because it would cause a `TypeLoadException` to be thrown.",title:"TypeLoadException Prevented")},
{Id.UnionTypeSettingsOnNonUnionType , new DiagnosticInfo(severity: DiagnosticSeverity.Warning,message:"The union type settings attribute will be ignored because the target type is not a union type.",title:"Union Type Settings Ignored")},
{Id.UnknownGenericParameterName , new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"The targeted union type contains an unknown parameter name `{0}`. It could not be located in the type parameter list.",title:"Unknown Generic Parameter Name") },
{Id.GenericRelation, new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"Relations may not be declared between generic types.",title:"Generic Relation") },
{Id.BidirectionalRelation, new DiagnosticInfo(severity: DiagnosticSeverity.Error,message:"The bidirectional relation with {0} will be ignored.",title:"Bidirectional Relation") },
{Id.DuplicateRelation, new DiagnosticInfo(severity: DiagnosticSeverity.Warning,message:"The duplicate relation with {0} will be ignored.",title:"Duplicate Relation") }
};

    public static readonly IReadOnlyList<DiagnosticDescriptor> Descriptors =
        Enum.GetValues(typeof(Id))
            .OfType<Id>()
            .Select(id => new DiagnosticDescriptor(
                    $"RUG{(Int32)id:0000}",
                    _infos[id].Title,
                    _infos[id].Message,
                    _category,
                    _infos[id].Severity,
                    true))
            .ToList();

    static readonly Dictionary<Id, DiagnosticDescriptor> _idDescriptorMap =
        Enum.GetValues(typeof(Id))
            .OfType<Id>()
            .Select(id => (Id: id, Descriptor: new DiagnosticDescriptor(
                    $"RUG{(Int32)id:0000}",
                    _infos[id].Title,
                    _infos[id].Message,
                    _category,
                    _infos[id].Severity,
                    true)))
        .ToDictionary(t => t.Id, t => t.Descriptor);

    static Diagnostic Create(
        Id id,
        Location location,
        params Object[] messageArgs) =>
        Diagnostic.Create(
            _idDescriptorMap[id],
            location,
            messageArgs);
}
