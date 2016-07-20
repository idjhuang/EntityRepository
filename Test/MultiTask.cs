using System;
using System.Diagnostics;
using System.Threading;
using System.Transactions;
using EntityRepositoryContract;
using ObjectResourceManager;

namespace Test
{
    public class MultiTask
    {
        private readonly string _connStr;
        private readonly TransactionalEntity<Simple> _obj;

        public MultiTask(string connStr)
        {
            _connStr = connStr;
            var simpleList = CollectionRepository.GetCollection(typeof(Simple)).GetAllEntities(typeof(Simple));
            _obj = simpleList[0] as TransactionalEntity<Simple>;
        }

        public void Test()
        {
            var origIntProperty = _obj.Value.IntProperty;
            var origStrProperty = _obj.Value.StrProperty;
            var observer = new Thread(Observer);
            observer.Start();
            var thread0 = new Thread(Task0);
            thread0.Start();
            Thread.Sleep(1000);
            var thread1 = new Thread(Task1);
            thread1.Start();
            var thread2 = new Thread(Task2);
            thread2.Start();
            var thread3 = new Thread(Task3);
            thread3.Start();
            var thread4 = new Thread(Task4);
            thread4.Start();
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection(_connStr))
                {
                    dbConn.Open();
                    Debug.Print("Thread main => Before update object from main thread.");
                    Debug.Print($"Thread main => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    var v = _obj.Value;
                    v.IntProperty = 1000;
                    v.StrProperty = "updated by main thread!";
                    _obj.Value = v;
                    Thread.Sleep(1000);
                    TransactionScopeUtil.UpdateAll();
                    Debug.Print("Thread main => After update object from main thread.");
                    Debug.Print($"Thread main => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    ts.Complete();
                }
                Debug.Print("Thread main => After commit from main thread.");
                Debug.Print($"Thread main => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
            }
            catch (Exception e)
            {
                Debug.Print($"Main thread transaction failure: {e.Message}");
            }
            Thread.Sleep(6000);
            observer.Abort();
            // set value back to ogiginal without transaction
            var val = _obj.Value;
            val.IntProperty = origIntProperty;
            val.StrProperty = origStrProperty;
            _obj.Value = val;
            _obj.Update();
            Debug.Print("Done!");
        }

        public void Observer()
        {
            using (var ts = TransactionScopeUtil.GetTransactionScope(IsolationLevel.ReadUncommitted))
            {
                while (true)
                {
                    Debug.Print($"Observer => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    Thread.Sleep(100);
                }
            }
        }

        public void Task0()
        {
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection(_connStr))
                {
                    dbConn.Open();
                    Debug.Print("Thread 0 => Before update object from thread 0.");
                    Debug.Print($"Thread 0 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    var v = _obj.GetValue(LockMode.Exclusive);
                    Debug.Print("Thread 0 get exclusive lock.");
                    v.IntProperty = 99999;
                    v.StrProperty = "updated by thread 0!";
                    Debug.Print("Thread 0 => Thread 0 read object value.");
                    _obj.Value = v;
                    Debug.Print("Thread 0 => Thread 0 set object value.");
                    TransactionScopeUtil.UpdateAll();
                    Debug.Print("Thread 0 => After update object from thread 0.");
                    Debug.Print($"Thread 0 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    Thread.Sleep(2000);
                    Debug.Print("Thread 0 => Beform commit from thread 0.");
                    ts.Complete();
                }
                Debug.Print("Thread 0 => After commit from thread 0.");
                Debug.Print($"Thread 0 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
            }
            catch (Exception e)
            {
                Debug.Print($"Thread 0 transaction failure: {e.Message}");
            }
        }

        public void Task1()
        {
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection(_connStr))
                {
                    dbConn.Open();
                    Debug.Print("Thread 1 => Before update object from thread 1.");
                    Debug.Print($"Thread 1 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    var v = _obj.Value;
                    v.IntProperty = 1111;
                    v.StrProperty = "Thread 1 => updated by thread 1!";
                    _obj.Value = v;
                    TransactionScopeUtil.UpdateAll();
                    Debug.Print("Thread 1 => After update object from thread 1.");
                    Debug.Print($"Thread 1 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    ts.Complete();
                }
                Debug.Print("Thread 1 => After commit from thread 1.");
                Debug.Print($"Thread 1 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
            }
            catch (Exception e)
            {
                Debug.Print($"Thread 1 transaction failure: {e.Message}");
            }
        }

        public void Task2()
        {
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection(_connStr))
                {
                    dbConn.Open();
                    Debug.Print("Thread 2 => Before update object from thread 2.");
                    Debug.Print($"Thread 2 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    var v = _obj.GetValue(LockMode.Exclusive);
                    Debug.Print("Thread 2 => Thread 2 get exclusive lock.");
                    v.IntProperty = 2222;
                    v.StrProperty = "Thread 2 => updated by thread 2!";
                    Debug.Print("Thread 2 => Thread 2 read object value.");
                    _obj.Value = v;
                    Debug.Print("Thread 2 => Thread 2 set object value.");
                    Thread.Sleep(5000);
                    TransactionScopeUtil.UpdateAll();
                    Debug.Print("Thread 2 => After update object from thread 2.");
                    Debug.Print($"Thread 2 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    ts.Complete();
                }
                Debug.Print("Thread 2 => After commit from thread 2.");
                Debug.Print($"Thread 2 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
            }
            catch (Exception e)
            {
                Debug.Print($"Thread 2 transaction failure: {e.Message}");
            }
        }

        public void Task3()
        {
            try
            {
                Debug.Print("Thread 3 => Before read object from thread 3.");
                Debug.Print($"Thread 3 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                var v = _obj.Value;
                v.IntProperty = 3333;
                v.StrProperty = "Thread 3 => updated by thread 3!";
                Debug.Print("Thread 3 => Before update object from thread 3.");
                _obj.Update(v);
                Debug.Print("Thread 3 => After update object from thread 3.");
                Debug.Print($"Thread 3 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
            }
            catch (Exception e)
            {
                Debug.Print($"Thread 3 failure: {e.Message}");
            }
        }

        public void Task4()
        {
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection(_connStr))
                {
                    dbConn.Open();
                    Debug.Print("Thread 4 => Before update object from thread 4.");
                    Debug.Print($"Thread 4 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    var v = _obj.Value;
                    v.IntProperty = 4444;
                    v.StrProperty = "Thread 4 => updated by thread 4!";
                    _obj.Value = v;
                    Thread.Sleep(5000);
                    TransactionScopeUtil.UpdateAll();
                    Debug.Print("Thread 4 => After update object from thread 4.");
                    Debug.Print($"Thread 4 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
                    ts.Complete();
                }
                Debug.Print("Thread 4 => After commit from thread 4.");
                Debug.Print($"Thread 4 => obj.Value.IntProperty: {_obj.Value.IntProperty}, obj.Value.StrProperty: {_obj.Value.StrProperty}");
            }
            catch (Exception e)
            {
                Debug.Print($"Thread 4 transaction failure: {e.Message}");
            }
        }
    }
}