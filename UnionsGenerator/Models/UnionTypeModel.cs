namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

/// <summary>
/// Represents a user-defined union type.
/// </summary>
/// <param name="Signature"></param>
/// <param name="RepresentableTypes"></param>
/// <param name="Relations"></param>
/// <param name="Settings"></param>
/// <param name="IsGenericType"></param>
/// <param name="ScopedDataTypeName"></param>
/// <param name="Groups"></param>
sealed record UnionTypeModel(
    TypeSignatureModel Signature,
    EquatableList<RepresentableTypeModel> RepresentableTypes,
    EquatableList<RelationModel> Relations,
    SettingsModel Settings,
    Boolean IsGenericType,
    String ScopedDataTypeName,
    GroupsModel Groups) : IModel<UnionTypeModel>
{
    /// <inheritdoc/>
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<UnionTypeModel>
        => visitor.Visit(this);

    /// <summary>
    /// Creates a new union type model.
    /// </summary>
    /// <param name="signature"></param>
    /// <param name="representableTypes"></param>
    /// <param name="relations"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static UnionTypeModel Create(
        TypeSignatureModel signature,
        EquatableList<RepresentableTypeModel> representableTypes,
        EquatableList<RelationModel> relations,
        SettingsModel settings,
        CancellationToken cancellationToken)
    {
        Throw.ArgumentNull(signature, nameof(signature));
        Throw.ArgumentNull(representableTypes, nameof(representableTypes));
        Throw.ArgumentNull(relations, nameof(relations));
        Throw.ArgumentNull(settings, nameof(settings));

        if(representableTypes.Count < 1)
            throw new ArgumentException($"{nameof(representableTypes)} must contain at least one representable type model.", nameof(representableTypes));

        cancellationToken.ThrowIfCancellationRequested();

        var isGenericType = signature.TypeArgs.Count > 0;
        var conversionFunctionsTypeName = $"{signature.Names.FullIdentifierOrHintName}_ScopedData{signature.Names.TypeArgsString}";

        var groups = GroupsModel.Create(representableTypes);

        var result = new UnionTypeModel(
            signature,
            representableTypes,
            relations,
            settings,
            isGenericType,
            conversionFunctionsTypeName,
            groups);

        return result;
    }

    public String GetSpecificAccessibility(TypeSignatureModel _)
    {
        var accessibility = Settings.ConstructorAccessibility;

        if(accessibility == ConstructorAccessibilitySetting.PublicIfInconvertible
            //TODO: consider omissions
            /*&& OperatorOmissions.AllOmissions.Contains(representableType)*/)
        {
            accessibility = ConstructorAccessibilitySetting.Public;
        }

        var result = accessibility == ConstructorAccessibilitySetting.Public ?
            "public" :
            "private";

        return result;
    }
}
