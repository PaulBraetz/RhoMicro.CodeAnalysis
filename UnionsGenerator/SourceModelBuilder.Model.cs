namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;

sealed partial class SourceModelBuilder
{
    public class Model
    {
        protected Model() { }
        public static readonly Model Instance = new();
        public virtual void AddToContext(SourceProductionContext context) { }
    }
}