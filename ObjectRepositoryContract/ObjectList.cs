using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using IDesign.System.Collections.Transactional;

namespace ObjectRepositoryContract
{
    public class ObjectList<T> : IReference, IList<T> where T:IObject
    {
        private TransactionalList<T> _list;

        public ObjectList()
        {
            _list = new TransactionalList<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            // replace list element to simple object to store type and id
            var list = _list.Select(obj => new ObjectBase(obj.Type, obj.Id)).Cast<T>().ToList();
            _list = new TransactionalList<T>(list);
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
            var list = _list.Select(obj => (T)CollectionRepository.GetCollection(obj.Type).GetObject(obj.Id)).ToList();
            _list = new TransactionalList<T>(list);
        }
    }
}