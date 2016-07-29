using System;
using System.Diagnostics;
using EntityRepository;

namespace Test
{
    public static class TestCases
    {
        public static void InsertObjects()
        {
            var simple_1 = new TransactionalEntity<Simple>(new Simple(Guid.NewGuid()) { IntProperty = 100, StrProperty = "StrProperty_1" });
            simple_1.Update();
            var simple_2 = new TransactionalEntity<Simple>(new Simple(Guid.NewGuid()) { IntProperty = 200, StrProperty = "StrProperty_2" });
            simple_2.Update();
            var simple_3 = new TransactionalEntity<Simple>(new Simple(Guid.NewGuid()) { IntProperty = 300, StrProperty = "StrProperty_3" });
            simple_3.Update();

            var container =
                new TransactionalEntity<Container>(new Container(Guid.NewGuid())
                {
                    StrProperty = "S2",
                    SimpleReference = new TransactionalEntityReference<Simple>(simple_1)
                });
            container.Update();

            var objList =
                new TransactionalEntity<ObjList>(new ObjList(Guid.NewGuid())
                {
                    SimpleList = new TransactionalEntityList<Simple>(),
                    IntProperty = 123
                });
            var val = objList.GetEntity();
            val.SimpleList.Add(simple_1);
            val.SimpleList.Add(simple_2);
            val.SimpleList.Add(simple_3);
            objList.Update(val);
            Debug.Print("Done");
        }

        public static void LoadObjects()
        {
            var simpleList = CollectionRepository.GetCollection(typeof(Simple)).GetAllEntities(typeof(Simple));
            var simple = simpleList[1] as TransactionalEntity<Simple>;
            var simple1 = CollectionRepository.GetCollection(typeof (Simple)).GetEntity(simple.GetEntity().Id) as TransactionalEntity<Simple>;
            Debug.Print($"simple1.GetEntity().StrProperty: {simple1.GetEntity().StrProperty}");
            var containerList = CollectionRepository.GetCollection(typeof(Container)).GetAllEntities(typeof(Container));
            var container = containerList[0] as TransactionalEntity<Container>;
            Debug.Print($"container.GetEntity().StrProperty: {container.GetEntity().StrProperty}");
            var objListList = CollectionRepository.GetCollection(typeof(ObjList)).GetAllEntities(typeof(ObjList));
            var objList = objListList[0] as TransactionalEntity<ObjList>;
            Debug.Print($"objList.GetEntity().SimpleList[0].GetEntity().StrProperty: {objList.GetEntity().SimpleList[0].GetEntity().StrProperty}");
            Debug.Print("Done");
        }

        public static void InsertAndLoadExtensionObjects()
        {
            var type = TypeRepository.GetType("TestExtension.M1");
            dynamic d = Activator.CreateInstance(type, new object[] {Guid.NewGuid()});
            d.StrProp = "Test Extension";
            d.IntProp = 123456;
            dynamic obj = TransactionalEntity.CreateTransactionalEntity(type, d);
            obj.Update();
            dynamic entity = obj.GetEntity();
            Debug.Print($"entity.StrProp: {entity.StrProp}");
            dynamic m1 = CollectionRepository.GetCollection(type).GetEntity(entity.Id, true);
            Debug.Print($"m1.GetEntity().StrProp: {m1.GetEntity().StrProp}");
            Debug.Print("Done!");
        }

        public static void Transaction(string connStr)
        {
            var simpleList = CollectionRepository.GetCollection(typeof(Simple)).GetAllEntities(typeof(Simple));
            var simple0 = simpleList[0] as TransactionalEntity<Simple>;
            var simple1 = simpleList[1] as TransactionalEntity<Simple>;
            var origIntProperty = simple0.GetEntity().IntProperty;
            var origStrProperty = simple1.GetEntity().StrProperty;
            Debug.Print("Before Transaction");
            Debug.Print($"simple0.GetEntity().IntProperty: {simple0.GetEntity().IntProperty}");
            Debug.Print($"simple1.GetEntity().StrProperty: {simple1.GetEntity().StrProperty}");
            using (var ts = TransactionScopeUtil.GetTransactionScope())
            using (var dbConn = TransactionScopeUtil.CreateDbConnection(connStr))
            {
                dbConn.Open();
                // retrieve object's value
                var v1 = simple0.GetEntity();
                var v2 = simple1.GetEntity();
                // change value (would not affect object)
                v1.IntProperty++;
                v2.StrProperty = "Updated!";
                Debug.Print("After change value");
                Debug.Print($"simple0.GetEntity().IntProperty: {simple0.GetEntity().IntProperty}");
                Debug.Print($"simple1.GetEntity().StrProperty: {simple1.GetEntity().StrProperty}");
                // update object's value
                simple0.SetEntity(v1);
                simple1.SetEntity(v2);
                // update to collection
                TransactionScopeUtil.UpdateAll();
                Debug.Print("After update object");
                Debug.Print($"simple0.GetEntity().IntProperty: {simple0.GetEntity().IntProperty}");
                Debug.Print($"simple1.GetEntity().StrProperty: {simple1.GetEntity().StrProperty}");
            }
            Debug.Print("After Rollback");
            Debug.Print($"simple0.GetEntity().IntProperty: {simple0.GetEntity().IntProperty}");
            Debug.Print($"simple1.GetEntity().StrProperty: {simple1.GetEntity().StrProperty}");
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection(connStr))
                {
                    dbConn.Open();
                    // retrieve object's value
                    var v1 = simple0.GetEntity();
                    var v2 = simple1.GetEntity();
                    // change value (would not affect object)
                    v1.IntProperty++;
                    v2.StrProperty = "Updated!";
                    // update object's value
                    simple0.SetEntity(v1);
                    simple1.SetEntity(v2);
                    // update to collection
                    TransactionScopeUtil.UpdateAll();
                    ts.Complete();
                }
                Debug.Print("After Commit");
                Debug.Print($"simple0.GetEntity().IntProperty: {simple0.GetEntity().IntProperty}");
                Debug.Print($"simple1.GetEntity().StrProperty: {simple1.GetEntity().StrProperty}");
            }
            catch (Exception e)
            {
                Debug.Print($"Transaction failure: {e.Message}");
            }
            // set value back to original without transaction
            // retrieve object's value
            var val1 = simple0.GetEntity();
            var val2 = simple1.GetEntity();
            // change value (would not affect object)
            val1.IntProperty = origIntProperty;
            val2.StrProperty = origStrProperty;
            // update object's value
            simple0.SetEntity(val1);
            simple1.SetEntity(val2);
            // update to collection
            TransactionScopeUtil.UpdateAll();
            Debug.Print("Done!");
        }

        public static void MultiTask(string connStr)
        {
            var multiTask = new MultiTask(connStr);
            multiTask.Test();
        }
    }
}