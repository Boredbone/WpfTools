using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTools.Models
{

    internal class Block<T>
    {

        public BlockItem<T> First { get; private set; }
        public BlockItem<T> Last { get; private set; }

        public int Id { get; }
        public const int maxId = 0xFFFF;

        public int Count => this.items.Count;

        private readonly List<BlockItem<T>> items;
        public readonly int maxSize;


        public Block(int id, int blockSize)
        {
            this.items = new List<BlockItem<T>>(blockSize);
            this.maxSize = blockSize;
            if (id > maxId)
            {
                id = 0;
            }
            this.Id = id;
        }

        public Block<T> Add(T value)
        {
            if (this.items.Count >= this.maxSize)
            {
                var block = new Block<T>(this.Id + 1, this.maxSize);
                block.Add(value);
                block.First.PrevItem = this.Last;
                return block;
            }
            else
            {
                var item = new BlockItem<T>(value, this, this.items.Count);

                if (this.First == null)
                {
                    this.First = item;
                }
                if (this.Last != null)
                {
                    this.Last.NextItem = item;
                }
                this.Last = item;
                this.items.Add(item);

                return null;
            }
        }

        public void Clear()
        {
            if (this.First != null)
            {
                this.First.PrevItem = null;
                this.First = null;
            }
            if (this.Last != null)
            {
                this.Last.NextItem = null;
                this.Last = null;
            }
            this.items.Clear();
        }



        public int GetReversedIndexOf(BlockItem<T> item)
            => (item.Parent == this) ? (this.items.Count - item.Index - 1) : -1;


        public BlockItem<T> this[int index] => this.items[index];
    }
}
