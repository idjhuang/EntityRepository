using System;
using System.Collections;
using System.Collections.Generic;
using IDesign.System.Collections.Transactional;
using Newtonsoft.Json;

namespace EntityRepositoryContract
{
    [Serializable]
    [JsonConverter(typeof(TransactionalEntityListConverter))]
    public class TransactionalEntityList<T> : IReference, IList<TransactionalEntity<T>> where T : Entity
    {
        internal List<TransactionalEntity<T>> List;
        internal List<object> TargetList;
        internal bool Loaded = true;

        public TransactionalEntityList()
        {
            List = new List<TransactionalEntity<T>>();
        }

        public IEnumerator<TransactionalEntity<T>> GetEnumerator()
        {
            if (!Loaded) SetReference();
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!Loaded) SetReference();
            return GetEnumerator();
        }

        public void Add(TransactionalEntity<T> item)
        {
            if (!Loaded) SetReference();
            List.Add(item);
        }

        public void Clear()
        {
            List.Clear();
        }

        public bool Contains(TransactionalEntity<T> item)
        {
            if (!Loaded) SetReference();
            return List.Contains(item);
        }

        public void CopyTo(TransactionalEntity<T>[] array, int arrayIndex)
        {
            if (!Loaded) SetReference();
            List.CopyTo(array, arrayIndex);
        }

        public bool Remove(TransactionalEntity<T> item)
        {
            if (!Loaded) SetReference();
            return List.Remove(item);
        }

        public int Count => List.Count;
        public bool IsReadOnly => false;

        public int IndexOf(TransactionalEntity<T> item)
        {
            if (!Loaded) SetReference();
            return List.IndexOf(item);
        }

        public void Insert(int index, TransactionalEntity<T> item)
        {
            if (!Loaded) SetReference();
            List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (!Loaded) SetReference();
            List.RemoveAt(index);
        }

        public TransactionalEntity<T> this[int index]
        {
            get
            {
                if (!Loaded) SetReference();
                return List[index];
            }
            set
            {
                if (!Loaded) SetReference();
                List[index] = value;
            }
        }

        public void SetReference()
        {
            // set all elements to object form collection by object id
            List = new TransactionalList<TransactionalEntity<T>>();
            foreach (var target in TargetList)
            {
                List.Add(CollectionRepository.GetCollection(typeof (T)).GetEntity(target) as TransactionalEntity<T>);
            }
            TargetList = null;
            Loaded = true;
        }
    }
}