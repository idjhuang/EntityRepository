using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using IDesign.System.Collections.Transactional;
using Newtonsoft.Json;

namespace ObjectRepositoryContract
{
    [JsonConverter(typeof(ObjectListConveter))]
    public class ObjectList<T> : IReference, ISerializable, IList<TransactionalObject<T>> where T: ObjectValue //
    {
        internal List<TransactionalObject<T>> List;
        internal List<ObjectValue> TargetList; 
        internal bool Loaded = true;

        public ObjectList()
        {
            List = new List<TransactionalObject<T>>();
        }

        public ObjectList(SerializationInfo info, StreamingContext context)
        {
            List = new List<TransactionalObject<T>>();
            TargetList = (List<ObjectValue>)info.GetValue("List", typeof(List<ObjectValue>));
            Loaded = false;
        }

        public IEnumerator<TransactionalObject<T>> GetEnumerator()
        {
            if (!Loaded) SetReference();
            return List.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!Loaded) SetReference();
            return GetEnumerator();
        }
        
        public void Add(TransactionalObject<T> item)
        {
            if (!Loaded) SetReference();
            List.Add(item);
        }

        public void Clear()
        {
            List.Clear();
        }

        public bool Contains(TransactionalObject<T> item)
        {
            if (!Loaded) SetReference();
            return List.Contains(item);
        }

        public void CopyTo(TransactionalObject<T>[] array, int arrayIndex)
        {
            if (!Loaded) SetReference();
            List.CopyTo(array, arrayIndex);
        }

        public bool Remove(TransactionalObject<T> item)
        {
            if (!Loaded) SetReference();
            return List.Remove(item);
        }

        public int Count => List.Count;
        public bool IsReadOnly => false;

        public int IndexOf(TransactionalObject<T> item)
        {
            if (!Loaded) SetReference();
            return List.IndexOf(item);
        }

        public void Insert(int index, TransactionalObject<T> item)
        {
            if (!Loaded) SetReference();
            List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (!Loaded) SetReference();
            List.RemoveAt(index);
        }

        public TransactionalObject<T> this[int index]
        {
            get { if (!Loaded) SetReference(); return List[index]; }
            set { if (!Loaded) SetReference(); List[index] = value; }
        }

        public void SetReference()
        {
            // set all elements to object form collection by object id
            List = new TransactionalList<TransactionalObject<T>>();
            foreach (var target in TargetList)
            {
                List.Add(CollectionRepository.GetCollection(target.Type).GetObject(target.Id) as TransactionalObject<T>);
            }
            TargetList = null;
            Loaded = true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var list = List.Select(obj => new ObjectValue(obj.Value.Type, obj.Value.Id)).ToList();
            info.AddValue("List", list, typeof(List<ObjectValue>));
        }
    }
}