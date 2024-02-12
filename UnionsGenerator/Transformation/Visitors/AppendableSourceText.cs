namespace RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;

using System.Collections.Immutable;
using System.Xml.Serialization;

using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using static Library.Text.IndentedStringBuilder.Appendables;
using System.Diagnostics.CodeAnalysis;

sealed partial class AppendableSourceText : IIndentedStringBuilderAppendable
{
    private readonly UnionTypeModel _target;
    private readonly StrategySourceHost _storageHost;
    private readonly IReadOnlyDictionary<RepresentableTypeModel, StorageStrategy> _storageStrategies;

    private AppendableSourceText(UnionTypeModel target, StrategySourceHost storageHost, IReadOnlyDictionary<RepresentableTypeModel, StorageStrategy> storageStrategies)
    {
        _target = target;
        _storageHost = storageHost;
        _storageStrategies = storageStrategies;
    }

    /// <summary>
    /// Creates a new appendable source text instance.
    /// </summary>
    /// <param name="target">The target model to create source text for.</param>
    /// <returns>A new instance of <see cref="AppendableSourceText"/>.</returns>
    internal static AppendableSourceText Create(UnionTypeModel target)
    {
        _ = target ?? throw new ArgumentNullException(nameof(target));

        var storageHost = new StrategySourceHost(target);
        var storageStrategies = new Dictionary<RepresentableTypeModel, StorageStrategy>();
        for(var i = 0; i < target.RepresentableTypes.Count; i++)
        {
            var representableType = target.RepresentableTypes[i];
            var strategy = StorageStrategy.Create(target, representableType);
            strategy.Visit(storageHost);
            storageStrategies.Add(representableType, strategy);
        }

        var result = new AppendableSourceText(target, storageHost, storageStrategies);

        return result;
    }

