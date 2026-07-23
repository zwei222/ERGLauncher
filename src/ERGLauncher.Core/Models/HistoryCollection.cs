using System.Collections;

namespace ERGLauncher.Core.Models;

public sealed class HistoryCollection<T> : IEnumerable<T>, ICloneable
{
    private readonly List<T> items;

    public HistoryCollection()
    {
        this.items = [];
        this.Index = -1;
    }

    public HistoryCollection(T firstValue)
    {
        this.items = [firstValue];
        this.Index = 0;
    }

    private HistoryCollection(IEnumerable<T> items, int index)
    {
        this.items = [.. items];
        this.Index = index;
    }

    public int Count => this.items.Count;

    public int Index { get; private set; }

    public T CurrentValue => this.Index >= 0
        ? this.items[this.Index]
        : throw new InvalidOperationException("History is empty.");

    public bool IsEnabledUndo => this.Index > 0;

    public bool IsEnabledRedo => this.Index >= 0 && this.Index < this.items.Count - 1;

    public T this[int index] => this.items[index];

    public void Push(T value)
    {
        if (this.Index < this.items.Count - 1)
        {
            this.items.RemoveRange(this.Index + 1, this.items.Count - this.Index - 1);
        }

        this.items.Add(value);
        this.Index = this.items.Count - 1;
    }

    public T? Back()
    {
        if (!this.IsEnabledUndo)
        {
            return default;
        }

        return this.items[--this.Index];
    }

    public T? At(int index)
    {
        if ((uint)index >= (uint)this.items.Count)
        {
            return default;
        }

        this.Index = index;
        return this.items[index];
    }

    public T? Forward()
    {
        if (!this.IsEnabledRedo)
        {
            return default;
        }

        return this.items[++this.Index];
    }

    public bool Remove(T value)
    {
        var removedIndex = this.items.IndexOf(value);
        if (removedIndex < 0)
        {
            return false;
        }

        this.items.RemoveAt(removedIndex);
        if (this.items.Count == 0)
        {
            this.Index = -1;
        }
        else if (removedIndex < this.Index)
        {
            this.Index--;
        }
        else if (removedIndex == this.Index)
        {
            this.Index = Math.Min(this.Index, this.items.Count - 1);
        }

        return true;
    }

    public T? Peek() => this.Index > 0 ? this.items[this.Index - 1] : default;

    public void Clear()
    {
        this.items.Clear();
        this.Index = -1;
    }

    public object Clone() => new HistoryCollection<T>(this.items, this.Index);

    public IEnumerator<T> GetEnumerator() => this.items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
