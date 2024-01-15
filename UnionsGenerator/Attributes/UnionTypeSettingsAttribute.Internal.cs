namespace RhoMicro.CodeAnalysis;

using System.Collections.Generic;

partial class UnionTypeSettingsAttribute : IEquatable<UnionTypeSettingsAttribute?>
{
    public override Boolean Equals(Object? obj) => Equals(obj as UnionTypeSettingsAttribute);
    public Boolean Equals(UnionTypeSettingsAttribute? other) => other is not null && base.Equals(other) && EmitGeneratedSourceCode == other.EmitGeneratedSourceCode && TypeDeclarationPreface == other.TypeDeclarationPreface && ToStringSetting == other.ToStringSetting && Layout == other.Layout && DiagnosticsLevel == other.DiagnosticsLevel && ConstructorAccessibility == other.ConstructorAccessibility && GenericTValueName == other.GenericTValueName && TryConvertTypeName == other.TryConvertTypeName && MatchTypeName == other.MatchTypeName && GenerateJsonConverter == other.GenerateJsonConverter && InterfaceMatchSetting == other.InterfaceMatchSetting;

    public override Int32 GetHashCode()
    {
        var hashCode = -73109686;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EmitGeneratedSourceCode.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(TypeDeclarationPreface);
        hashCode = hashCode * -1521134295 + ToStringSetting.GetHashCode();
        hashCode = hashCode * -1521134295 + Layout.GetHashCode();
        hashCode = hashCode * -1521134295 + DiagnosticsLevel.GetHashCode();
        hashCode = hashCode * -1521134295 + ConstructorAccessibility.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(GenericTValueName);
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(TryConvertTypeName);
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(MatchTypeName);
        hashCode = hashCode * -1521134295 + GenerateJsonConverter.GetHashCode();
        hashCode = hashCode * -1521134295 + InterfaceMatchSetting.GetHashCode();
        return hashCode;
    }

    public Boolean IsReservedGenericTypeName(String name) => _reservedGenericTypeNames.Contains(name);

    public static Boolean operator ==(UnionTypeSettingsAttribute? left, UnionTypeSettingsAttribute? right) => 
        EqualityComparer<UnionTypeSettingsAttribute?>.Default.Equals(left, right);
    public static Boolean operator !=(UnionTypeSettingsAttribute? left, UnionTypeSettingsAttribute? right) => 
        !(left == right);
}