    /// <inheritdoc/>
    public void AppendTo(IndentedStringBuilder builder)
    {
        Throw.ArgumentNull(builder, nameof(builder));

        var signature = _target.Signature;

        if(!String.IsNullOrEmpty(signature.Names.Namespace))
            builder.Append("namespace ").Append(signature.Names.Namespace).OpenBlockCore(Blocks.Braces(builder.Options.NewLine));

        builder.AppendLine("using System.Linq;")
            .AppendCore(ScopedData);

        foreach(var containingType in signature.ContainingTypes)
        {
            builder.Append("partial ").Append(containingType.DeclarationKeyword)
                .Append(' ').Append(containingType.Names.GenericName).OpenBlockCore(Blocks.Braces(builder.Options.NewLine));
        }

        if(_target.Settings.Miscellaneous.HasFlag(MiscellaneousSettings.GenerateJsonConverter))
        {
            builder.Append("[System.Text.Json.Serialization.JsonConverter(typeof(")
                .Append(_target.Signature.Names.OpenGenericName)
                .Append(".JsonConverter))]")
                .AppendLineCore();
        }

        builder.Append("partial ").Append(signature.DeclarationKeyword).Append(' ').Append(signature.Names.GenericName)
            .Append(" : System.IEquatable<").Append(signature.Names.GenericName).Append(b =>
            {
                if(_target.Signature.Nature == TypeNature.ReferenceType)
                    b.AppendCore('?');
            }).Append('>')
            .OpenBracesBlock()
                .Append(NestedTypes)
                .Append(Constructors)
                .Append(Fields)
                .Append(Factories)
                .Append(Switch)
                .Append(Match)
                .Append(RepresentedTypes)
                .Append(IsAsProperties)
                .Append(IsGroupProperties)
                .Append(IsAsFunctions)
                .Append(ToString)
                .Append(GetHashCode)
                .Append(Equals)
                .Append(Conversions)
            .CloseAllBlocksCore();
        //InterfaceIntersections,
    }
    public void IsGroupProperties(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Is Group Properties")
            .Append(b =>
                b.AppendJoin(
                    StringOrChar.Empty,
                    _target.Groups.Groups,
                    (b, group) =>
                    {
                        var (name, members) = group;
                        b.Comment.OpenSummary()
                        .Comment.OpenParagraph()
                        .Append("Gets a value indicating whether the currently represented value is part of the ").Append(name).Append(" group.")
                        .CloseBlock()
                        .Comment.OpenParagraph()
                        .Append("The group encompasses values of the following types:")
                        .CloseBlock()
                        .Comment.OpenParagraph()
                        .Comment.OpenList("bullet")
                        .AppendJoin(
                            StringOrChar.Empty,
                            members,
                            (b, m) => b.Comment.OpenItem().Comment.Ref(m.Signature).CloseBlockCore())
                        .CloseBlock()
                        .CloseBlock()
                        .CloseBlock()
                        .Append("public System.Boolean Is").Append(name).AppendLine("Group => ")
                        .TagSwitchExpr(
                            _target,
                            (b, m) => b.Append(members.Contains(m) ? "true" : "false"))
                        .Append(';')
                        .AppendLineCore();
                    }))
            .CloseBlockCore();
    }
    public void Conversions(IndentedStringBuilder builder) =>
        builder.OpenRegionBlock("Conversions")
            .Append(RepresentableTypeConversions)
            .Append(RelatedTypeConversions)
            .CloseBlockCore();
    public void RepresentableTypeConversions(IndentedStringBuilder builder)
    {
#pragma warning disable IDE0047 // Remove unnecessary parentheses
        var convertableTypes = _target.RepresentableTypes
            .Where(t => !( t.OmitConversionOperators /*|| t.Signature.IsTypeParameter && t.Options.HasFlag(UnionTypeOptions.SupersetOfParameter) */));
#pragma warning restore IDE0047 // Remove unnecessary parentheses
        builder.OpenRegionBlock("Representable Type Conversions")
            .AppendJoinLines(
            StringOrChar.Empty,
            convertableTypes,
            (b, representableType) =>
            {
                b.Comment.OpenSummary()
                    .Append("Converts an instance of the representable type ")
                    .Comment.Ref(representableType.Signature)
                    .Append(" to the union type ")
                    .Comment.Ref(_target.Signature).Append('.')
                    .CloseBlock()
                    .Comment.OpenParam("value")
                    .Append("The value to convert.")
                    .CloseBlock()
                    .Comment.OpenReturns()
                    .Append("The union type instance.")
                    .CloseBlock()
                    .Append("public static implicit operator ").Append(_target.Signature.Names.GenericName)
                    .Append('(')
                    .Append(representableType.Signature.Names.FullGenericNullableName).Append(" value) => ")
                    .Append(representableType.Factory.Name).Append("(value);")
                    .AppendLineCore();

                var generateSolitaryExplicit =
                    _target.RepresentableTypes.Count > 1 ||
                    !representableType.Options.HasFlag(UnionTypeOptions.ImplicitConversionIfSolitary);

                if(generateSolitaryExplicit)
                {
                    b.Comment.OpenSummary()
                        .Append("Converts an instance of the union type ")
                        .Comment.Ref(_target.Signature)
                        .Append(" to the representable type ")
                        .Comment.Ref(representableType.Signature).Append('.')
                        .CloseBlock()
                        .Comment.OpenParam("union")
                        .Append("The union to convert.")
                        .CloseBlock()
                        .Comment.OpenReturns()
                        .Append("The represented value.")
                        .CloseBlock()
                        .Append("public static explicit operator ")
                        .Append(representableType.Signature.Names.FullGenericNullableName)
                        .Append('(')
                        .Append(_target.Signature.Names.FullGenericName)
                        .AppendCore(" union) =>");

                    if(_target.RepresentableTypes.Count > 1)
                    {
                        b.Append("union.").Append(_target.Settings.TagFieldName).Append(" == ")
                            .Append(_target.Settings.TagTypeName)
                            .Append('.')
                            .Append(representableType.Alias)
                            .Append('?')
                            .AppendLineCore();
                    }

                    b.AppendCore(_storageStrategies[representableType].StrongInstanceVariableExpression("union"));

                    if(_target.RepresentableTypes.Count > 1)
                    {
                        _ = b.Append(':')
                        .InvalidConversionThrow($"typeof({representableType.Signature.Names.FullGenericName}).Name");
                    }

                    b.AppendCore(';');
                } else
                {
                    b.Append("public static implicit operator ")
                        .Append(representableType.Signature.Names.FullGenericNullableName)
                        .Append('(')
                        .Append(_target.Signature.Names.FullGenericName)
                        .Append(" union) => ")
                        .Append(_storageStrategies[representableType].StrongInstanceVariableExpression("union"))
                        .Append(';')
                        .AppendLineCore();
                }
            })
            .CloseBlockCore();
    }
    public void RelatedTypeConversions(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Related Type Conversions")
            .AppendJoinLines(
            StringOrChar.Empty,
                _target.Relations.Where(r => r.RelationType != RelationType.None),
                (b, relation) =>
                {
                    var relationTypeSet = relation.RelatedType.RepresentableTypes
                        .Select(t => t.Names.FullGenericNullableName)
                        .ToImmutableHashSet();
                    //we need two maps because each defines different access to the corresponding AsXX etc. properties
                    var targetTypeMap = relation.RelatedType.RepresentableTypes
                        .Where(t => relationTypeSet.Contains(t.Names.FullGenericNullableName))
                        .ToDictionary(t => t.Names.FullGenericNullableName);
                    var relationTypeMap = relation.RelatedType.RepresentableTypes
                        .Where(t => targetTypeMap.ContainsKey(t.Names.FullGenericNullableName))
                        .ToDictionary(t => t.Names.FullGenericNullableName);

                    //conversion to model from relation
                    //public static _plicit operator Target(Relation relatedUnion)
                    var relationType = relation.RelationType;
                    b.OpenRegionBlock($"{relationType switch
                    {
                        RelationType.Congruent => "Congruency with ",
                        RelationType.Intersection => "Intersection with ",
                        RelationType.Superset => "Superset of ",
                        RelationType.Subset => "Subset of ",
                        _ => "Relation"
                    }} {relation.RelatedType.Signature.Names.GenericName}")
                    .Append("public static ")
                    .Append(relationType is RelationType.Congruent or RelationType.Superset ? "im" : "ex")
                    .Append("plicit operator ").Append(_target.Signature.Names.GenericName)
                    .Append('(')
                    .Append(relation.RelatedType.Signature.Names.FullGenericName)
                    .Append(" relatedUnion) =>")
                    .Append(b =>
                    {
                        if(targetTypeMap.Count == 1)
                        {
                            b.AppendCore("/*unknown conversion*/");
                            //UnknownConversion(
                            //    builder,
                            //    model,
                            //    relationTypeMap.Single().Value,
                            //    targetTypeMap.Single().Value,
                            //    "relatedUnion");
                        } else
                        {
                            b.AppendCore("/*type switch expression*/");
                            //TypeSwitchExpression(
                            //    builder,
                            //    targetTypeMap,
                            //    (b) => _ = b.WithOperators(builder.CancellationToken) * (b => UtilFullString(b, b => _ = b * "relatedUnion.RepresentedType")),
                            //    (b, m) => _ = b * m.Value.Names.TypeStringName,
                            //    (b, m) => _ = b.WithOperators(builder.CancellationToken) * (b => UnknownConversion(b, model, relationTypeMap[m.Key], m.Value, "relatedUnion")),
                            //    (b) => _ = b.WithOperators(builder.CancellationToken) % (InvalidConversionThrow, $"typeof({model.Symbol.ToFullOpenString()})"));
                        }
                    })
                    .AppendCore(';');

                    //conversion to relation from model
                    //public static _plicit operator Relation(Target relatedUnion)
                    b.Append("public static ")
                    .Append(relationType is RelationType.Congruent or RelationType.Subset ? "im" : "ex")
                    .Append("plicit operator ").Append(relation.RelatedType.Signature.Names.FullGenericName)
                    .Append('(')
                    .Append(_target.Signature.Names.FullGenericName)
                    .Append(" union) => ")
                    .Append(b =>
                    {
                        if(relationTypeMap.Count == 1)
                        {
                            b.AppendCore("/*known conversion*/");
                            //KnownConversion(
                            //    builder,
                            //    relation,
                            //    targetTypeMap.Single().Value,
                            //    relationTypeMap.Single().Value,
                            //    "union");
                            _ = builder.AppendLine();
                        } else
                        {
                            b.AppendCore("/*switch expression*/");
                            //SwitchExpression(
                            //    builder,
                            //    relationTypeMap,
                            //    (b) => _ = b * "union.__tag",
                            //    (b, kvp) => _ = b * targetTypeMap[kvp.Key].GetCorrespondingTag(model),
                            //    (b, kvp) => _ = b * (b => KnownConversion(b, relation, targetTypeMap[kvp.Key], kvp.Value, "union")),
                            //    (b) => _ = b % (InvalidConversionThrow, $"typeof({relation.Symbol.ToFullOpenString()})"));
                        }
                    })
                    .AppendCore(';');
                    b.CloseBlockCore();
                })
            .CloseBlockCore();
    }
    public void Equals(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Equality")
            .Comment.InheritDoc()
            .AppendLine("public override System.Boolean Equals(System.Object? obj) =>")
            .Indent()
                .Append("obj is ").Append(_target.Signature.Names.GenericName).AppendLine(" union && Equals(union);")
            .Detent()
            .Comment.InheritDoc()
            .Append("public System.Boolean Equals(").Append(_target.Signature.Names.GenericName).Append(b =>
            {
                if(_target.Signature.Nature == TypeNature.ReferenceType)
                    b.AppendCore('?');
            }).AppendLine(" other) =>")
            .Indent()
                .Append(b =>
                {
                    if(_target.Signature.Nature == TypeNature.ReferenceType)
                    {
                        b.AppendLine("ReferenceEquals(other, this)")
                            .AppendLine("|| other != null")
                            .AppendCore("&& ");
                    }

                    if(_target.RepresentableTypes.Count > 1)
                    {
                        b.Append("this.").Append(_target.Settings.TagFieldName)
                            .Append(" == other.")
                            .AppendLine(_target.Settings.TagFieldName)
                            .AppendCore("&& ");
                    }
                }).TagSwitchExpr(
                    _target,
                    (b, t) => b.Append(_storageStrategies[t].EqualsInvocation("other")))
                .AppendLine(';')
            .Detent()
            .Append(b =>
            {
                if(_target.Signature.Nature is TypeNature.PureValueType or TypeNature.ImpureValueType)
                {
                    b.Append("public static System.Boolean operator ==(")
                    .Append(_target.Signature.Names.GenericName).Append(" a, ")
                    .Append(_target.Signature.Names.GenericName).AppendLine(" b) => a.Equals(b);")
                    .Append("public static System.Boolean operator !=(")
                    .Append(_target.Signature.Names.GenericName).Append(" a, ")
                    .Append(_target.Signature.Names.GenericName).AppendCore(" b) => !a.Equals(b);");
                }
            })
            .CloseBlockCore();
    }
    public void GetHashCode(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("GetHashCode")
            .Comment.InheritDoc()
            .AppendLine("public override System.Int32 GetHashCode() => ")
            .TagSwitchExpr(
                _target,
                (b, t) => b.Append(_storageStrategies[t].GetHashCodeInvocation()))
            .Append(';')
            .CloseBlockCore();
    }
    public void ToString(IndentedStringBuilder builder)
    {
        if(!_target.Settings.IsToStringImplementationRequired)
            return;

        builder.OpenRegionBlock("ToString")
            .Comment.InheritDoc()
            .Append(b =>
            {
                if(_target.Settings.ToStringSetting == ToStringSetting.Simple)
                {
                    builder.Append("public override System.String ToString() =>")
                        .Append(simpleToStringExpression)
                        .AppendCore(';');
                } else
                {
                    builder.Append("public override System.String ToString()")
                        .OpenBracesBlock()
                        .Append("var stringRepresentation = ").Append(simpleToStringExpression)
                        .AppendLine(';')
                        .Append("var result = $\"").Append(_target.Signature.Names.GenericName)
                        .Append('(')
                        .Append(b =>
                        {
                            if(_target.RepresentableTypes.Count == 1)
                            {
                                b.Append('<').Append(_target.RepresentableTypes[0].Signature.Names.GenericName).AppendCore('>');
                            } else
                            {
                                _ = b.AppendJoin(
                                    '|',
                                    _target.RepresentableTypes,
                                    (b, t) => b.Append("{(")
                                    .Append(_target.Settings.TagFieldName).Append(" == ")
                                    .Append(_target.Settings.TagTypeName).Append('.').Append(t.Alias)
                                    .Append(" ? \"<").Append(t.Alias).Append(">\" : \"").Append(t.Alias)
                                    .AppendCore("\")}"));
                            }
                        })
                        .AppendLine("){{{stringRepresentation}}}\";")
                        .AppendLine("return result;")
                        .CloseBlockCore();
                }
            })
            .CloseBlockCore();

        void simpleToStringExpression(IndentedStringBuilder b)
        {
            if(_target.RepresentableTypes.Count > 1)
            {
                _ = b.TagSwitchExpr(
                    _target,
                    (b, t) => b.AppendCore(_storageStrategies[t].ToStringInvocation()));
            } else
            {
                b.AppendCore(
                    _storageStrategies.Single().Value.ToStringInvocation());
            }
        }
    }
    public void IsAsFunctions(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Is/As Functions")
            .AppendJoin(
                StringOrChar.Empty,
                _target.RepresentableTypes,
                (b, t) =>
                {
                    b.Comment.OpenSummary()
                        .Append("Determines whether this instance is representing a value of type ")
                        .Comment.Ref(t.Signature).Append('.')
                    .CloseBlock()
                    .Comment.OpenReturns()
                        .Comment.Langword("true").Append(" if this instance is representing a value of type ")
                        .Comment.Ref(t.Signature).Append("; otherwise, ")
                        .Comment.Langword("false").Append('.')
                    .CloseBlock()
                    .Comment.OpenParam("value")
                        .Append("If this instance is representing a value of type ").Comment.Ref(t.Signature)
                        .Append(", this parameter will contain that value; otherwise, ").Comment.Langword("default").Append('.')
                    .CloseBlock()
                    .Append("public System.Boolean TryAs").Append(t.Alias).Append('(')
                    .Append(b =>
                    {
                        if(t.Signature.Nature is not TypeNature.ReferenceType)
                            return;
                        b.AppendCore("[System.Diagnostics.CodeAnalysis.NotNullWhen(true)]");
                    })
                    .Append(" out ").Append(t.Signature.Names.FullGenericNullableName).Append(" value)")
                    .OpenBracesBlock()
                        .Append(b =>
                        {
                            if(_target.RepresentableTypes.Count == 1)
                            {
                                b.Append("value = ").Append(_storageStrategies.Single().Value.StrongInstanceVariableExpression("this")).AppendLine(';')
                                .AppendCore("return true;");
                            } else
                            {
                                b.Append("if(")
                                .Append("this").Append('.').Append(_target.Settings.TagFieldName)
                                .Append(" == ")
                                .Append(_target.Settings.TagTypeName).Append('.').Append(t.Alias).Append(')')
                                .OpenBracesBlock()
                                    .Append("value = ").Append(_storageStrategies[t].StrongInstanceVariableExpression("this")).AppendLine(';')
                                    .Append("return true;")
                                .CloseBlock()
                                .AppendLine("value = default;")
                                .AppendCore("return false;");
                            }
                        })
                    .CloseBlockCore();
                })
            .Comment.OpenSummary()
                .Append("Determines whether this instance is representing a value of type ")
                .Comment.TypeParamRef(_target.Settings.GenericTValueName).Append('.')
            .CloseBlock()
            .Comment.OpenTypeParam(_target.Settings.GenericTValueName)
                .Append("The type whose representation in this instance to determine.")
            .CloseBlock()
            .Comment.OpenReturns()
                .Comment.Langword("true").Append(" if this instance is representing a value of type ")
                .Comment.TypeParamRef(_target.Settings.GenericTValueName).Append("; otherwise, ")
                .Comment.Langword("false").Append('.')
            .CloseBlock()
            .Append("public System.Boolean Is<").Append(_target.Settings.GenericTValueName).Append(">() =>")
            .Append(b =>
            {
                _ = _target.RepresentableTypes.Count > 1
                    ? b.Append("typeof(").Append(_target.Settings.GenericTValueName).Append(") ==")
                        .TagSwitchExpr(
                            _target,
                            (b, a) => b.Typeof(a.Signature))
                    : b.Append("typeof(").Append(_target.Settings.GenericTValueName).Append(") == ")
                        .Typeof(_target.RepresentableTypes[0].Signature);
            })
            .AppendLine(';')
            .Comment.OpenSummary()
                .Append("Determines whether this instance is representing a value of type ")
                .Comment.TypeParamRef(_target.Settings.GenericTValueName).Append('.')
            .CloseBlock()
            .Comment.OpenParam("value")
                .Append("If this instance is representing a value of type ").Comment.TypeParamRef(_target.Settings.GenericTValueName)
                .Append(", this parameter will contain that value; otherwise, ").Comment.Langword("default").Append('.')
            .CloseBlock()
            .Comment.OpenTypeParam(_target.Settings.GenericTValueName)
                .Append("The type whose representation in this instance to determine.")
            .CloseBlock()
            .Comment.OpenReturns()
                .Comment.Langword("true").Append(" if this instance is representing a value of type ")
                .Comment.TypeParamRef(_target.Settings.GenericTValueName)
                .Append("; otherwise, ").Comment.Langword("false").Append('.')
            .CloseBlock()
            .Append("public System.Boolean Is<").Append(_target.Settings.GenericTValueName).Append(">(out ")
            .Append(_target.Settings.GenericTValueName).Append("? value)")
            .OpenBracesBlock()
                .Append(b =>
                {
                    if(_target.RepresentableTypes.Count > 1)
                    {
                        _ = b.MetadataNameSwitchStmt(
                            _target,
                            b => b.Append("typeof(").Append(_target.Settings.GenericTValueName).Append(')'),
                            (b, a) => b.Append("value = ").Append(
                                    StorageStrategy.Create(_target, a).ConvertedInstanceVariableExpression(_target.Settings.GenericTValueName))
                                .AppendLine(';').Append("return true;"),
                            b => b.AppendLine("value = default;").Append("return false;"));
                    } else
                    {
                        b.Append("if(typeof(").Append(_target.Settings.GenericTValueName).Append(") == ")
                        .Typeof(_target.RepresentableTypes[0].Signature).Append(")")
                        .OpenBracesBlock()
                            .Append("value = ").Append(
                                StorageStrategy.Create(_target, _target.RepresentableTypes[0])
                                .ConvertedInstanceVariableExpression(_target.Settings.GenericTValueName))
                            .AppendLine(';')
                            .Append("return true;")
                        .CloseBlock()
                        .Append("else")
                        .OpenBracesBlock()
                            .AppendLine("value = default;")
                            .Append("return false;")
                        .CloseBlockCore();
                    }
                })
            .CloseBlock()
            .Comment.OpenSummary()
                .Append("Determines whether this instance is representing an instance of ")
                .Comment.ParamRef("type").Append('.')
            .CloseBlock()
            .Comment.OpenParam("type")
                .Append("The type whose representation in this instance to determine.")
            .CloseBlock()
            .Comment.OpenReturns()
                .Comment.Langword("true").Append(" if this instance is representing an instance of ")
                .Comment.ParamRef("type").Append("; otherwise, ").Comment.Langword("false").Append('.')
            .CloseBlock()
            .AppendLine("public System.Boolean Is(System.Type type) =>")
            .Append(b =>
            {
                _ = _target.RepresentableTypes.Count > 0
                    ? b.Append("type == ").TagSwitchExpr(
                        _target,
                        (b, a) => b.Typeof(a.Signature))
                    : b.Append("type == ").Typeof(_target.RepresentableTypes[0].Signature);
            })
            .AppendLine(';')
            .Comment.OpenSummary()
                .Append("Retrieves the value represented by this instance as an instance of ")
                .Comment.TypeParamRef(_target.Settings.GenericTValueName).Append('.')
            .CloseBlock()
            .Comment.OpenTypeParam(_target.Settings.GenericTValueName)
                .Append("The type to retrieve the represented value as.")
            .CloseBlock()
            .Comment.OpenReturns()
                .Append("The currently represented value as an instance of ").Comment.TypeParamRef(_target.Settings.GenericTValueName).Append('.')
            .CloseBlock()
            .Append("public ").Append(_target.Settings.GenericTValueName)
            .Append(" As<").Append(_target.Settings.GenericTValueName).AppendLine(">() =>")
            .Append(b =>
            {
                if(_target.RepresentableTypes.Count > 1)
                {
                    b.TagSwitchExpr(
                        _target,
                        (b, a) => b.Append("typeof(").Append(_target.Settings.GenericTValueName).Append(") == ")
                            .Typeof(a.Signature).AppendLine()
                            .Append("? ").AppendLine(StorageStrategy.Create(_target, a).ConvertedInstanceVariableExpression(_target.Settings.GenericTValueName))
                            .Append(": ").InvalidConversionThrow($"typeof({_target.Settings.GenericTValueName}).FullName"))
                    .AppendCore(';');
                } else
                {
                    b.Append("typeof(").Append(_target.Settings.GenericTValueName).Append(") == ")
                    .Typeof(_target.RepresentableTypes[0].Signature).AppendLine()
                    .Append("? ").Append(
                        StorageStrategy.Create(_target, _target.RepresentableTypes[0])
                        .ConvertedInstanceVariableExpression(_target.Settings.GenericTValueName))
                        .AppendLine()
                    .Append(": ").InvalidConversionThrow($"typeof({_target.Settings.GenericTValueName}).FullName")
                    .AppendCore(';');
                }
            })
            .CloseBlockCore();
    }
    public void IsAsProperties(IndentedStringBuilder builder)
    {
        _ = builder.OpenRegionBlock("Is/As Properties");
        if(_target.RepresentableTypes.Count > 1)
        {
            builder.AppendJoinLines(
                StringOrChar.Semicolon,
                _target.RepresentableTypes,
                (b, a) =>
                b.Comment.OpenSummary()
                    .Append("Gets a value indicating whether this instance is representing a value of type ").Comment.Ref(a.Signature).Append('.')
                .CloseBlock()
                .Append("public System.Boolean Is").Append(a.Alias).Append(" => ")
                .Append(_target.Settings.TagFieldName).Append(" == ")
                .Append(_target.Settings.TagTypeName).Append('.').Append(a.Alias))
            .AppendLine(';')
            .AppendJoinLines(
                StringOrChar.Semicolon,
                _target.RepresentableTypes,
                (b, a) =>
                b.Comment.OpenSummary()
                    .Append("Retrieves the value represented by this instance as a ").Comment.Ref(a.Signature).Append('.')
                .CloseBlock()
                .Append("public ").Append(a.Signature.Names.FullGenericNullableName).Append(" As").Append(a.Alias).Append(" => ")
                .Append(_target.Settings.TagFieldName).Append(" == ")
                .Append(_target.Settings.TagTypeName).Append('.').AppendLine(a.Alias)
                .Append("? ").AppendLine(StorageStrategy.Create(_target, a).StrongInstanceVariableExpression("this"))
                .Append(": ").InvalidConversionThrow($"\"{a.Signature.Names.FullGenericName}\""))
            .AppendCore(';');
        } else
        {
            var attribute = _target.RepresentableTypes[0];
            builder.Comment.OpenSummary()
                    .Append("Gets a value indicating whether this instance is representing a value of type ").Comment.Ref(attribute.Signature).Append('.')
                .CloseBlock()
                .Append("public System.Boolean Is").Append(attribute.Alias).AppendLine(" => true;")
                .Comment.OpenSummary()
                    .Append("Retrieve the value represented by this instance as a ").Comment.Ref(attribute.Signature).Append('.')
                .CloseBlock()
                .Append("public ").Append(attribute.Signature.Names.FullGenericName).Append(" As").Append(attribute.Alias).Append(" => ")
                .Append(StorageStrategy.Create(_target, attribute).StrongInstanceVariableExpression("this"))
                .AppendCore(';');
        }

        builder.CloseBlockCore();
    }
    public void RepresentedTypes(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Represented Type")
            .Comment.OpenSummary()
                .Append("Gets the types of value this union type can represent.")
            .CloseBlock()
            .AppendLine("public static System.Collections.Generic.IReadOnlyCollection<System.Type> RepresentableTypes { get; } = ")
            .Indent()
                .Append(_target.ScopedDataTypeName).Append(".RepresentableTypes;")
            .Detent()
            .Comment.OpenSummary()
                .Append("Gets the type of value represented by this instance.")
            .CloseBlock()
            .AppendLine("public System.Type RepresentedType => ")
            .TagSwitchExpr(
                _target,
                (b, a) => b.Typeof(a.Signature))
            .Append(';')
        .CloseBlockCore();
    }
    public void Match(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Match")
            .Comment.OpenSummary()
                .Append("Invokes a projection based on the type of value being represented.")
            .CloseBlockCore();

        foreach(var attribute in _target.RepresentableTypes)
        {
            builder.Comment.OpenParam($"on{attribute.Alias}")
                .Append("The projection to invoke if the union is currently representing an instance of ").Comment.Ref(attribute.Signature).Append('.')
            .CloseBlockCore();
        }

        builder.Comment.OpenTypeParam(_target.Settings.MatchTypeName)
            .Append("The type of value produced by the projections passed.")
            .CloseBlock()
            .Comment.OpenReturns()
            .Append("The projected value.")
            .CloseBlock()
            .Append("public ").Append(_target.Settings.MatchTypeName).Append(" Match<").Append(_target.Settings.MatchTypeName).AppendLine(">(")
            .Indent()
                .AppendJoinLines(_target.RepresentableTypes
                    .Select<RepresentableTypeModel, Action<IndentedStringBuilder>>(a => b =>
                        b.Append("System.Func<").Append(a.Signature.Names.FullGenericNullableName).Append(", ")
                        .Append(_target.Settings.MatchTypeName).Append("> on").Append(a.Alias)))
                .AppendLine(") =>")
            .Detent()
            .TagSwitchExpr(
                _target,
                (b, t) =>
                    b.Append("on").Append(t.Alias).Append(".Invoke(")
                    .Append(StorageStrategy.Create(_target, t).StrongInstanceVariableExpression("this"))
                    .AppendLine(")"))
            .Append(';')
        .CloseBlockCore();
    }
    public void Switch(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Switch")
            .Comment.OpenSummary()
                .Append("Invokes a handler based on the type of value being represented.")
            .CloseBlockCore();

        foreach(var attribute in _target.RepresentableTypes)
        {
            builder.Comment.OpenParam($"on{attribute.Alias}")
                .Append("The handler to invoke if the union is currently representing an instance of ").Comment.Ref(attribute.Signature).Append('.')
            .CloseBlockCore();
        }

        builder.AppendLine("public void Switch(")
            .Indent()
                .AppendJoinLines(_target.RepresentableTypes
                    .Select<RepresentableTypeModel, Action<IndentedStringBuilder>>(a => b =>
                        b.Append("System.Action<").Append(a.Signature.Names.FullGenericNullableName).Append("> on").Append(a.Alias)))
                .AppendLine(")")
            .Detent()
            .OpenBracesBlock()
                .TagSwitchStmt(
                    _target,
                    (b, t) =>
                        b.Append("on").Append(t.Alias).Append(".Invoke(")
                        .Append(StorageStrategy.Create(_target, t).StrongInstanceVariableExpression("this"))
                        .AppendLine(");")
                        .Append("return;"))
            .CloseBlock()
        .CloseBlockCore();
    }
    public void ScopedData(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Scoped Data")
            .Append("file static class ").Append(_target.ScopedDataTypeName)
            .OpenBracesBlock()
                .AppendLine("public static System.Collections.Concurrent.ConcurrentDictionary<System.Type, System.Object> Cache { get; } = new();")
                .AppendLine("public static System.Collections.Generic.HashSet<System.Type> RepresentableTypes { get; } = ")
                .AppendLine("new ()")
                .OpenBracesBlock()
                    .AppendJoinLines(
                        _target.RepresentableTypes,
                        (b, a) => b.Typeof(a.Signature))
                .CloseBlock()
                .AppendLine(';')
            .CloseBlock()
        .CloseBlockCore();
    }
    public void Factories(IndentedStringBuilder builder)
    {
        var tValueName = _target.Settings.GenericTValueName;
        builder.OpenRegionBlock("Factories")
            .AppendJoin(
                StringOrChar.Empty,
                _target.RepresentableTypes.Where(a => a.Factory.RequiresGeneration)
                    .Select<RepresentableTypeModel, Action<IndentedStringBuilder>>(a => b =>
                    {
                        b.Comment.OpenSummary()
                        .Append("Creates a new instance of ").Comment.SeeRef(_target)
                        .Append(" representing an instance of ").Comment.Ref(a.Signature).Append('.')
                        .CloseBlock()
                        .Comment.OpenParam("value")
                        .Append("The value to be represented by the new instance of ").Comment.SeeRef(_target).Append('.')
                        .CloseBlock()
                        .Comment.OpenReturns()
                        .Append("A new instance of ").Comment.SeeRef(_target).Append(" representing ").Comment.ParamRef("value").Append('.')
                        .CloseBlock()
                        .Append("public static ").Append(_target.Signature.Names.GenericName).Append(' ')
                        .Append(a.Factory.Name).Append("([RhoMicro.CodeAnalysis.UnionTypeFactory]").Append(a.Signature.Names.FullGenericNullableName)
                        .Append(" value) => new(value);").AppendLineCore();
                    }))
            .Comment.OpenSummary()
            .Append("Attempts to create an instance of ").Comment.SeeRef(_target)
            .Append(" from an instance of ").Comment.TypeParamRef(tValueName).Append('.')
            .CloseBlock()
            .Comment.OpenParam("value")
            .Append("The value from which to attempt to create an instance of ").Comment.SeeRef(_target).Append('.')
            .CloseBlock()
            .Comment.OpenParam("result")
            .Append("If an instance of ").Comment.SeeRef(_target)
            .Append(" could successfully be created, this parameter will contain the newly created instance; otherwise, ")
            .Comment.Langword("default").Append('.')
            .CloseBlock()
            .Comment.OpenReturns()
            .Comment.Langword("true").Append(" if an instance of ").Comment.SeeRef(_target).Append(" could successfully be created; otherwise, ")
            .Comment.Langword("false").AppendLine('.')
            .CloseBlock()
            .Append("public static System.Boolean TryCreate<").Append(tValueName)
            .Append(">(").Append(tValueName).Append(" value, ").Append(b =>
            {
                if(_target.Signature.Nature == TypeNature.ReferenceType)
                {
                    b.AppendCore("[System.Diagnostics.CodeAnalysis.NotNullWhen(true)]");
                }
            }).Append(" out ")
            .Append(_target.Signature.Names.GenericName).Append(b =>
            {
                if(_target.Signature.Nature == TypeNature.ReferenceType)
                {
                    b.AppendCore('?');
                }
            }).Append(" result)")
            .OpenBracesBlock()
            .MetadataNameSwitchStmt(
                target: _target,
                typeExpr: b => b.Append("typeof(").Append(tValueName).Append(")"),
                caseBody: (b, t) => _ = b.Operators +
                    "result = " + t.Factory.Name + '(' + UtilUnsafeConvert(tValueName, t.Signature, "value") + ");" + NewLine +
                    "return true;",
                defaultBody: b => b.Append(TryCreateDefaultCase))
            .CloseBlock()
            .Comment.OpenSummary()
            .Append("Creates an instance of ").Comment.SeeRef(_target).Append(" from an instance of ").Comment.TypeParamRef(tValueName).Append('.')
            .CloseBlock()
            .Comment.OpenParam("value")
            .Append("The value from which to create an instance of ").Comment.SeeRef(_target).Append('.')
            .CloseBlock()
            .Comment.OpenReturns()
            .Append("A new instance of ").Comment.SeeRef(_target).Append(" representing ").Comment.ParamRef("value").Append('.')
            .CloseBlock()
            .Append("public static ").Append(_target.Signature.Names.GenericName)
            .Append(" Create<").Append(tValueName).Append(">(").Append(tValueName).Append(" value)")
            .OpenBracesBlock()
            .MetadataNameSwitchStmt(
                target: _target,
                typeExpr: b => b.Append("typeof(").Append(tValueName).Append(")"),
                caseBody: (b, t) => _ = b.Operators +
                    "return " + t.Factory.Name + '(' + UtilUnsafeConvert(tValueName, t.Signature, "value") + ");",
                defaultBody: b => b.Append(CreateDefaultCase))
            .CloseBlock()
            .CloseBlockCore();
    }
    public void CreateDefaultCase(IndentedStringBuilder builder)
    {
        builder.Append("var sourceType = typeof(").Append(_target.Settings.GenericTValueName).AppendLine(");")
            .Append("if(!").Append(_target.ScopedDataTypeName).Append(".Cache.TryGetValue(sourceType, out var weakMatch))")
            .OpenBracesBlock()
                .Append("if(!").Append(b => b.UtilIsMarked("sourceType")).Append(')')
                .OpenBracesBlock()
                    .Append(invalidCreationThrow)
                .CloseBlock()
                .Append(ConversionCacheWeakMatchExpr)
            .CloseBlock()
            .Append("var match = (System.Func<TValue, (System.Boolean, ").Append(_target.Signature.Names.FullGenericName).AppendLine(")>)weakMatch;")
            .AppendLine("var matchResult = match.Invoke(value);")
            .AppendLine("if(!matchResult.Item1)")
            .OpenBracesBlock()
                .Append(invalidCreationThrow)
            .CloseBlock()
            .Append("return matchResult.Item2;")
            .AppendLineCore();

        void invalidCreationThrow(IndentedStringBuilder builder) =>
            builder.Append("throw new System.InvalidOperationException($\"Unable to create an instance of ")
                .Append(_target.Signature.Names.FullGenericName)
                .Append(" from an instance of {typeof(").Append(_target.Settings.GenericTValueName).AppendCore(")}.\");");
    }
    public void ConversionCacheWeakMatchExpr(IndentedStringBuilder builder) =>
        builder.Append("weakMatch = ").Append(_target.ScopedDataTypeName).Append(".Cache.GetOrAdd(sourceType, t =>")
        .OpenBracesBlock()
            .Append("var tupleType = typeof(System.ValueTuple<System.Boolean, ").Append(_target.Signature.Names.GenericName).AppendLine(">);")
            .AppendLine("var matchMethod = sourceType.GetMethod(nameof(Match), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)")
            .Indent()
                .AppendLine("?.MakeGenericMethod(tupleType) ??")
                .AppendLine("throw new System.InvalidOperationException(\"Unable to locate match function on source union type. This indicates a bug in the marker detection algorithm.\");")
            .Detent()
            .Append("var targetFactoryMap = ").Typeof(_target.Signature).AppendLine(".GetMethods()")
            .Indent()
                .Append(".Where(c => c.CustomAttributes.Any(a => a.AttributeType.FullName == \"").Append(UnionTypeFactoryAttribute.MetadataName).AppendLine("\"))")
                .AppendLine(".ToDictionary(c => c.GetParameters()[0].ParameterType);")
            .Detent()
            .AppendLine("var handlers = matchMethod.GetParameters()")
            .Indent()
                .AppendLine(".Select(p => p.ParameterType.GenericTypeArguments[0])")
                .AppendLine(".Select(t => (ParameterExpr: System.Linq.Expressions.Expression.Parameter(t), ParameterExprType: t))")
                .Append(".Select(t =>")
                .OpenBracesBlock()
                    .AppendLine("var delegateType = typeof(System.Func<,>).MakeGenericType(t.ParameterExprType, tupleType);")
                    .AppendLine("System.Linq.Expressions.Expression expression = targetFactoryMap.TryGetValue(t.ParameterExprType, out var factory)")
                    .Indent()
                        .AppendLine("? System.Linq.Expressions.Expression.New(tupleType.GetConstructors()[0], System.Linq.Expressions.Expression.Constant(true), System.Linq.Expressions.Expression.Call(factory, t.ParameterExpr))")
                        .AppendLine(": System.Linq.Expressions.Expression.Default(tupleType);")
                    .Detent()
                    .AppendLine("return System.Linq.Expressions.Expression.Lambda(delegateType, expression, t.ParameterExpr);")
                .CloseBlock()
                .Append(");")
            .Detent()
            .AppendLine("var paramExpr = System.Linq.Expressions.Expression.Parameter(sourceType);")
            .AppendLine("var callExpr = System.Linq.Expressions.Expression.Call(paramExpr, matchMethod, handlers);")
            .AppendLine("var lambdaExpr = System.Linq.Expressions.Expression.Lambda(callExpr, paramExpr);")
            .AppendLine("var result = lambdaExpr.Compile();")
            .AppendLine("return result;")
        .CloseBlock()
        .Append(");");
    public void TryCreateDefaultCase(IndentedStringBuilder builder)
    {
        builder.Append("var sourceType = typeof(").Append(_target.Settings.GenericTValueName).AppendLine(");")
            .Append("if(!").Append(_target.ScopedDataTypeName).AppendLine(".Cache.TryGetValue(sourceType, out var weakMatch))")
            .OpenBracesBlock()
                .Append("if(!RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.IsMarked(sourceType))")
                .OpenBracesBlock()
                    .AppendLine("result = default;")
                    .Append("return false;")
                .CloseBlock()
                .Append(ConversionCacheWeakMatchExpr)
            .CloseBlock()
            .Append("var match = (System.Func<TValue, (System.Boolean, ").Append(_target.Signature.Names.FullGenericName).AppendLine(")>)weakMatch;")
            .AppendLine("var matchResult = match.Invoke(value);")
            .AppendLine("if(!matchResult.Item1)")
            .OpenBracesBlock()
                .AppendLine("result = default;")
                .Append("return false;")
            .CloseBlock()
            .AppendLine("result = matchResult.Item2;")
            .Append("return true;")
            .AppendLineCore();
    }
    public void Fields(IndentedStringBuilder builder)
    {
        builder.OpenRegionBlock("Fields")
            .Append(_storageHost.ReferenceTypeContainerField)
            .Append(_storageHost.DedicatedReferenceFields)
            .Append(_storageHost.DedicatedImpureAndUnknownFields)
            .Append(_storageHost.DedicatedPureValueTypeFields)
            .AppendCore(_storageHost.ValueTypeContainerField);

        if(_target.RepresentableTypes.Count > 1)
        {
            builder.Comment.OpenSummary()
                .Append("Used to determine the currently represented type and value.")
                .CloseBlock()
                .GeneratedUnnavigableInternalCode(_target)
                .Append("private readonly ")
                .Append(_target.Settings.TagTypeName)
                .Append(' ')
                .Append(_target.Settings.TagFieldName)
                .AppendCore(';');
        }

        builder.CloseBlockCore();
    }
    public void Constructors(IndentedStringBuilder builder)
    {
        var ctors = _target.RepresentableTypes.Select(t =>
            new IndentedStringBuilderAppendable(b =>
            {
                var accessibility = _target.GetSpecificAccessibility(t.Signature);
                b.Comment.OpenSummary()
                .Append("Creates a new instance of ").Comment.SeeRef(_target).Append("representing an instance of ").Comment.Ref(t.Signature).Append('.')
                .CloseBlockCore();

                if(!t.Factory.RequiresGeneration)
                {
                    b.Comment.OpenRemarks()
                    .Append("Using this constructor will sidestep any validation or sideeffects defined in the ").Comment.SeeRef(t.Factory.Name).Append(" factory method.")
                    .CloseBlockCore();
                }

                b.AppendLine(ConstantSources.EditorBrowsableNever)
                .AppendLine(ConstantSources.GeneratedCode)
                .Append(accessibility).Append(' ').Append(_target.Signature.Names.Name)
                .Append('(').Append(t.Signature.Names.FullGenericNullableName).Append(" value)")
                .OpenBlockCore(Blocks.Braces(b.Options.NewLine));

                if(_target.RepresentableTypes.Count > 1)
                {
                    b.Append(_target.Settings.TagFieldName).Append(" = ")
                    .Append(_target.Settings.TagTypeName).Append('.').Append(t.Alias).Append(';')
                    .AppendLineCore();
                }

                b.Append(StorageStrategy.Create(_target, t)
                    .InstanceVariableAssignmentExpression("value", "this")).Append(';')
                    .CloseBlockCore();
            }));

        builder.OpenRegionBlock("Constructors")
            .AppendJoinLines(StringOrChar.Empty, ctors)
            .CloseBlockCore();
    }
    public void TagType(IndentedStringBuilder builder)
    {
        var (_, representableTypes, _, settings, _, _, _) = _target;

        if(representableTypes.Count < 2)
            return;

        builder
            .OpenRegionBlock("Tag Type")
            .Comment.OpenSummary()
                .Append("Defines tags to discriminate between representable types.")
                .CloseBlock()
                .GeneratedUnnavigableInternalCode(_target)
                .Append("private enum ").Append(settings.TagTypeName).Append(" : System.Byte")
                .OpenBracesBlock()
                .AppendJoinLines(representableTypes, (b, t) =>
                    b.Comment.OpenSummary()
                    .Append("Used when representing an instance of ").Comment.Ref(t.Signature).Append('.')
                    .CloseBlock()
                    .Append(t.Alias))
                .CloseBlock()
            .CloseBlockCore();
    }
    public void JsonConverterType(IndentedStringBuilder builder)
    {
        var (_, representableTypes, _, settings, _, _, _) = _target;

        using var __ = builder.OpenRegionBlockScope("Json Converter Type");

        builder.Comment.OpenSummary()
                .Append("Implements json conversion logic for the ").Comment.Ref(_target.Signature).AppendLine(" type.")
                .CloseBlock()
                .AppendLine(ConstantSources.GeneratedCode)
                .Append("public sealed class JsonConverter : System.Text.Json.Serialization.JsonConverter<")
                .Append(_target.Signature.Names.GenericName).AppendLine('>')
                .OpenBracesBlock()
                .Append("sealed class Dto")
                .OpenBracesBlock()
                .Append("public static Dto Create(").Append(_target.Signature.Names.GenericName)
                .Append(" value) => new()")
                .OpenBracesBlock()
                .AppendCore("RepresentedType = ");

        if(_target.RepresentableTypes.Count == 1)
        {
            builder.Append('"').Append(_target.RepresentableTypes[0].Signature.Names.FullMetadataName).AppendCore('"');
        } else
        {
            _ = builder.TagSwitchExpr(
                _target,
                (b, t) => b.Append('"').Append(t.Signature.Names.FullMetadataName).AppendCore('"'),
                instanceExpr: "value");
        }

        builder.AppendLine(',').AppendCore("RepresentedValue = ");

        if(_target.RepresentableTypes.Count == 1)
        {
            builder.AppendCore(
                _storageStrategies[_target.RepresentableTypes[0]]
                .InstanceVariableExpression("value"));
        } else
        {
            _ = builder.TagSwitchExpr(
                _target,
                (b, t) => b.AppendCore(
                    _storageStrategies[t].InstanceVariableExpression(instance: "value")),
                instanceExpr: "value");
        }

        builder.CloseBlock()
            .AppendLine(';')
            .Append("public ").Append(_target.Signature.Names.GenericName).AppendLine(" Reconstitute() =>")
            .FullMetadataNameSwitchExpr(
                _target,
                "RepresentedType",
                (b, t) =>
                {
                    var isNullable = t.Options.HasFlag(UnionTypeOptions.Nullable) || t.Signature.Names.FullOpenGenericName == "System.Nullable<>";
                    if(isNullable)
                    {
                        b.Append(_target.Signature.Names.GenericName).Append('.').Append(t.Factory.Name).AppendLine('(')
                            .Indent()
                                .AppendLine("RepresentedValue != null")
                                .Append("? System.Text.Json.JsonSerializer.Deserialize<").Append(t.Signature.Names.FullGenericName).AppendLine(">((System.Text.Json.JsonElement)RepresentedValue)")
                                .Indent()
                                    .Append("?? throw new System.Text.Json.JsonException(\"Unable to deserialize an instance of the nullable type ")
                                    .Append(t.Signature.Names.FullGenericName)
                                    .Append(" from an unknown value in the RepresentedValue property.\")")
                                .Detent()
                                .Append(": null)")
                            .DetentCore();
                    } else
                    {
                        b.Append(_target.Signature.Names.GenericName).Append('.').Append(t.Factory.Name).AppendLine('(')
                            .Indent()
                                .Append("System.Text.Json.JsonSerializer.Deserialize<")
                                .Append(t.Signature.Names.FullGenericName)
                                .AppendLine(">(")
                                .Indent()
                                    .AppendLine("(System.Text.Json.JsonElement)")
                                    .AppendLine("(RepresentedValue")
                                    .Append("?? throw new System.Text.Json.JsonException(\"Unable to deserialize an instance of the non-nullable type ")
                                    .Append(t.Signature.Names.FullGenericName)
                                    .AppendLine(" from a null value in the RepresentedValue property.\")))")
                                .Detent()
                                .Append(b =>
                                {
                                    if(t.Signature.Nature is TypeNature.ReferenceType or TypeNature.UnknownType)
                                    {
                                        b.Append("?? throw new System.Text.Json.JsonException(\"Unable to deserialize an instance of the non-nullable type ")
                                        .Append(t.Signature.Names.FullGenericName)
                                        .AppendCore(" from an unknown value in the RepresentedValue property.\")");
                                    }
                                })
                                .Append(")")
                            .DetentCore();
                    }
                }, defaultCase: b => b
                    .Append("throw new System.Text.Json.JsonException($\"Unable to deserialize a union instance representing an instance of {RepresentedType} as an instance of ")
                    .Append(_target.Signature.Names.FullGenericName).AppendCore("\")"))
            .AppendLine(';')
            .AppendLine("public required System.String RepresentedType { get; set; }")
            .AppendLine("public required System.Object? RepresentedValue { get; set; }")
            .CloseBlock()
            .Comment.InheritDoc()
            .Append("public override ").Append(_target.Signature.Names.GenericName)
            .Append(_target.Signature.Nature == TypeNature.ReferenceType ? "?" : String.Empty)
            .AppendLine(" Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)")
            .OpenBracesBlock()
                .AppendLine("var dto = System.Text.Json.JsonSerializer.Deserialize<Dto?>(ref reader, options);")
                .AppendLine("if(dto == null)")
                .OpenBracesBlock()
                    .Append("throw new System.Text.Json.JsonException(\"Unable to deserialize union instance from invalid dto.\");")
                .CloseBlock()
                .AppendLine("var result = dto.Reconstitute();")
                .AppendLine("return result;")
            .CloseBlock()
            .Comment.InheritDoc()
            .Append("public override void Write(System.Text.Json.Utf8JsonWriter writer, ")
            .Append(_target.Signature.Names.GenericName)
            .AppendLine(" value, System.Text.Json.JsonSerializerOptions options) => System.Text.Json.JsonSerializer.Serialize(writer, Dto.Create(value), options);")
            .CloseBlockCore();
    }
    public void ValueTypeContainerType(IndentedStringBuilder builder)
    {
        using var _ = builder.OpenRegionBlockScope("Value Type Container");
        _storageHost.ValueTypeContainerType(builder);
    }
    public void NestedTypes(IndentedStringBuilder builder)
    {
        using var _ = builder.OpenRegionBlockScope("Nested Types");

        ValueTypeContainerType(builder);
        TagType(builder);
        JsonConverterType(builder);
    }
}
