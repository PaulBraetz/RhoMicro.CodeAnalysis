namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;

readonly struct LayoutModel
{
    public readonly String SourceText;
    private LayoutModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<LayoutModel> context) =>
        context.Source.SetLayout(context.Model);

    static LayoutModel Create(ModelCreationContext context)
    {
        var sourceText = context.TargetData.Annotations.Settings.Layout == LayoutSetting.Small &&
            !context.TargetData.Symbol.IsGenericType ?
            "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]" :
            String.Empty;
        var result = new LayoutModel(sourceText);

        return result;
    }
}
