namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

partial class ExpandingMacroStringBuilder
{
    sealed partial class Queue<TValue, TMacro>
    {
        sealed class Node<T> : Node
        {
            private Node(T value) => _value = value;

            public T Value
            {
                get
                {
                    ValidateRented();
                    return _value!;
                }
                private set
                {
                    ValidateRented();
                    _value = value;
                }
            }

            private static readonly ConcurrentBag<Node<T>> _pool = [];
            private T? _value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Node<T> Rent(T value)
            {
                if(_pool.TryTake(out var result))
                {
                    result.SetRented();
                    result.Value = value;
                } else
                {
                    result = new Node<T>(value);
                    result.SetRented();
                }

                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Return(Node<T> node)
            {
                node._value = default;
                node.Next = default;
                node.Previous = default;
                node.SetReturned();
                _pool.Add(node);
            }

            public override String ToString() => base.ToString().Replace("[ ]", $"[{(IsRented ? $"\"{Value}\"" : "_")}]");
            protected override Node CreateClone()
            {
                var result = Rent(Value);
                result.Previous = Previous?.Clone();
                result.Next = Next?.Clone();

                return result;
            }
        }
        abstract class Node
        {
            private Node? _previous;
            private Node? _next;

            public Node? Next
            {
                get
                {
                    ValidateRented();
                    return _next;
                }

                set
                {
                    ValidateRented();
                    _next = value;
                }
            }
            public Node? Previous
            {
                get
                {
                    ValidateRented();
                    return _previous;
                }

                set
                {
                    ValidateRented();
                    _previous = value;
                }
            }

            private Boolean _cloning;

            private const Int32 _rentedState = 1;
            private const Int32 _returnedState = 0;
            private Int32 _isRented = _returnedState;

            protected Boolean IsRented => _isRented == _rentedState;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected void ValidateRented()
            {
                if(_isRented == _returnedState)
                {
                    throw new InvalidOperationException("Unable to access returned node.");
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected void SetRented()
            {
                if(Interlocked.Exchange(ref _isRented, _rentedState) == _rentedState)
                {
                    throw new InvalidOperationException("Unable to rent node multiple times.");
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected void SetReturned()
            {
                if(Interlocked.Exchange(ref _isRented, _returnedState) == _returnedState)
                {
                    throw new InvalidOperationException("Unable to return node multiple times.");
                }
            }

            public void RemoveSelf()
            {
                if(Previous != null)
                    Previous.Next = Next;

                if(Next != null)
                    Next.Previous = Previous;
            }
            public void Replace(Node? first, Node? last)
            {
                if(Previous != null)
                    Previous.Next = first;
                if(first != null)
                    first.Previous = Previous;

                if(Next != null)
                    Next.Previous = last;
                if(last != null)
                    last.Next = Next;

                Previous = null;
                Next = null;
            }

            public override String ToString() => $"{(_previous != null ? "<" : String.Empty)}[ ]{(_next != null ? ">" : String.Empty)}";
            protected abstract Node CreateClone();
            internal Node Clone()
            {
                if(_cloning)
                {
                    return this;
                }

                _cloning = true;
                var result = CreateClone();
                _cloning = false;

                return result;
            }
        }
    }
}
