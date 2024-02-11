namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Collections.Immutable;
using System.Reflection;

sealed class SettingsAttributeData : AttributeData, IEquatable<SettingsAttributeData?>
{
    #region Constructors
    private SettingsAttributeData(
        AttributeData declaredSettings,
        ImmutableArray<KeyValuePair<String, TypedConstant>> namedArguments,
        Dictionary<String, TypedConstant> namedArgumentsMap)
    {
        _namedArgumentsMap = namedArgumentsMap;

        CommonAttributeClass = declaredSettings.AttributeClass;
        CommonAttributeConstructor = declaredSettings.AttributeConstructor;
        CommonApplicationSyntaxReference = declaredSettings.ApplicationSyntaxReference;
        CommonConstructorArguments = declaredSettings.ConstructorArguments;
        CommonNamedArguments = namedArguments;
    }
    private SettingsAttributeData(
        AttributeData declaredSettings,
        Dictionary<String, TypedConstant> namedArgumentsMap)
        : this(declaredSettings, declaredSettings.NamedArguments, namedArgumentsMap)
    { }
    private SettingsAttributeData()
    {
        _namedArgumentsMap = [];
        CommonConstructorArguments = ImmutableArray.Create<TypedConstant>();
        CommonNamedArguments = ImmutableArray.Create<KeyValuePair<String, TypedConstant>>();
    }
    #endregion

    public static SettingsAttributeData Default { get; } = new();

    private readonly Dictionary<String, TypedConstant> _namedArgumentsMap;
    public Boolean ImplementsToString { get; private init; }

    protected override INamedTypeSymbol? CommonAttributeClass { get; }
    protected override IMethodSymbol? CommonAttributeConstructor { get; }
    protected override SyntaxReference? CommonApplicationSyntaxReference { get; }
    protected override ImmutableArray<TypedConstant> CommonConstructorArguments { get; }
    protected override ImmutableArray<KeyValuePair<String, TypedConstant>> CommonNamedArguments { get; }

    public static SettingsAttributeData CreateAssemblySettingsWithDefaults(AttributeData assemblySettings)
        => CreateWithDefaults(assemblySettings);
    public static SettingsAttributeData CreateDeclaredSettings(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var declaredSettings = context.Attributes[0];

        var declaredPropsMap = MapProperties(declaredSettings);

        var implementsToString = ( (ITypeSymbol)context.TargetSymbol ).GetMembers(nameof(ToString))
            .OfType<IMethodSymbol>()
            .Any(s => s.IsOverride &&
                s.Parameters.Length == 0);

        var result = new SettingsAttributeData(declaredSettings, declaredPropsMap)
        {
            ImplementsToString = implementsToString
        };

        return result;
    }
    public static SettingsAttributeData CreateDeclaredSettingsWithDefaults(
        AttributeData declaredSettings,
        SettingsAttributeData assemblySettings)
        => CreateWithDefaults(declaredSettings, assemblySettings);

    private static SettingsAttributeData CreateWithDefaults(
        AttributeData declaredSettings,
        SettingsAttributeData? fallbackSettings = null)
    {
        var declaredPropsMap = MapProperties(declaredSettings);
        var assemblyPropsMap = fallbackSettings?._namedArgumentsMap ?? [];
        var namedArgsMap = SettingsModel.PropertyNames
               .Select<String, (String name, TypedConstant value)?>(n =>
                   declaredPropsMap.TryGetValue(n, out var v) ? (n, v) :
                   assemblyPropsMap.TryGetValue(n, out v) ? (n, v) : null)
               .Where(c => c.HasValue)
               .ToDictionary(t => t!.Value.name, t => t!.Value.value);
        var namedArgs = namedArgsMap.ToImmutableArray();
        var result = new SettingsAttributeData(declaredSettings, namedArgs, namedArgsMap)
        {
            ImplementsToString = declaredSettings is SettingsAttributeData { ImplementsToString: true }
        };

        return result;
    }

    static Dictionary<String, TypedConstant> MapProperties(AttributeData data) =>
        data.NamedArguments
            .Where(kvp => kvp.Key != null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public override Boolean Equals(Object? obj) => Equals(obj as SettingsAttributeData);
    public Boolean Equals(SettingsAttributeData? other)
    {
        if(_namedArgumentsMap.Count != other?._namedArgumentsMap.Count)
            return false;

        foreach(var (name, constant) in _namedArgumentsMap)
        {
            if(!other._namedArgumentsMap.TryGetValue(name, out var otherConstant) ||
               !Equals(constant, otherConstant))
            {
                return false;
            }
        }

        return true;
    }

    private static Boolean Equals(TypedConstant a, TypedConstant b)
    {
        if(a.Kind != b.Kind || a.IsNull != b.IsNull)
            return false;

        switch(a.Kind)
        {
            case TypedConstantKind.Primitive or TypedConstantKind.Enum
            when !EqualityComparer<Object?>.Default.Equals(a.Value, b.Value):
                return false;
            case TypedConstantKind.Type
            when !SymbolEqualityComparer.IncludeNullability.Equals(a.Type, b.Type):
                return false;
            case TypedConstantKind.Array:
            {
                if(a.Values.Length != b.Values.Length)
                    return false;

                for(var i = 0; i < a.Values.Length; i++)
                {
                    var aValue = a.Values[i];
                    var bValue = b.Values[i];
                    if(!Equals(aValue, bValue))
                        return false;
                }

                break;
            }
        }

        return true;
    }

    public override Int32 GetHashCode() => throw new NotSupportedException("GetHashCode is not supported on settings attribute data.");
}
