namespace RhoMicro.CodeAnalysis.CopyToGenerator;

using Microsoft.CodeAnalysis;

using System.Collections.Generic;

readonly struct PropertyModel : IEquatable<PropertyModel>
{
    private PropertyModel(String name, Boolean isListAssignable, String? itemType, Boolean isMarked)
    {
        Name = name;
        IsListAssignable = isListAssignable;
        ItemType = itemType;
        IsMarked = isMarked;
    }

    public readonly String Name;
    public readonly Boolean IsListAssignable;
    public readonly String? ItemType;
    public readonly Boolean IsMarked;

    public override Boolean Equals(Object? obj) => obj is PropertyModel model && Equals(model);
    public Boolean Equals(PropertyModel other) => Name == other.Name &&
        IsListAssignable == other.IsListAssignable &&
        ItemType == other.ItemType &&
        IsMarked == other.IsMarked;

    public override Int32 GetHashCode()
    {
        var hashCode = 501377101;
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Name);
        if(IsListAssignable)
        {
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(ItemType!);
        }

        hashCode = hashCode * -1521134295 + IsMarked.GetHashCode();
        return hashCode;
    }

    internal static PropertyModel Create(IPropertySymbol symbol) => new(
        name: symbol.Name,
        isListAssignable: symbol.Type.TryGetListAssignableItemType(out var itemType),
        itemType: itemType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        isMarked: symbol.Type.IsMarked());

    public static Boolean operator ==(PropertyModel left, PropertyModel right) => left.Equals(right);
    public static Boolean operator !=(PropertyModel left, PropertyModel right) => !(left == right);
}