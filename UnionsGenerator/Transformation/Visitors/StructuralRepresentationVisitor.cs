namespace RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors
{
    using RhoMicro.CodeAnalysis;
    using RhoMicro.CodeAnalysis.Library.Text;
    using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

    sealed class StructuralRepresentationVisitor(IndentedStringBuilder builder) : IModelVisitor
    {
        private readonly IndentedStringBuilder _builder = builder;

        public static StructuralRepresentationVisitor Create() => new(new());
        public static StructuralRepresentationVisitor Create(IndentedStringBuilderOptions builderOptions) => new(new(builderOptions));

        public void Visit(RepresentableTypeModel model) => _builder.ModelString(model);
        public void Visit(TypeSignatureModel model) => _builder.ModelString(model);
        public void Visit(UnionTypeModel model) => _builder.ModelString(model);
        public void Visit(SettingsModel model) => _builder.ModelString(model);
        public void Visit(RelatedTypeModel model) => _builder.ModelString(model);
        public void Visit(RelationModel model) => _builder.ModelString(model);
        public void Visit(TypeNamesModel model) => _builder.ModelString(model);
        public void Visit(PartialRepresentableTypeModel model) => _builder.ModelString(model);
        public void Visit(PartialUnionTypeModel model) => _builder.ModelString(model);
        public void Visit(FactoryModel model) => _builder.ModelString(model);

        public override String ToString() => _builder.ToString();
    }
}

