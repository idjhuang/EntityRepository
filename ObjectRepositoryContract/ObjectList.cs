using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using IDesign.System.Collections.Transactional;

namespace ObjectRepositoryContract
{
    public class ObjectList<T> : IReference, IList<TransactionalObject<T>> where T:ObjectBase
    {
        private TransactionalList<TransactionalObject<T>> _list;

        public ObjectList()
        {
            _list = new TransactionalList<TransactionalObject<T>>();
        }

        public IEnumerator<TransactionalObject<T>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TransactionalObject<T> item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(TransactionalObject<T> item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(TransactionalObject<T>[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(TransactionalObject<T> item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public int IndexOf(TransactionalObject<T> item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, TransactionalObject<T> item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public TransactionalObject<T> this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            // replace list element to simple object to store type and id
            var list = new TransactionalList<TransactionalObject<T>>();
            foreach (var obj in _list)
            {
                list.Add(new TransactionalObject<T>(new ObjectBase(obj.Value.Type, obj.Value.Id) as T));
            }
            _list = list;
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            // register to CollectionRepository for later process
            CollectionRepository.RegisterReference(this);
        }

        public void SetReference()
        {
            // set all elements to object form collection by object id
            var list = new TransactionalList<TransactionalObject<T>>();
            foreach (var obj in _list)
            {
                list.Add(new TransactionalObject<T>(CollectionRepository.GetCollection(obj.Value.Type).GetObject(obj.Value.Id) as T));
            }
            _list = list;
        }
    }
}