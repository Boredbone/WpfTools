using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;

namespace WpfTools.Models
{

    internal class ItemsManager<T> : IDisposable
    {
        private readonly List<Block<T>> blocks;
        public Block<T> CurrentBlock { get; private set; }

        public int MaxBlockCount
        {
            get { return _fieldMaxBlockCount; }
            set
            {
                if (_fieldMaxBlockCount != value)
                {
                    this.CheckBlockSize(this.BlockSize, value);
                    _fieldMaxBlockCount = value;
                }
            }
        }
        private int _fieldMaxBlockCount;

        public int BlockSize
        {
            get { return _fieldBlockSize; }
            set
            {
                if (_fieldBlockSize != value)
                {
                    this.CheckBlockSize(value, this.MaxBlockCount);
                    _fieldBlockSize = value;
                    this.Clear();
                }
            }
        }
        private int _fieldBlockSize;



        private BehaviorSubject<int> CountSubject { get; }
        public IObservable<int> CountChanged => this.CountSubject.AsObservable();
        public int Count => this.CountSubject.Value;
        private int countInner;


        private Subject<ItemsAddedEventArgs> ItemsAddedSubject { get; }
        public IObservable<ItemsAddedEventArgs> ItemsAdded => this.ItemsAddedSubject.AsObservable();

        private Subject<ItemsRemovedEventArgs> ItemsRemovedSubject { get; }
        public IObservable<ItemsRemovedEventArgs> ItemsRemoved => this.ItemsRemovedSubject.AsObservable();

        private readonly CompositeDisposable disposables;

        private object lockObject = new object();

        public ItemsManager() : this(256, 16)
        {
        }

        public ItemsManager(int blockSize, int maxBlockCount)
        {
            this.CheckBlockSize(blockSize, maxBlockCount);
            this.countInner = 0;

            this.disposables = new CompositeDisposable();

            this.ItemsAddedSubject = new Subject<ItemsAddedEventArgs>().AddTo(this.disposables);
            this.ItemsRemovedSubject = new Subject<ItemsRemovedEventArgs>().AddTo(this.disposables);
            this.CountSubject = new BehaviorSubject<int>(this.countInner).AddTo(this.disposables);


            this._fieldMaxBlockCount = maxBlockCount;
            this._fieldBlockSize = blockSize;

            this.blocks = new List<Block<T>>();
            this.AddBlock(new Block<T>(0, this.BlockSize));
        }

        private void CheckBlockSize(int blockSize, int maxBlockCount)
        {
            if (maxBlockCount < 2 || maxBlockCount >= Block<T>.maxId)
            {
                throw new ArgumentException(nameof(maxBlockCount));
            }
            if (int.MaxValue / blockSize <= maxBlockCount)
            {
                throw new ArgumentException("size over");
            }
        }

        private void AddBlock(Block<T> block)
        {
            this.blocks.Add(block);
            this.CurrentBlock = block;
        }

        private void AddMain(T item)
        {
            var block = this.CurrentBlock.Add(item);
            if (block != null)
            {
                this.AddBlock(block);
                if (this.blocks.Count > this.MaxBlockCount)
                {
                    var removedBlock = this.blocks[0];
                    var blockSize = removedBlock.Count;
                    var removedId = removedBlock.Id;
                    removedBlock.Clear();
                    this.blocks.RemoveAt(0);

                    this.countInner += 1 - blockSize;

                    this.ItemsRemovedSubject.OnNext
                        (new ItemsRemovedEventArgs() { RemovedBlockId = removedId });
                }
                else
                {
                    this.countInner++;
                }
            }
            else
            {
                this.countInner++;
            }
        }

        public BlockItem<T> Add(T item)
        {
            BlockItem<T> result;
            lock (this.lockObject)
            {
                this.AddMain(item);
                result = this.CurrentBlock.Last;
            }

            this.CountSubject.OnNext(this.countInner);
            this.ItemsAddedSubject.OnNext(null);
            return result;
        }

        public BlockItem<T> AddRange(IEnumerable<T> items)
        {
            BlockItem<T> result;
            lock (this.lockObject)
            {
                foreach (var item in items)
                {
                    this.AddMain(item);
                }
                result = this.CurrentBlock.Last;
            }

            this.CountSubject.OnNext(this.countInner);
            this.ItemsAddedSubject.OnNext(null);
            return result;
        }


        public BlockItem<T> GetItemFromReversedIndex(int index)
        {
            lock (this.lockObject)
            {
                Block<T> block = null;
                for (int i = this.blocks.Count - 1; i >= 0; i--)
                {
                    var blockSize = this.blocks[i].Count;
                    if (index < blockSize)
                    {
                        block = this.blocks[i];
                        break;
                    }
                    index -= blockSize;
                }
                if (block == null)
                {
                    return null;
                }
                var itemIndex = block.Count - 1 - index;

                if (index >= 0 && index < block.Count)
                {
                    return block[itemIndex];
                }
                return null;
            }
        }

        public BlockItem<T> GetFirst()
        {
            lock (this.lockObject)
            {
                if (this.blocks.Count <= 0)
                {
                    return null;
                }
                return this.blocks[0].First;
            }
        }

        public void Clear()
        {
            lock (this.lockObject)
            {
                this.blocks.Clear();
                this.CurrentBlock = null;
                this.countInner = 0;
                this.AddBlock(new Block<T>(0, this.BlockSize));
                this.CountSubject.OnNext(this.countInner);
            }
        }

        public int GetReversedIndexOf(BlockItem<T> item)
        {
            if (item == null)
            {
                return -1;
            }
            lock (this.lockObject)
            {
                int index = 0;
                for (int i = this.blocks.Count - 1; i >= 0; i--)
                {
                    if (item.Parent == this.blocks[i])
                    {
                        index += this.blocks[i].GetReversedIndexOf(item);
                        return index;
                    }
                    index += this.blocks[i].Count;
                }
                return -1;
            }
        }

        public int CompareItems(BlockItem<T> baseItem, BlockItem<T> otherItem)
        {
            if (baseItem.Parent == otherItem.Parent)
            {
                return baseItem.Index.CompareTo(otherItem.Index);
            }
            if (baseItem.Parent.Id == otherItem.Parent.Id + 1)
            {
                return 1;
            }
            if (baseItem.Parent.Id == otherItem.Parent.Id - 1)
            {
                return -1;
            }
            var firstBlock = this.blocks.FindIndex(x => x == baseItem.Parent);
            var secondBlock = this.blocks.FindIndex(x => x == otherItem.Parent);

            if (firstBlock < 0)
            {
                if (secondBlock < 0)
                {
                    return 0;
                }
                return -1;
            }
            if (secondBlock < 0)
            {
                return 1;
            }
            return firstBlock.CompareTo(secondBlock);
        }

        public void Dispose()
        {
            this.disposables.Dispose();
        }
    }

    internal class ItemsAddedEventArgs
    {
    }

    internal class ItemsRemovedEventArgs
    {
        public int RemovedBlockId { get; set; }
    }
}
