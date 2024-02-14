namespace RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

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
