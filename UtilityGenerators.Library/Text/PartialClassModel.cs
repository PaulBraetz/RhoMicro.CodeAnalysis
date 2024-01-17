namespace RhoMicro.CodeAnalysis.Library.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Immutable;

record PartialClassModel(
    String Namespace,
    ImmutableArray<ContainingTypeModel> ContainingTypes,
    ImmutableArray<AttributeSyntaxModel> Attributes,
    String Visibility,
    String TypeModifiers,
    String Name,
    ImmutableArray<StringOrChar> TypeParameters,
    ImmutableArray<String> ImplementedTypes) :
    IIndentedStringBuilderAppendable
{
    public PartialClassModel(
        String @namespace,
        ImmutableArray<ContainingTypeModel> containingTypes,
        String visibility,
        String typeModifiers,
        String name,
        ImmutableArray<StringOrChar> typeParameters)
        : this(
             @namespace,
             containingTypes,
             ImmutableArray.Create<AttributeSyntaxModel>(),
             visibility,
             typeModifiers,
             name,
             typeParameters,
             ImmutableArray.Create<String>())
    { }

    public static PartialClassModel Create(INamedTypeSymbol symbol)
    {
        var @namespace = symbol.ContainingNamespace.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        var containingTypesBuilder = ImmutableArray.CreateBuilder<ContainingTypeModel>();
        appendContainingType(symbol.ContainingType);
        var containingTypes = containingTypesBuilder.ToImmutable();
        var visibility = SyntaxFacts.GetText(symbol.DeclaredAccessibility);
        var typeModifiers = Utils.GetTypeModifiers(symbol);
        var name = symbol.Name;
        ImmutableArray<StringOrChar> typeParameters;
        if(symbol.TypeParameters.Length > 0)
        {
            var typeParamsBuilder = ImmutableArray.CreateBuilder<StringOrChar>();
            for(var i = 0; i < symbol.TypeParameters.Length; i++)
            {
                var param = symbol.TypeParameters[i].Name;
                typeParamsBuilder.Add(param);
            }

            typeParameters = typeParamsBuilder.ToImmutable();
        } else
        {
            typeParameters = ImmutableArray.Create<StringOrChar>();
        }

        var result = new PartialClassModel(
            @namespace,
            containingTypes,
            visibility,
            typeModifiers,
            name,
            typeParameters);

        return result;

        void appendContainingType(INamedTypeSymbol? parent)
        {
            if(parent == null)
                return;

            appendContainingType(parent.ContainingType);
            var model = ContainingTypeModel.Create(parent);
            containingTypesBuilder!.Add(model);
        }
    }

    public void AppendTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = builder.Append("namespace ")
        .AppendLine(Namespace)
        .OpenBracesBlock();
        for(var i = 0; i < ContainingTypes.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.Append(ContainingTypes[i], cancellationToken).OpenBracketsBlock();
        }

        if(!String.IsNullOrEmpty(Visibility))
        {
            _ = builder.Append(Visibility).Append(' ');
        }

        _ = builder.Append(TypeModifiers).Append(' ').Append(Name);
        if(TypeParameters.Length > 0)
        {
            _ = builder.OpenAngledBlock().AppendJoin(", ", TypeParameters, cancellationToken).CloseBlock();
        }

        if(ImplementedTypes.Length > 0)
        {
            _ = builder.Append(": ")
                .AppendLine()
                .AppendJoinLines(',', ImplementedTypes);
        }

        _ = builder.OpenBracketsBlock();
    }

    public virtual Boolean Equals(PartialClassModel? other) =>
        other != null &&
        Namespace == other.Namespace &&
        Visibility == other.Visibility &&
        TypeModifiers == other.TypeModifiers &&
        Name == other.Name &&
        TypeParameters.Length == other.TypeParameters.Length &&
        ImmutableArrayCollectionEqualityComparer<ContainingTypeModel>.Default.Equals(ContainingTypes, other.ContainingTypes) &&
        ImmutableArrayCollectionEqualityComparer<AttributeSyntaxModel>.Default.Equals(Attributes, other.Attributes) &&
        ImmutableArrayCollectionEqualityComparer<String>.Default.Equals(ImplementedTypes, other.ImplementedTypes);
    public override Int32 GetHashCode() =>
        (Namespace,
         Visibility,
         TypeModifiers,
         Name,
         TypeParameters.Length.GetHashCode(),
         ImmutableArrayCollectionEqualityComparer<ContainingTypeModel>.Default.GetHashCode(ContainingTypes),
         ImmutableArrayCollectionEqualityComparer<AttributeSyntaxModel>.Default.GetHashCode(Attributes),
         ImmutableArrayCollectionEqualityComparer<String>.Default.GetHashCode(ImplementedTypes))
        .GetHashCode();
}