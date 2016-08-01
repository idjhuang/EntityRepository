using System;
using System.Collections.Generic;

namespace EntityRepository
{
    public abstract class Query<T, TP>
    {
        protected List<Type> Dependencies;
        protected DateTime LastExecuteTime;
        protected T LastResult;

        protected Query(IEnumerable<Type> dependencies)
        {
            Dependencies = new List<Type>(dependencies);
            LastExecuteTime = DateTime.MinValue;
            LastResult = default(T);
        }

        public virtual T Execute(TP parameters)
        {
            // check update time of dependencies with last execute time
            var lastUpdateTime = DateTime.MinValue;
            foreach (var type in Dependencies)
            {
                var updateTime = (CollectionRepository.LastUpdateTime.ContainsKey(type))? CollectionRepository.LastUpdateTime[type] : DateTime.MaxValue;
                if (DateTime.Compare(lastUpdateTime, updateTime) < 0) lastUpdateTime = updateTime;
            }
            // if update time before last execute time and last result is not default, then return last result directly
            if (DateTime.Compare(LastExecuteTime, lastUpdateTime) > 0 && !LastResult.Equals(default(T)))
                return LastResult;
            // set last execute time and execute query
            LastExecuteTime = DateTime.Now;
            return ExecuteImpl(parameters);
        }

        protected abstract T ExecuteImpl(TP parameters);
    }
}