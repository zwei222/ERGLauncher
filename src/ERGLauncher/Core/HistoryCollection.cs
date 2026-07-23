using System.Collections;

namespace ERGLauncher.Core;

/// <summary>
/// Stores navigation history and tracks the current position.
/// </summary>
public sealed class HistoryCollection<T> : IEnumerable<T>, ICloneable
{
    private List<T> values = [];

    public HistoryCollection()
    {
    }

    public HistoryCollection(T firstValue)
    {
        values.Add(firstValue);
        Index = 0;
    }

    public int Count => values.Count;

    public int Index { get; set; }

    public T CurrentValue => values[Index];

    public bool IsEnabledUndo => Index > 0;

    public bool IsEnabledRedo => Index < values.Count - 1;

    public T this[int index] => values[index];

    public void Push(T value)
    {
        if (Index < values.Count - 1)
        {
            values.RemoveRange(Index + 1, values.Count - 1 - Index);
        }

        values.Add(value);
        Index = values.Count - 1;
    }

    public T Back()
    {
        if (Index <= 0)
        {
            return default!;
        }

        Index--;
        return values[Index];
    }

    public T At(int index)
    {
        if (index < 0 || index >= values.Count)
        {
            return default!;
        }

        Index = index;
        return values[index];
    }

    public T Forward()
    {
        if (Index >= values.Count - 1)
        {
            return default!;
        }

        Index++;
        return values[Index];
    }

    public bool Remove(T value)
    {
        var index = values.IndexOf(value);
        if (index < 0)
        {
            return false;
        }

        values.RemoveAt(index);
        if (Index >= index)
        {
            Index--;
        }

        return true;
    }

    public T Peek() => Index > 0 ? values[Index - 1] : default!;

    public void Clear()
    {
        values.Clear();
        Index = -1;
    }

    public IEnumerator<T> GetEnumerator() => values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public object Clone() => new HistoryCollection<T>
    {
        Index = Index,
        values = [.. values],
    };
}
