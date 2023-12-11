namespace RhoMicro.CodeAnalysis.CopyToGenerator.ExpansionProviders;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.CopyToGenerator;
using RhoMicro.CodeAnalysis.CopyToGenerator.MacroExpansions;

sealed class CopyToMethodExpansion(Model model) : MacroExpansionBase(model, Macro.CopyTo)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        _ = builder.Append("""
                /// <summary>
                /// Copies this instances public properties to another ones.
                /// </summary>
                /// <param name="target">The instance to copy this instances properties' values to.</param>
                public void CopyTo(
                """)
            .Append(Model.Name)
            .Append(
            """
                 target)
                {
                    if(this == target || target == null)
                    { 
                        return;
                    }
                                
                    if(AvoidCopy(this, target))
                    {
                        return;
                    }

                """)
            .AppendJoin(
                '\n',
                Model.Properties,
                GetCopyStatement,
                cancellationToken)
            .AppendLine('}');
    }

    private static IExpandingMacroStringBuilder<Macro> GetCopyStatement(IExpandingMacroStringBuilder<Macro> resultBuilder, PropertyModel p, CancellationToken cancellationToken)
    {
        if(p.IsListAssignable)
#pragma warning disable IDE0045 // Convert to conditional expression
        {
            _ = resultBuilder
                .Append("if(target.").Append(p.Name).Append(" == default || this.").Append(p.Name).Append(" == default){")
                .Append("target.").Append(p.Name).Append(" = this.").Append(p.Name).Append(";}else{")
                //tradeoff: duplicates are lost
                .Append("var oldTargetElements = new global::System.Collections.Generic.HashSet<")
                .Append(p.ItemType!)
                .Append(">(target.").Append(p.Name).Append(");")
                .Append("var newTargetElements = new global::System.Collections.Generic.List<")
                .Append(p.ItemType!)
                .Append(">();foreach(var sourceElement in ").Append(p.Name)
                .Append(
"""
)
{
    if(oldTargetElements.TryGetValue(sourceElement, out var targetElement))
    {
        //both target & source contain element -> copy
        sourceElement.CopyTo(targetElement);
        newTargetElements.Add(targetElement);
        _ = oldTargetElements.Remove(sourceElement);
    } else
    {
        //only source contains element -> add to target
        newTargetElements.Add(sourceElement);
    }
}

//add remaining target elements to result collection
foreach(var remainingElement in oldTargetElements)
{
    newTargetElements.Add(remainingElement);
}
target.
""")
                .Append(p.Name).Append(" = newTargetElements;}");

        } else if(p.IsMarked)
        {
            _ = resultBuilder.Append("if(target.").Append(p.Name)
                .Append(" == default || this.").Append(p.Name).Append(" == default){")
                .Append("target.").Append(p.Name).Append(" = this.").Append(p.Name).Append(";}")
                .Append("else{")
                .Append("this.").Append(p.Name).Append(".CopyTo(target.").Append(p.Name).Append(");}");
        } else
        {
            _ = resultBuilder.Append("target.")
                .Append(p.Name)
                .Append(" = this.")
                .Append(p.Name)
                .Append(';');
        }
#pragma warning restore IDE0045 // Convert to conditional expression

        return resultBuilder;
    }
}
