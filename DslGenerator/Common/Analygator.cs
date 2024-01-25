namespace RhoMicro.CodeAnalysis.DslGenerator.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract class Analygator<TConsumable, TToken, TDiscriminator>
{
    protected Analygator(Range<TConsumable> consumables) => Consumables = consumables;

    private readonly Object _syncRoot = new();
    private Int32 _current;
    private List<TToken> _tokens = [];
    public Range<TConsumable> Consumables { get; }

    public IReadOnlyList<TToken> Consume()
    {
        lock(_syncRoot)
        {
            Reset();
            while(!IsOutOfBounds())
            {
                MovePure();
                try
                {
                    OnConsume();
                } catch(AnalygatorException ex)
                {
                    Synchronize(ex);
                }
            }

            OnAfterConsume();

            return _tokens;
        }
    }

    private void Reset()
    {
        _current = -1;
        _tokens = [];
    }

    protected abstract void OnConsume();
    protected virtual void Synchronize(AnalygatorException ex) { }
    protected virtual void OnAfterConsume() { }

    protected abstract TDiscriminator GetDiscriminator(TConsumable consumable);

    protected abstract TToken CreateToken(TDiscriminator discriminator, TConsumable consumable);
    protected abstract TToken CreateToken(TDiscriminator discriminator, ReadOnlySpan<TConsumable> consumables);

    protected virtual void OnError(String errorMessage) { }
    protected virtual AnalygatorException CreateError(String errorMessage) => new(errorMessage);

    protected void AddToken(TDiscriminator discriminator, TConsumable consumable)
    {
        var token = CreateToken(discriminator, consumable);
        _tokens.Add(token);
    }
    protected void AddToken(TDiscriminator discriminator)
    {
        var consumable = Get();
        AddToken(discriminator, consumable);
    }
    protected void AddToken(TDiscriminator discriminator, ReadOnlySpan<TConsumable> consumables)
    {
        var token = CreateToken(discriminator, consumables);
        _tokens.Add(token);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Boolean IsOutOfBounds(Int32 offset = 0)
    {
        var result = !Consumables.ContainsIndex(_current + offset);

        return result;
    }

    protected void MovePure(Int32 offset = 1)
    {
        VerifyOffset(offset);
        _current += offset;
    }
    protected Boolean TryMovePure(Int32 offset = 1)
    {
        if(IsOutOfBounds(offset))
            return false;

        _current += offset;
        return true;
    }

    protected TConsumable Get(Int32 offset = 0)
    {
        VerifyOffset(offset);
        var result = Consumables[_current + offset];

        return result;
    }
    protected Boolean TryGet([NotNullWhen(true)] out TConsumable? consumable, Int32 offset = 0)
    {
        if(IsOutOfBounds(offset))
        {
            consumable = default;
            return false;
        }

        consumable = Consumables[_current + offset]!;
        return true;
    }

    protected ReadOnlySpan<TConsumable> GetSpan(Int32 length = 1)
    {
        VerifyOffset(length - 1);
        var result = Consumables.Slice(_current, length);

        return result;
    }
    protected Boolean TryGetSpan(out ReadOnlySpan<TConsumable> consumables, Int32 length = 1)
    {
        if(IsOutOfBounds(length - 1))
        {
            consumables = Array.Empty<TConsumable>();
            return false;
        }

        consumables = Consumables.Slice(_current, length);
        return true;
    }

    protected TConsumable Consume(Int32 offset = 0)
    {
        MovePure(offset);
        var result = Consumables[_current];

        return result;
    }
    protected Boolean TryConsume([NotNullWhen(true)] out TConsumable? consumable, Int32 offset = 0)
    {
        if(TryMovePure(offset))
        {
            consumable = Consumables[_current]!;
            return true;
        }

        consumable = default;
        return false;
    }

    protected ReadOnlySpan<TConsumable> ConsumeSpan(Int32 length = 1)
    {
        var result = Consumables.Slice(_current, length);
        MovePure(length - 1);

        return result;
    }
    protected Boolean TryConsumeSpan(out ReadOnlySpan<TConsumable> consumable, Int32 length = 1)
    {
        var previousCurrent = _current;
        if(TryMovePure(length - 1))
        {
            consumable = Consumables.Slice(previousCurrent, length);
            return true;
        }

        consumable = Array.Empty<TConsumable>();
        return false;
    }

    protected Boolean Match(TDiscriminator expected, [NotNullWhen(true)] out TConsumable? consumable, Int32 offset = 0)
    {
        if(!TryConsume(out consumable, offset))
        {
            consumable = default;
            return false;
        }

        var actual = GetDiscriminator(consumable);
        if(!Equals(actual, expected))
        {
            MovePure(-1 * offset);
            consumable = default;
            return false;
        }

        return true;
    }
    protected Boolean MatchSpan(ReadOnlySpan<TDiscriminator> expecteds, out ReadOnlySpan<TConsumable> consumables, Int32 offset = 0)
    {
        if(!TryConsumeSpan(out consumables, offset))
        {
            consumables = Array.Empty<TConsumable>();
            return false;
        }

        for(var i = 0; i < consumables.Length; i++)
        {
            var consumable = consumables[i];
            var actual = GetDiscriminator(consumable);
            var expected = expecteds[i];
            if(!Equals(actual, expected))
            {
                MovePure(-1 * offset);
                consumables = default;
                return false;
            }
        }

        return true;
    }

    protected void MatchChecked(TDiscriminator expected, [NotNullWhen(true)] out TConsumable? consumable, String errorMessage, Int32 offset = 0, Boolean throwError = false)
    {
        if(Match(expected, out consumable, offset))
            return;
        HandleError(errorMessage, throwError);
    }
    protected void MatchSpanChecked(ReadOnlySpan<TDiscriminator> expecteds, out ReadOnlySpan<TConsumable> consumables, String errorMessage, Int32 offset = 0, Boolean throwError = false)
    {
        if(MatchSpan(expecteds, out consumables, offset))
            return;
        HandleError(errorMessage, throwError);
    }

    private void HandleError(String errorMessage, Boolean throwError)
    {
        OnError(errorMessage);
        if(throwError)
            throw CreateError(errorMessage);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Boolean Equals(TDiscriminator a, TDiscriminator b) =>
        EqualityComparer<TDiscriminator>.Default.Equals(a, b);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InvalidOperationException OutOfBoundsEx(Int32 offset) =>
        new($"Cannot move past consumables limits ({Consumables.LowerBound}:{Consumables.UpperBound}) using the offset provided: {nameof(_current)}({_current}) + {nameof(offset)}({offset}).");
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void VerifyOffset(Int32 offset)
    {
        if(IsOutOfBounds(offset))
        {
            throw OutOfBoundsEx(offset);
        }
    }
}
