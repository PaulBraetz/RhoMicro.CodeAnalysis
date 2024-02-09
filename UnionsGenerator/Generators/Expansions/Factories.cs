namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

internal sealed class Factories(TargetDataModel model) : ExpansionBase(model, Macro.Factories)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        //only commented out because of breaking rewrite changes
        //var representableTypes = Model.Annotations.AllRepresentableTypes;
        //var target = Model.Symbol;
        //var settings = Model.Annotations.Settings;

        //_ = (builder % "#region Factories")
        //    .AppendJoin(
        //        representableTypes.Where(t => t.Factory.RequiresGeneration),
        //        (b, a, t) => b.WithOperators(t) *
        //            (Docs.Summary, b => _ = b * "Creates a new instance of " * target.CommentRef * ".") /
        //            "[RhoMicro.CodeAnalysis.UnionFactory]" /
        //            "public static " * target.ToMinimalOpenString() * ' ' * a.Factory.Name * '(' * a.Names.FullTypeName % " value) => new(value);",
        //        builder.CancellationToken)
        //    .WithOperators(builder.CancellationToken) *
        //    "public static Boolean TryCreate<" * settings.GenericTValueName * ">(" *
        //    settings.GenericTValueName * " value, out " * target.ToMinimalOpenString() %
        //    " instance){instance = default;" * (b => TypeSwitchStatement(
        //        builder: b,
        //        values: representableTypes,
        //        valueTypeExpression: b => _ = b * (b => UtilFullString(b, (b) => _ = b * "typeof(" * settings.GenericTValueName * ')')),
        //        caseSelector: t => t.Names,
        //        (b, v) => _ = b * "instance = " * v.Factory.Name * '(' * (b => UtilUnsafeConvert(b, settings.GenericTValueName, v.Names.FullTypeName, "value")) * ");return true;",
        //        TryCreateDefaultCase)) * '}' /
        //    "public static " * target.ToMinimalOpenString() * " Create<" * settings.GenericTValueName * ">(" *
        //    settings.GenericTValueName * " value){" *
        //    (b => TypeSwitchStatement(
        //        b,
        //        representableTypes,
        //        b => _ = b * (b => UtilFullString(b, b => _ = b * "typeof(" * settings.GenericTValueName * ')')),
        //        t => t.Names,
        //        (b, v) => _ = b * "return " * v.Factory.Name * "(" * (b => UtilUnsafeConvert(b, settings.GenericTValueName, v.Names.FullTypeName, "value")) * ");",
        //        CreateDefaultCase)) %
        //    '}' %
        //    "#endregion";
    }

    private void TryCreateDefaultCase(ExpandingMacroBuilder builder)
    {
        _ = builder * "var sourceType = typeof(TValue);" /
            "if(!" * Model.ConversionFunctionsTypeName * ".Cache.TryGetValue(sourceType, out var weakMatch))" /
        """    
            {
                if(!RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.IsMarked(sourceType))
                {
                    return false;
                }
        """ * "weakMatch = " * Model.ConversionFunctionsTypeName * ".Cache.GetOrAdd(sourceType, t =>{" *
        "var tupleType = typeof(System.ValueTuple<System.Boolean, " * Model.Symbol.ToMinimalOpenString() * ">);" *
        """
                    var matchMethod = sourceType.GetMethod(
                        nameof(Match),
                         System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)?
                         .MakeGenericMethod(tupleType) ??
                         throw new System.Exception("Unable to locate match function on source union type. This indicates a bug in the marker detection algorithm.");
        """ * "var targetFactoryMap = typeof(" * Model.Symbol.ToMinimalOpenString() * ").GetMethods()" *
        """
                        .Where(c => c.CustomAttributes.Any(a => a.AttributeType == typeof(RhoMicro.CodeAnalysis.UnionFactoryAttribute)))
                        .ToDictionary(c => c.GetParameters()[0].ParameterType);

                    var handlers = matchMethod.GetParameters()
                        .Select(p => p.ParameterType.GenericTypeArguments[0])
                        .Select(t => (ParameterExpr: System.Linq.Expressions.Expression.Parameter(t), ParameterExprType: t))
                        .Select(t =>
                        {
                            var delegateType = typeof(System.Func<,>).MakeGenericType(t.ParameterExprType, tupleType);
                            System.Linq.Expressions.Expression expression = targetFactoryMap.TryGetValue(t.ParameterExprType, out var factory) ?
                                System.Linq.Expressions.Expression.New(
                                    tupleType.GetConstructors()[0],
                                    System.Linq.Expressions.Expression.Constant(true),
                                    System.Linq.Expressions.Expression.Call(factory, t.ParameterExpr)) :
                                System.Linq.Expressions.Expression.Default(tupleType);

                            return System.Linq.Expressions.Expression.Lambda(delegateType, expression, t.ParameterExpr);
                        });

                    var paramExpr = System.Linq.Expressions.Expression.Parameter(sourceType);
                    var callExpr = System.Linq.Expressions.Expression.Call(paramExpr, matchMethod, handlers);
                    var lambdaExpr = System.Linq.Expressions.Expression.Lambda(callExpr, paramExpr);
                    var result = lambdaExpr.Compile();

                    return result;
                });
            }
        """ * "var match = (System.Func<TValue, (System.Boolean, " * Model.Symbol.ToMinimalOpenString() * ")>)weakMatch;" *
        """    
            var matchResult = match.Invoke(value);
            if(!matchResult.Item1)
            {
                return false;
            }

            instance = matchResult.Item2;
            return true;
        """;
    }
    private void CreateDefaultCase(ExpandingMacroBuilder _)
    {
        //only commented out because of breaking rewrite changes
        //_ = builder * "var sourceType = typeof(TValue);" /
        //    "if(!" * Model.ConversionFunctionsTypeName * ".Cache.TryGetValue(sourceType, out var weakMatch)){" /
        //    "if(!RhoMicro.CodeAnalysis.UnionsGenerator.Generated.Util.IsMarked(sourceType)){" /
        //    (b => InvalidCreationThrow(b, $"typeof({Model.Symbol.ToMinimalOpenString()})", "value")) * ';' /
        //    "}weakMatch = " * Model.ConversionFunctionsTypeName * ".Cache.GetOrAdd(sourceType, t =>" /
        //    "{var tupleType = typeof(System.ValueTuple<System.Boolean, " * Model.Symbol.ToMinimalOpenString() * ">);" /
        //    """
        //            var matchMethod = sourceType.GetMethod(
        //                    nameof(Match), 
        //                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)?
        //                .MakeGenericMethod(tupleType) ?? 
        //                throw new System.Exception("Unable to locate match function on source union type. This indicates a bug in the marker detection algorithm.");
        //    """ * "var targetFactoryMap = typeof(" * Model.Symbol.ToMinimalOpenString() * ')' *
        //    """
        //            .GetMethods()
        //            .Where(c => c.CustomAttributes.Any(a => a.AttributeType == typeof(RhoMicro.CodeAnalysis.UnionFactoryAttribute)))
        //            .ToDictionary(c => c.GetParameters()[0].ParameterType);

        //            var handlers = matchMethod.GetParameters()
        //                .Select(p => p.ParameterType.GenericTypeArguments[0])
        //                .Select(t => (ParameterExpr: System.Linq.Expressions.Expression.Parameter(t), ParameterExprType: t))
        //                .Select(t =>
        //                {
        //                    var delegateType = typeof(System.Func<,>).MakeGenericType(t.ParameterExprType, tupleType);
        //                    System.Linq.Expressions.Expression expression = targetFactoryMap.TryGetValue(t.ParameterExprType, out var factory) ? System.Linq.Expressions.Expression.New(tupleType.GetConstructors()[0], System.Linq.Expressions.Expression.Constant(true), System.Linq.Expressions.Expression.Call(factory, t.ParameterExpr)) : System.Linq.Expressions.Expression.Default(tupleType);
        //                    return System.Linq.Expressions.Expression.Lambda(delegateType, expression, t.ParameterExpr);
        //                });
        //            var paramExpr = System.Linq.Expressions.Expression.Parameter(sourceType);
        //            var callExpr = System.Linq.Expressions.Expression.Call(paramExpr, matchMethod, handlers);
        //            var lambdaExpr = System.Linq.Expressions.Expression.Lambda(callExpr, paramExpr);
        //            var result = lambdaExpr.Compile();
        //            return result;
        //        });
        //    }
        //    """ *
        //    "var match = (System.Func<TValue, (System.Boolean, " * Model.Symbol.ToMinimalOpenString() * ")>)weakMatch;" *
        //    """
        //    var matchResult = match.Invoke(value);
        //    if(!matchResult.Item1)
        //    {
        //    """ *
        //    (b => InvalidCreationThrow(b, $"typeof({Model.Symbol.ToMinimalOpenString()})", "value")) * ';' *
        //    """
        //    }
        //    return matchResult.Item2;
        //    """;
    }
}

