//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using System.Diagnostics;

using ObjectResourceManager;

namespace IDesign.System.Collections.Transactional
{
   public abstract class TransactionalCollection<C,T> : Transactional<C>,IEnumerable<T> where C : IEnumerable<T>
   {
      public TransactionalCollection(C collection)
      {
         Value = collection;
      }
      IEnumerator<T> IEnumerable<T>.GetEnumerator()
      {
         return Value.GetEnumerator();
      }
      IEnumerator IEnumerable.GetEnumerator()
      {
         IEnumerable<T> enumerable = this;
         return enumerable.GetEnumerator();
      }
   }
}