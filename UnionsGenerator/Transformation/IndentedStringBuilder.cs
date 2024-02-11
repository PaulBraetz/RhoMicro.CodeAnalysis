namespace RhoMicro.CodeAnalysis.Library.Text;
using System;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

partial class IndentedStringBuilder
{
    public IndentedStringBuilder Typeof(TypeSignatureModel type, Boolean open = false) =>
        Append("typeof(").Append(type.IsTypeParameter ? type.Names.Name : open ? type.Names.FullOpenGenericName : type.Names.FullGenericName).Append(')');
    public IndentedStringBuilder UtilIsMarked(String typeExpression) =>
        Append("RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.IsMarked(").Append(typeExpression).Append(")");
    public IndentedStringBuilder UtilGetFullString(String typeExpression) =>
        Append("RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.GetFullString(").Append(typeExpression).Append(")");
    public IndentedStringBuilder UtilGetFullString(Action<IndentedStringBuilder> typeExpression) =>
        Append("RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.GetFullString(").Append(typeExpression).Append(")");
    public IndentedStringBuilder UtilGetFullString(TypeSignatureModel type) =>
        Append("RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.GetFullString(").Typeof(type).Append(")");
    public IndentedStringBuilder UtilUnsafeConvert(
        String tFrom,
        String tTo,
        String valueExpression) =>
        Append("(RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.UnsafeConvert<")
        .Append(tFrom).Append(", ").Append(tTo).Append(">(").Append(valueExpression).Append("))");
    public IndentedStringBuilder UtilUnsafeConvert(
        String tFrom,
        TypeSignatureModel tTo,
        String valueExpression) =>
        UtilUnsafeConvert(tFrom, tTo.IsTypeParameter ? tTo.Names.Name : tTo.Names.FullGenericName, valueExpression);
    public IndentedStringBuilder TagSwitchExpr(
        UnionTypeModel target,
        Action<IndentedStringBuilder, RepresentableTypeModel> caseExpr,
        String instanceExpr = "this",
        Action<IndentedStringBuilder>? specialDefault = null)
    {
        var representableTypes = target.RepresentableTypes;

        if(representableTypes.Count == 1)
        {
            caseExpr.Invoke(this, representableTypes[0]);

            return this;
        }

        Append(instanceExpr).Append('.').Append(target.Settings.TagFieldName).Append(" switch")
            .OpenBracesBlock()
            .AppendJoinLines(representableTypes.Append(null), (b, t) =>
            {
                if(t == null)
                {
                    b.AppendCore("_ => ");
                    if(specialDefault != null)
                    {
                        specialDefault.Invoke(b);
                    } else
                    {
                        b.AppendCore(ConstantSources.InvalidTagStateThrow);
                    }
                } else
                {
                    b.Append(target.Settings.TagTypeName).Append('.').Append(t.Alias).AppendCore(" => ");
                    caseExpr.Invoke(b, t);
                }
            })
            .CloseBlockCore();

        return this;
    }
    public IndentedStringBuilder TagSwitchStmt(
        UnionTypeModel target,
        Action<IndentedStringBuilder, RepresentableTypeModel> caseExpr,
        String instanceExpr = "this",
        Action<IndentedStringBuilder>? specialDefault = null)
    {
        if(target.RepresentableTypes.Count == 1)
        {
            caseExpr.Invoke(this, target.RepresentableTypes[0]);

            return this;
        }

        Append("switch(").Append(instanceExpr).Append('.').Append(target.Settings.TagFieldName).Append(')')
            .OpenBracesBlock()
            .AppendJoinLines(
                StringOrChar.Empty,
                target.RepresentableTypes.Append(null),
                (b, t) =>
                {
                    if(t == null)
                    {
                        _ = b.Append("default:").OpenBracesBlock();

                        if(specialDefault != null)
                        {
                            specialDefault.Invoke(b);
                        } else
                        {
                            b.Append(ConstantSources.InvalidTagStateThrow).AppendCore(';');
                        }

                        CloseBlockCore();
                    } else
                    {
                        _ = b.Append("case ").Append(target.Settings.TagTypeName).Append('.').Append(t.Alias).Append(":")
                        .OpenBracesBlock();
                        caseExpr.Invoke(b, t);
                        CloseBlockCore();
                    }
                })
            .CloseBlockCore();

        return this;
    }
    public IndentedStringBuilder FullMetadataNameSwitchExpr(
        UnionTypeModel target,
        String metadataNameExpr,
        Action<IndentedStringBuilder, RepresentableTypeModel> caseExpr,
        Action<IndentedStringBuilder> defaultCase)
        => FullMetadataNameSwitchExpr(
            target,
            b => b.Append(metadataNameExpr),
            caseExpr,
            defaultCase);
    public IndentedStringBuilder FullMetadataNameSwitchExpr(
        UnionTypeModel target,
        Action<IndentedStringBuilder> metadataNameExpr,
        Action<IndentedStringBuilder, RepresentableTypeModel> caseExpr,
        Action<IndentedStringBuilder> defaultCase)
    {
        if(target.RepresentableTypes.Count == 1)
        {
            caseExpr.Invoke(this, target.RepresentableTypes[0]);

            return this;
        }

        Append(metadataNameExpr).Append(" switch")
            .OpenBracesBlock()
            .AppendJoinLines(target.RepresentableTypes.Append(null), (b, t) =>
            {
                if(t == null)
                {
                    b.AppendCore("_ => ");
                    defaultCase.Invoke(b);
                } else
                {
                    b.Append('"').Append(t.Signature.Names.FullMetadataName).AppendCore("\" => ");
                    caseExpr.Invoke(b, t);
                }
            })
            .CloseBlockCore();

        return this;
    }
    public IndentedStringBuilder InvalidConversionThrow(String typeNameExpression) =>
        Append("throw new System.InvalidOperationException($\"Unable to convert the union instance to an instance of {")
        .Append(typeNameExpression).Append("}.\")");
    public IndentedStringBuilder MetadataNameSwitchStmt(
        UnionTypeModel target,
        Action<IndentedStringBuilder> typeExpr,
        Action<IndentedStringBuilder, RepresentableTypeModel> caseBody,
        Action<IndentedStringBuilder> defaultBody)
    {
        var nonTypeParamCases = new List<Action<IndentedStringBuilder>>();
        var typeParamCases = new List<Action<IndentedStringBuilder>>();

        foreach(var t in target.RepresentableTypes)
        {
            ( t.Signature.IsTypeParameter ? typeParamCases : nonTypeParamCases )
                .Add(t.Signature.IsTypeParameter ?
                b =>
                {
                    _ = b.Append("if(metadataName == ").UtilGetFullString(t.Signature).Append(")")
                        .OpenBracesBlock();
                    caseBody.Invoke(b, t);
                    b.CloseBlockCore();
                }
            :
                b =>
                {
                    _ = b.Append("case ")
                        .Append('"').Append(t.Signature.Names.FullGenericName).Append("\":")
                        .OpenBracesBlock();
                    caseBody.Invoke(b, t);
                    b.CloseBlockCore();
                });
        }

        typeParamCases.Add(b => b.OpenBracesBlock().Append(defaultBody).CloseBlockCore());

        Append("var metadataName = ").UtilGetFullString(typeExpr).AppendLine(';')
            .Append("switch(metadataName)")
            .OpenBracesBlock()
            .AppendJoinLines(StringOrChar.Empty, nonTypeParamCases)
            .Append("default:")
            .OpenBracesBlock()
            .AppendJoin(" else ", typeParamCases)
            .CloseBlock()
            .CloseBlockCore();

        return this;
    }
    public IndentedStringBuilder GeneratedUnnavigableInternalCode(UnionTypeModel model) =>
        Comment.InternalUse(model)
        .AppendLine(ConstantSources.GeneratedCode)
        .AppendLine(ConstantSources.EditorBrowsableNever);
    public IndentedStringBuilder AppendJoinLines<T>(
        IEnumerable<T> values,
        Action<IndentedStringBuilder, T> append)
        => AppendJoinLines(values.Select(v => new IndentedStringBuilderAppendable(b => append.Invoke(b, v))));
    public IndentedStringBuilder AppendJoinLines<T>(
        StringOrChar separator,
        IEnumerable<T> values,
        Action<IndentedStringBuilder, T> append)
        => AppendJoinLines(separator, values.Select(v => new IndentedStringBuilderAppendable(b => append.Invoke(b, v))));
    public IndentedStringBuilder AppendJoin<T>(
        StringOrChar separator,
        IEnumerable<T> values,
        Action<IndentedStringBuilder, T> append)
        => AppendJoin(separator, values.Select(v => new IndentedStringBuilderAppendable(b => append.Invoke(b, v))));

}
