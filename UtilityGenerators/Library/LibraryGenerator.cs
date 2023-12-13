namespace RhoMicro.CodeAnalysis.UtilityGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

partial class LibraryGenerator
{
    static partial void InitializeStatic(IncrementalGeneratorInitializationContext context)
    {
        const String metadataName = "global::RhoMicro.CodeAnalysis.UtilityGenerators.Library.ExpandingMacroStringBuilder.OperatorsDecorator";
        var generatedArgLists = new ConcurrentDictionary<String, Object?>();
        var provider = context.SyntaxProvider.CreateSyntaxProvider<(Boolean, String?, String?)>(
            (n, t) => n is BinaryExpressionSyntax b &&
                        b.IsKind(SyntaxKind.AddExpression) &&
                        b.Right is TupleExpressionSyntax,
            (c, t) =>
            {
                var semanticModel = c.SemanticModel;
                var expr = (c.Node as BinaryExpressionSyntax)!;
                var builderType = semanticModel.GetTypeInfo(expr.Left, cancellationToken: t).Type;
                var builderTypeMetadataName = builderType?.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat
                        .WithGenericsOptions(SymbolDisplayGenericsOptions.None));
                if(builderTypeMetadataName != metadataName)
                {
                    return (false, default, default);
                }

                var rhs = (expr.Right as TupleExpressionSyntax)!;
                var actionExpr = rhs.Arguments[0];
                var argTypes = rhs.Arguments.Skip(1)
                    .Select(a => semanticModel.GetTypeInfo(a.Expression).Type)
                    .Where(s => s != null)
                    .Select(s => s!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                    .ToList();

                var typeListId = String.Join("°#°", argTypes);
                if(!(argTypes.Count == rhs.Arguments.Count - 1 &&
                     generatedArgLists.TryAdd(typeListId, null)))
                {
                    return (false, default, default);
                }

                var typeList = String.Join(", ", argTypes);
                var argList = String.Join(", ", argTypes.Select((s, i) => $"invocation.Item{i + 2}"));
                var sourceText = $$"""
                namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

                using System;

                partial class ExpandingMacroStringBuilder
                {
                    partial class OperatorsDecorator<TMacro, TModel>
                    {
                        /// <summary>
                        /// Applies a method to a decorator.
                        /// </summary>
                        /// <param name="decorator">The decorator to apply a method to.</param>
                        /// <param name="invocation">The method, including its second argument, to apply to the decorator.</param>
                        /// <returns>A reference to the decorator.</returns>
                        //public static OperatorsDecorator<TMacro, TModel> operator +(
                        //    OperatorsDecorator<TMacro, TModel> decorator,
                        //    (Action<OperatorsDecorator<TMacro, TModel>, {{typeList}}>, {{typeList}}) invocation)
                        //{
                        //    invocation.Item1.Invoke(decorator, {{argList}});
                        //    return decorator.GetSelf();
                        //}
                        public void Foo(){}
                    }
                    partial class OperatorsDecorator<TMacro>
                    {
                        /// <summary>
                        /// Applies a method to a decorator.
                        /// </summary>
                        /// <param name="decorator">The decorator to apply a method to.</param>
                        /// <param name="invocation">The method, including its second argument, to apply to the decorator.</param>
                        /// <returns>A reference to the decorator.</returns>
                        public static OperatorsDecorator<TMacro> operator +(
                            OperatorsDecorator<TMacro> decorator,
                            (Action<OperatorsDecorator<TMacro>, {{typeList}}>, {{typeList}}) invocation)
                        {
                            invocation.Item1.Invoke(decorator, {{argList}});
                            return decorator.GetSelf();
                        }
                    }
                }

                """;
                var hintName = $"ExpandingMacroStringBuilder.OperatorsDecorator_{Math.Abs(typeList.GetHashCode())}{Guid.NewGuid()}.g.cs";

                return (true, hintName, sourceText);
            }).Where(v => v.Item1)
            .Select((v, t) => (HintName: v.Item2!, SourceText: v.Item3!));

        context.RegisterSourceOutput(provider, (c, t) => c.AddSource(t.HintName, t.SourceText));
    }
}
