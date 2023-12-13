namespace RhoMicro.CodeAnalysis;

partial class UnionTypeSettingsAttribute: IEquatable<UnionTypeSettingsAttribute>
{
    public override Boolean Equals(Object? obj) => Equals(obj as UnionTypeSettingsAttribute);
    public Boolean Equals(UnionTypeSettingsAttribute? other) =>
        other is not null &&
        ToStringSetting == other.ToStringSetting &&
        Layout == other.Layout &&
        DiagnosticsLevel == other.DiagnosticsLevel &&
        ConstructorAccessibility == other.ConstructorAccessibility &&
        MatchTypeName == other.MatchTypeName &&
        GenericTValueName == other.GenericTValueName &&
        TryConvertTypeName == other.TryConvertTypeName;

    public Boolean IsReservedGenericTypeName(String name) => _reservedGenericTypeNames.Contains(name);

    public override Int32 GetHashCode()
    {
        var hashCode = 304521224;
        hashCode = hashCode * -1521134295 + ToStringSetting.GetHashCode();
        hashCode = hashCode * -1521134295 + Layout.GetHashCode();
        hashCode = hashCode * -1521134295 + MatchTypeName.GetHashCode();
        hashCode = hashCode * -1521134295 + TryConvertTypeName.GetHashCode();
        hashCode = hashCode * -1521134295 + GenericTValueName.GetHashCode();
        hashCode = hashCode * -1521134295 + DiagnosticsLevel.GetHashCode();
        hashCode = hashCode * -1521134295 + ConstructorAccessibility.GetHashCode();
        return hashCode;
    }
}