namespace RhoMicro.CodeAnalysis.Library.Text
{
    using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
    partial class Blocks
    {
        public static Block SameLineBraces = new('{', '}');
    }
    partial class IndentedStringBuilder
    {
        private IndentedStringBuilder OpenSameLineBracesBlock() => OpenBlock(Blocks.SameLineBraces);
        private IndentedStringBuilder CloseSameLineBracesBlock() => CloseBlock();
        public IndentedStringBuilder ModelString(RelatedTypeModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.Signature), ModelString, model.Signature)
            .Property(nameof(model.RepresentableTypeSignatures), ModelString, model.RepresentableTypeSignatures, isLast: true)
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(RelationModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.RelatedType), ModelString, model.RelatedType)
            .Property(nameof(model.RelationType), model.RelationType, isLast: true)
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(TypeNamesModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.ContainingTypesString), model.ContainingTypesString)
            .Property(nameof(model.FullGenericName), model.FullGenericName)
            .Property(nameof(model.FullGenericNullableName), model.FullGenericNullableName)
            .Property(nameof(model.FullMetadataName), model.FullMetadataName)
            .Property(nameof(model.FullOpenGenericName), model.FullOpenGenericName)
            .Property(nameof(model.GenericName), model.GenericName)
            .Property(nameof(model.IdentifierOrHintName), model.IdentifierOrHintName)
            .Property(nameof(model.Name), model.Name)
            .Property(nameof(model.Namespace), model.Namespace)
            .Property(nameof(model.OpenGenericName), model.OpenGenericName)
            .Property(nameof(model.TypeArgsString), model.TypeArgsString, isLast: true)
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(PartialUnionTypeModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.Signature), ModelString, model.Signature)
            .Property(nameof(model.RepresentableType), ModelString, model.RepresentableType, isLast: true)
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(PartialRepresentableTypeModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.Signature), ModelString, model.Signature)
            .Property(nameof(model.Groups), model.Groups)
            .Property(nameof(model.Storage), model.Storage)
            .Property(nameof(model.Alias), model.Alias)
            .Property(nameof(model.Options), model.Options, isLast: true)
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(SettingsModel model) =>
            OpenSameLineBracesBlock()
        #region Settings
            .Property(nameof(model.ToStringSetting), model.ToStringSetting)
            .Property(nameof(model.Layout), model.Layout)
            .Property(nameof(model.DiagnosticsLevel), model.DiagnosticsLevel)
            .Property(nameof(model.ConstructorAccessibility), model.ConstructorAccessibility)
            .Property(nameof(model.InterfaceMatchSetting), model.InterfaceMatchSetting)
            .Property(nameof(model.Miscellaneous), model.Miscellaneous)
        #endregion
        #region Strings
            .Property(nameof(model.TypeDeclarationPreface), model.TypeDeclarationPreface)
            .Property(nameof(model.GenericTValueName), model.GenericTValueName)
            .Property(nameof(model.TryConvertTypeName), model.TryConvertTypeName)
            .Property(nameof(model.MatchTypeName), model.MatchTypeName)
            .Property(nameof(model.TagTypeName), model.TagTypeName)
            .Property(nameof(model.TagFieldName), model.TagFieldName)
            .Property(nameof(model.ValueTypeContainerTypeName), model.ValueTypeContainerTypeName)
            .Property(nameof(model.ValueTypeContainerName), model.ValueTypeContainerName)
            .Property(nameof(model.ReferenceTypeContainerName), model.ReferenceTypeContainerName)
            .Property(nameof(model.JsonConverterTypeName), model.JsonConverterTypeName, isLast: true)
        #endregion
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(UnionTypeModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.Signature), ModelString, model.Signature)
            .Property(nameof(model.RepresentableTypes), ModelString, model.RepresentableTypes)
            .Property(nameof(model.Relations), ModelString, model.Relations)
            .Property(nameof(model.Settings), ModelString, model.Settings, isLast: true)
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(FactoryModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.Name), model.Name)
            .Property(nameof(model.RequiresGeneration), model.RequiresGeneration, isLast: true)
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(RepresentableTypeModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.Alias), model.Alias)
            .Property(nameof(model.Options), model.Options)
            .Property(nameof(model.Storage), model.Storage)
            .Property(nameof(model.Groups), model.Groups)
            .Property(nameof(model.Factory), ModelString, model.Factory)
            .Property(nameof(model.Signature), ModelString, model.Signature, isLast: true)
            .CloseSameLineBracesBlock();
        public IndentedStringBuilder ModelString(TypeSignatureModel model) =>
            OpenSameLineBracesBlock()
            .Property(nameof(model.DeclarationKeyword), model.DeclarationKeyword)
            .Property(nameof(model.IsTypeParameter), model.IsTypeParameter)
            .Property(nameof(model.Nature), model.Nature)
            .Property(nameof(model.Names), ModelString, model.Names, isLast: true)
            .CloseSameLineBracesBlock();
        private IndentedStringBuilder Property<T>(String name, T value, Boolean isLast = false)
            where T : struct, Enum
            => Property(name, Enum, value, isLast);
        private IndentedStringBuilder Property(String name, Boolean value, Boolean isLast = false)
            => Property(name, Append, value.ToString(), isLast);
        private IndentedStringBuilder Property(String name, String value, Boolean isLast = false)
            => Property(name, Literal, value, isLast);
        private IndentedStringBuilder Property(String name, IEnumerable<String> value, Boolean isLast = false)
            => Property(name, Literals, value, isLast);
        private IndentedStringBuilder Property<T>(
            String name,
            Func<T, IndentedStringBuilder> valueAppend,
            IEnumerable<T> values,
            Boolean isLast = false)
        {
            Append(name).AppendCore(": [");

            using var enumerator = values.GetEnumerator();
            if(enumerator.MoveNext())
                _ = valueAppend.Invoke(enumerator.Current);

            while(enumerator.MoveNext())
            {
                Append(',').AppendLineCore();
                _ = valueAppend.Invoke(enumerator.Current);
            }

            AppendCore(']');
            if(!isLast)
                Append(',').AppendLineCore();

            return this;
        }
        private IndentedStringBuilder Property<T>(
            String name,
            Func<T, IndentedStringBuilder> valueAppend,
            T value,
            Boolean isLast = false)
        {
            Append(name).AppendCore(": ");
            _ = valueAppend.Invoke(value);
            if(!isLast)
                Append(',').AppendLineCore();

            return this;
        }
        private IndentedStringBuilder Enum<T>(T value)
            where T : struct, Enum
            => Append(value.ToString());
        private IndentedStringBuilder Literal(String literal)
        {
            LiteralCore(literal);
            return this;
        }
        private IndentedStringBuilder Literals(IEnumerable<String> literals)
        {
            AppendCore('[');

            using var enumerator = literals.GetEnumerator();
            if(enumerator.MoveNext())
                LiteralCore(enumerator.Current);

            while(enumerator.MoveNext())
            {
                AppendCore(',');
                LiteralCore(enumerator.Current);
            }

            AppendCore(']');

            return this;
        }
        private void LiteralCore(String literal)
        {
            if(literal == null)
                AppendCore("null");
            else
                Append('"').Append(literal).AppendCore('"');
        }
    }
}