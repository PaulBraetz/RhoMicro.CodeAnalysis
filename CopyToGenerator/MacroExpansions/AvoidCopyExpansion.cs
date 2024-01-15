namespace RhoMicro.CodeAnalysis.CopyToGenerator.ExpansionProviders;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.CopyToGenerator;
using RhoMicro.CodeAnalysis.CopyToGenerator.MacroExpansions;

using System.Text;

sealed class AvoidCopyExpansion(Model model) : MacroExpansionBase(model, Macro.AvoidCopy)
{
    public override void Expand(
        IExpandingMacroStringBuilder<Macro> sourceTextBuilder,
        CancellationToken cancellationToken)
    {
        _ = sourceTextBuilder
            .AppendLine("/// <summary>")
            .Append("/// Evaluates whether copying between two instances of <see cref=\"").Append(Model.Name).AppendLine("\"/> should be avoided due to equivalence. This can help avoid unnecessary copying or infinite copying in nested recursive relationships.")
            .AppendLine("/// </summary>")
            .AppendLine("/// <param name =\"a\">The first instance to compare.</param>")
            .AppendLine("/// <param name =\"b\">The second instance to compare.</param>")
            .AppendLine("/// <param name =\"result\">Upon returning, contains <see langword=\"true\"/> if copying between <paramref name=\"a\"/> and <paramref name=\"b\"/> should be avoided; otherwise, <see langword=\"false\"/>.</param>")
            .Append("static partial void AvoidCopy(")
            .Append(Model.Name).Append(" a, ").Append(Model.Name).Append(" b, ref global::System.Boolean result);")
            .AppendLine("/// <summary>")
            .Append("/// Evaluates whether copying between two instances of <see cref=\"").Append(Model.Name).AppendLine("\"/> should be avoided due to equivalence. This can help avoid unnecessary copying or infinite copying in nested recursive relationships.")
            .AppendLine("/// </summary>")
            .AppendLine("/// <param name =\"a\">The first instance to compare.</param>")
            .AppendLine("/// <param name =\"b\">The second instance to compare.</param>")
            .AppendLine("/// <returns><see langword=\"true\"/> if copying between <paramref name=\"a\"/> and <paramref name=\"b\"/> should be avoided; otherwise, <see langword=\"false\"/>.</returns>")
            .Append("static global::System.Boolean AvoidCopy(")
            .Append(Model.Name).Append(" a, ").Append(Model.Name).Append(" b){{var result = false; AvoidCopy(a,b,ref result);return result;}}");
    }
}
