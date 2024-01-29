namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

sealed record GenericTargetTypeModel(
    String Namespace,
    EquatableList<String> ContainingTypeNames,
    String DeclarationKeyword,
    String DeclarationName,
    EquatableList<String> TypeParameterNames)
    : TargetTypeModel(Namespace, ContainingTypeNames, DeclarationKeyword, DeclarationName);
