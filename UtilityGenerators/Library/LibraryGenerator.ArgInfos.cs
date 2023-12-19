namespace RhoMicro.CodeAnalysis.UtilityGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections;

partial class LibraryGenerator
{
    sealed class ArgInfos
    {
        private ArgInfos(IReadOnlyList<ArgInfo> infos) => _infos = infos;

        private readonly IReadOnlyList<ArgInfo> _infos;

        static readonly ArgInfos _empty = new(Array.Empty<ArgInfo>());
        static readonly IReadOnlyList<ArgInfos> _emptyInfos = Array.Empty<ArgInfos>();

        public Boolean IsValid =>
            _infos.Count > 1 &&
            IsBuilderType(_infos[0].Type);

        public String GetArgumentTypes() =>
            String.Join(", ", _infos.Skip(1).Select(static i => i.Type));
        public String GetArgumentsPart() =>
            String.Join(", ", _infos.Select(static (ai, i) =>
                i == 0 ?
                _decoratorParamName :
                $"invocation.Item{i + 1}"));

        public static IReadOnlyList<ArgInfos> CreateAll(
        SemanticModel semanticModel,
        TupleExpressionSyntax tupleExpr,
        CancellationToken cancellationToken)
        {
            var result = tryGetIdentifierNameOrMemberAccessArgTypes(tupleExpr.Arguments[0].Expression, out var r) ||
                tryGetCastArgTypes(tupleExpr.Arguments[0].Expression, out r) ||
                tryGetLambdaArgTypes(tupleExpr.Arguments, out r) ?
                r :
                _emptyInfos;

            return result;

            Boolean tryGetLambdaArgTypes(
            SeparatedSyntaxList<ArgumentSyntax> tupleArgs,
                out IReadOnlyList<ArgInfos> argTypesLists)
            {
                argTypesLists = _emptyInfos;
                if(tupleArgs[0].Expression is not ParenthesizedLambdaExpressionSyntax lambda)
                {
                    return false;
                }

                //not implemented yet (TODO?)
                return false;
            }

            Boolean tryGetCastArgTypes(
                ExpressionSyntax expr,
                out IReadOnlyList<ArgInfos> argTypesLists)
            {
                argTypesLists = _emptyInfos;
                if(expr is not CastExpressionSyntax cast)
                {
                    return false;
                }

                var targetType = semanticModel.GetTypeInfo(cast.Type, cancellationToken).Type;
                if(targetType == null)
                {
                    return false;
                }

                var argTypes = getActionArgTypes(targetType);
                if(!argTypes.IsValid)
                {
                    return false;
                }

                argTypesLists = new[] { argTypes };
                return true;
            }

            Boolean tryGetIdentifierNameOrMemberAccessArgTypes(
                ExpressionSyntax expr,
                out IReadOnlyList<ArgInfos> argTypesLists)
            {
                argTypesLists = _emptyInfos;
                if(expr is not IdentifierNameSyntax
                   and not MemberAccessExpressionSyntax)
                {
                    return false;
                }

                var identifier = (expr is IdentifierNameSyntax name ?
                    name.Identifier :
                    ((MemberAccessExpressionSyntax)expr).Name.Identifier).Text;

                var result = semanticModel!.LookupSymbols(expr.Span.Start, name: identifier)
                    .Select(s =>
                        s is IMethodSymbol m ?
                        new(m.Parameters.Select(ArgInfo.Create).ToList()) :
                        s is ILocalSymbol l ?
                        getActionArgTypes(l.Type) :
                        _empty)
                    .Where(i => i.IsValid)
                    .ToList();

                argTypesLists = result;
                return true;
            }

            ArgInfos getActionArgTypes(ITypeSymbol symbol)
            {
                var infos = symbol.GetMembers("Invoke")
                    .OfType<IMethodSymbol>()
                    .SingleOrDefault()
                    ?.Parameters
                    .Select(ArgInfo.Create)
                    .ToList();
                var result = infos == null ?
                    _empty :
                    new ArgInfos(infos);

                return result;
            }
        }
    }
}
