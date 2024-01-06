namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;
using System.Text;

partial class ExpandingMacroStringBuilder
{
    /// <summary>
    /// Represents a queue of values and macros. _macros may be replaced at a later time.
    /// </summary>
    /// <typeparam name="TValue">The type of value to enqueue.</typeparam>
    /// <typeparam name="TMacro">The type of macro to enqueue.</typeparam>
    internal sealed partial class Queue<TValue, TMacro>
    {
        private Queue(Dictionary<TMacro, Stack<Queue<TValue, TMacro>.Node<TMacro>>> macros) => _macros = macros;
        public Queue() : this([]) { }

        private readonly Dictionary<TMacro, Stack<Node<TMacro>>> _macros;
        private Node? _first;
        private Node? _last;

        /// <summary>
        /// Gets a value indicating whether the queue contains any macros.
        /// </summary>
        public Boolean ContainsMacros => _macros.Any(static kvp => kvp.Value.Count > 0);
        public IReadOnlyList<TMacro> GetMacros() => _macros.SelectMany(static kvp => kvp.Value).Select(static n => n.Value).ToList();

        /// <summary>
        /// Gets an enumeration of distinct macros that have yet to be expanded.
        /// </summary>
        /// <returns>The distinct macros that have yet to be expanded.</returns>
        public IEnumerable<TMacro> UnexpandedMacros() => _macros.Where(s => s.Value.Count > 0).Select(kvp => kvp.Key);
        /// <summary>
        /// Adds a value to the end of the queue.
        /// </summary>
        /// <param name="value">The value to enqueue.</param>
        public Queue<TValue, TMacro> EnqueueValue(TValue value)
        {
            var node = Node<TValue>.Rent(value);
            Enqueue(node);
            return this;
        }
        /// <summary>
        /// Adds a macro to the end of the queue.
        /// </summary>
        /// <param name="macro">The macro to enqueue.</param>
        public Queue<TValue, TMacro> EnqueueMacro(TMacro macro)
        {
            var node = Node<TMacro>.Rent(macro);
            GetMacroNodes(macro).Push(node);
            Enqueue(node);
            return this;
        }
        /// <summary>
        /// Dequeues all sequential values from the end of the queue.
        /// </summary>
        /// <returns>The sequential values at the end of the queue.</returns>
        public IEnumerable<TValue> DequeueValues()
        {
            while(_first is Node<TValue> valueNode)
            {
                if(_last == _first)
                {
                    _last = valueNode.Next;
                }

                _first = valueNode.Next;
                var value = valueNode.Value;
                RemoveNode(valueNode);

                yield return value;
            }
        }
        /// <summary>
        /// Replaces all occurences of a macro with the replacement queue provided.
        /// </summary>
        /// <param name="macro">The macro to replace.</param>
        /// <param name="configureExpansionQueue">The action using which to configure the queue of items to replace occurences of the macro with.</param>
        public Queue<TValue, TMacro> Expand(TMacro macro, Action<Queue<TValue, TMacro>> configureExpansionQueue)
        {
            var nodes = GetMacroNodes(macro);
            if(nodes.Count == 0)
            {
                return this;
            }

            var replacementQueue = new Queue<TValue, TMacro>();
            configureExpansionQueue.Invoke(replacementQueue);

            if(replacementQueue._first == null)
            {
                RemoveMacros(nodes);
            } else
            {
                ReplaceMacros(replacementQueue, nodes);
            }

            return this;
        }

        private void Enqueue(Node? node)
        {
            if(_first == null)
            {
                _first = node;
            } else if(_last != null)
            {
                _last.Next = node;
                if(node != null)
                {
                    node.Previous = _last;
                }
            }

            _last = node;
        }
        private Stack<Node<TMacro>> GetMacroNodes(TMacro macro)
        {
            if(!_macros.TryGetValue(macro, out var nodes))
            {
                nodes = [];
                _macros.Add(macro, nodes);
            }

            return nodes;
        }
        private void ReplaceMacros(Queue<TValue, TMacro> replacementQueue, Stack<Node<TMacro>> nodes)
        {
            while(nodes.Count > 0)
            {
                var node = nodes.Pop();
                node.Replace(replacementQueue._first, replacementQueue._last);

                if(node == _first)
                {
                    _first = replacementQueue._first;
                }

                if(node == _last)
                {
                    _last = replacementQueue._last;
                }

                foreach(var kvp in replacementQueue._macros)
                {
                    while(kvp.Value.Count > 0)
                    {
                        GetMacroNodes(kvp.Key).Push(kvp.Value.Pop());
                    }
                }

                Node<TMacro>.Return(node);

                if(nodes.Count > 0)
                {
                    replacementQueue = replacementQueue.Clone();
                }
            }
        }
        private void RemoveMacros(Stack<Node<TMacro>> nodes)
        {
            while(nodes.Count > 0)
                RemoveNode(nodes.Pop());
        }
        private void RemoveNode<T>(Node<T> node)
        {
            RemoveFirstOrLast(node);
            node.RemoveSelf();
            Node<T>.Return(node);
        }
        private void RemoveFirstOrLast(Node node)
        {
            if(node == _first)
            {
                _first = null;
                _first = node.Next;
            }

            if(node == _last)
            {
                _last = null;
            }
        }
        private IEnumerable<Node> GetNodes()
        {
            var node = _first;
            while(node != null)
            {
                yield return node;

                node = node.Next;
            }
        }
        /// <inheritdoc/>
        public override String ToString() => GetNodes().Aggregate(new StringBuilder(), static (b, s) => b.Append(s)).ToString();
        internal Queue<TValue, TMacro> Clone()
        {
            var result = new Queue<TValue, TMacro>(new(_macros));
            var firstClone = _first?.Clone();
            result.Enqueue(firstClone);

            return result;
        }
    }
}
