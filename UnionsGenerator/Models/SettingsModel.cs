﻿namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

sealed record SettingsModel(
#region Settings
    ToStringSetting ToStringSetting,
    LayoutSetting Layout,
    DiagnosticsLevelSettings DiagnosticsLevel,
    ConstructorAccessibilitySetting ConstructorAccessibility,
    InterfaceMatchSetting InterfaceMatchSetting,
    EqualityOperatorsSetting EqualityOperatorsSetting,
    MiscellaneousSettings Miscellaneous,
#endregion
#region Strings
    String TypeDeclarationPreface,
    String GenericTValueName,
    String TryConvertTypeName,
    String MatchTypeName,
    String TagTypeName,
    String ValueTypeContainerTypeName,
    String ValueTypeContainerName,
    String ReferenceTypeContainerName,
    String TagFieldName,
    String TagNoneName,
    String JsonConverterTypeName
#endregion
    ) : IModel<SettingsModel>
{
    private EquatedData<HashSet<String>>? _reservedNames;
    public Boolean IsReservedGenericTypeName(String name) =>
        ( _reservedNames ??= new([GenericTValueName, TryConvertTypeName, MatchTypeName]) ).Value.Contains(name);

    public static ImmutableHashSet<String> PropertyNames { get; } = new[]
    {
#region Settings
    nameof(ToStringSetting),
    nameof(Layout),
    nameof(DiagnosticsLevel),
    nameof(ConstructorAccessibility),
    nameof(InterfaceMatchSetting),
    nameof(EqualityOperatorsSetting),
    nameof(Miscellaneous),
#endregion
#region Strings
    nameof(TypeDeclarationPreface),
    nameof(GenericTValueName),
    nameof(TryConvertTypeName),
    nameof(MatchTypeName),
    nameof(TagTypeName),
    nameof(ValueTypeContainerTypeName),
    nameof(ValueTypeContainerName),
    nameof(ReferenceTypeContainerName),
    nameof(TagFieldName),
    nameof(TagNoneName),
    nameof(JsonConverterTypeName),
#endregion
    }.ToImmutableHashSet();
    public static SettingsModel Create(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var assemblySettings = type.ContainingAssembly.GetAttributes()
            .Where(Qualifications.IsUnionTypeSettingsAttribute)
            .Select(SettingsAttributeData.CreateAssemblySettingsWithDefaults)
            .FirstOrDefault()
            ?? SettingsAttributeData.Default;

        cancellationToken.ThrowIfCancellationRequested();
        var settings = type.GetAttributes()
            .Where(Qualifications.IsUnionTypeSettingsAttribute)
            .Select(a => SettingsAttributeData.CreateDeclaredSettingsWithDefaults(a, assemblySettings))
            .OfUnionTypeSettingsAttribute()
            .SingleOrDefault()
            ?? new();

        cancellationToken.ThrowIfCancellationRequested();
        var result = Create(settings);

        return result;
    }
    public static SettingsModel Create(UnionTypeSettingsAttribute attribute) => new(
    #region Settings
    ToStringSetting: attribute.ToStringSetting,
    Layout: attribute.Layout,
    DiagnosticsLevel: attribute.DiagnosticsLevel,
    ConstructorAccessibility: attribute.ConstructorAccessibility,
    InterfaceMatchSetting: attribute.InterfaceMatchSetting,
    EqualityOperatorsSetting: attribute.EqualityOperatorsSetting,
    Miscellaneous: attribute.Miscellaneous,
    #endregion
    #region Strings
    TypeDeclarationPreface: attribute.TypeDeclarationPreface,
    GenericTValueName: attribute.GenericTValueName,
    TryConvertTypeName: attribute.TryConvertTypeName,
    MatchTypeName: attribute.MatchTypeName,
    TagTypeName: attribute.TagTypeName,
    ValueTypeContainerTypeName: attribute.ValueTypeContainerTypeName,
    ValueTypeContainerName: attribute.ValueTypeContainerName,
    ReferenceTypeContainerName: attribute.ReferenceTypeContainerName,
    TagFieldName: attribute.TagFieldName,
    TagNoneName: attribute.TagNoneName,
    JsonConverterTypeName: attribute.JsonConverterTypeName
    #endregion
        );
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<SettingsModel>
        => visitor.Visit(this);
}