using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ERGLauncher.Core
{
    /// <summary>
    /// Manage operation history.
    /// </summary>
    /// <typeparam name="T">History type</typeparam>
    public class HistoryCollection<T> : IEnumerable<T>, ICloneable
    {
        /// <summary>
        /// History.
        /// </summary>
        private List<T> list = new List<T>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public HistoryCollection() {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="firstValue">The first value</param>
        public HistoryCollection(T firstValue)
        {
            this.list.Add(firstValue);
            this.Index = 0;
        }

        /// <summary>
        /// History count.
        /// </summary>
        public int Count => this.list.Count;

        /// <summary>
        /// History index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Current history value.
        /// </summary>
        public T CurrentValue => this.list[this.Index];

        /// <summary>
        /// <see langword="true" /> if you can go back; otherwise <see langword="false" />.
        /// </summary>
        public bool IsEnabledUndo => this.Index > 0;

        /// <summary>
        /// <see langword="true" /> if you can go forward; otherwise <see langword="false" />.
        /// </summary>
        public bool IsEnabledRedo => this.Index < this.list.Count - 1;

        /// <summary>
        /// Indexer.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>History value</returns>
        public T this[int index] => this.list[index];

        /// <summary>
        /// Push a value to history.
        /// </summary>
        /// <param name="value">New history</param>
        public void Push(T value)
        {
            if (this.Index < this.list.Count - 1)
            {
                this.list.RemoveRange(this.Index + 1, this.list.Count - 1 - this.Index);
            }

            this.list.Add(value);
            this.Index = this.list.Count - 1;
        }

        /// <summary>
        /// Go back history.
        /// </summary>
        /// <returns>Value</returns>
        public T Back()
        {
            if (this.Index > 0)
            {
                this.Index--;

                return this.list[this.Index];
            }

            return default!;
        }

        /// <summary>
        /// Acquires the history of the specified position.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value at specified position</returns>
        public T At(int index)
        {
            if (index < 0 || index >= this.list.Count)
            {
                return default!;
            }

            this.Index = index;

            return this.list[index];
        }

        /// <summary>
        /// Go forward history.
        /// </summary>
        /// <returns>Value</returns>
        public T Forward()
        {
            if (this.Index < this.list.Count - 1)
            {
                this.Index++;

                return this.list[this.Index];
            }

            return default!;
        }

        /// <summary>
        /// Remove history.
        /// </summary>
        /// <param name="value">The target value</param>
        /// <returns><see langword="true" /> if the value was deleted; otherwise <see langword="false" /></returns>
        public bool Remove(T value)
        {
            var index = this.list.IndexOf(value);

            if (index < 0)
            {
                return false;
            }

            this.list.RemoveAt(index);

            if (this.Index >= index)
            {
                this.Index--;
            }

            return true;
        }

        /// <summary>
        /// Returns the value at the beginning.
        /// </summary>
        /// <returns>The value at the beginning</returns>
        public T Peek()
        {
            if (this.Index > 0)
            {
                return this.list[this.Index - 1];
            }

            return default!;
        }

        /// <summary>
        /// Clear the history.
        /// </summary>
        public void Clear()
        {
            this.list.Clear();
            this.Index = -1;
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        /// <inheritdoc />
        public object Clone()
        {
            var history = new HistoryCollection<T>();

            history.Index = this.Index;
            history.list = this.list.ToList();

            return history;
        }
    }
}
