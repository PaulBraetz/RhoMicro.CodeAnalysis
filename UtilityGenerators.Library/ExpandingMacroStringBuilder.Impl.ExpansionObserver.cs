namespace RhoMicro.CodeAnalysis.Library;

using System;

partial class ExpandingMacroStringBuilder
{
    partial class Impl<TMacro>
    {
        sealed class ExpansionObserver
        {
            private ExpansionObserver(HashSet<TMacro> recursionDetectionSet) => _recursionDetectionSet = recursionDetectionSet;

            public Boolean Expanded { get; private set; }
            private readonly HashSet<TMacro> _recursionDetectionSet;

            public static ExpansionObserver Create() => new([]);
            public ExpansionObserver Derive() => new(_recursionDetectionSet);
            public ExpansionObserver Clone() => new(new(_recursionDetectionSet))
            {
                Expanded = Expanded
            };

            public void NotifyExpansionStart(TMacro macro)
            {
                if(!_recursionDetectionSet.Add(macro))
                    throw new InfinitelyRecursingExpansionException<TMacro>(macro);
            }
            public void NotifyExpansionEnd(TMacro macro)
            {
                if(!_recursionDetectionSet.Remove(macro))
                    throw new ArgumentException($"Observer was not notified of {macro} expansion start.", nameof(macro));
                Expanded = true;
            }

            public override String ToString() => $"{(Expanded ? "Expanded" : "Not Expanded")} [{String.Join(",", _recursionDetectionSet)}]";
        }
    }
}