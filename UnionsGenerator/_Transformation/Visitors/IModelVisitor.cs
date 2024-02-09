namespace RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;

using RhoMicro.CodeAnalysis.UnionsGenerator._Models;

interface IModelVisitor :
    IVisitor<SettingsModel>,
    IVisitor<RepresentableTypeModel>,
    IVisitor<TypeSignatureModel>,
    IVisitor<UnionTypeModel>,
    IVisitor<TypeNamesModel>,
    IVisitor<FactoryModel>,
    IVisitor<RelatedTypeModel>,
    IVisitor<RelationModel>,
    IVisitor<PartialRepresentableTypeModel>,
    IVisitor<PartialUnionTypeModel>;
