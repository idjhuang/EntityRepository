using System;
using System.Diagnostics;
using EntityRepositoryContract;

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
            var val = objList.GetValue();
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
            var simple1 = CollectionRepository.GetCollection(typeof (Simple)).GetEntity(simple.GetValue().Id) as TransactionalEntity<Simple>;
            Debug.Print($"simple1.GetValue().StrProperty: {simple1.GetValue().StrProperty}");
            var containerList = CollectionRepository.GetCollection(typeof(Container)).GetAllEntities(typeof(Container));
            var container = containerList[0] as TransactionalEntity<Container>;
            Debug.Print($"container.GetValue().StrProperty: {container.GetValue().StrProperty}");
            var objListList = CollectionRepository.GetCollection(typeof(ObjList)).GetAllEntities(typeof(ObjList));
            var objList = objListList[0] as TransactionalEntity<ObjList>;
            Debug.Print($"objList.GetValue().SimpleList[0].GetValue().StrProperty: {objList.GetValue().SimpleList[0].GetValue().StrProperty}");
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
            dynamic entity = obj.GetValue();
            Debug.Print($"entity.StrProp: {entity.StrProp}");
            dynamic m1 = CollectionRepository.GetCollection(type).GetEntity(entity.Id, true);
            Debug.Print($"m1.GetValue().StrProp: {m1.GetValue().StrProp}");
            Debug.Print("Done!");
        }

        public static void Transaction(string connStr)
        {
            var simpleList = CollectionRepository.GetCollection(typeof(Simple)).GetAllEntities(typeof(Simple));
            var simple0 = simpleList[0] as TransactionalEntity<Simple>;
            var simple1 = simpleList[1] as TransactionalEntity<Simple>;
            var origIntProperty = simple0.GetValue().IntProperty;
            var origStrProperty = simple1.GetValue().StrProperty;
            Debug.Print("Before Transaction");
            Debug.Print($"simple0.GetValue().IntProperty: {simple0.GetValue().IntProperty}");
            Debug.Print($"simple1.GetValue().StrProperty: {simple1.GetValue().StrProperty}");
            using (var ts = TransactionScopeUtil.GetTransactionScope())
            using (var dbConn = TransactionScopeUtil.CreateDbConnection(connStr))
            {
                dbConn.Open();
                // retrieve object's value
                var v1 = simple0.GetValue();
                var v2 = simple1.GetValue();
                // change value (would not affect object)
                v1.IntProperty++;
                v2.StrProperty = "Updated!";
                Debug.Print("After change value");
                Debug.Print($"simple0.GetValue().IntProperty: {simple0.GetValue().IntProperty}");
                Debug.Print($"simple1.GetValue().StrProperty: {simple1.GetValue().StrProperty}");
                // update object's value
                simple0.SetValue(v1);
                simple1.SetValue(v2);
                // update to collection
                TransactionScopeUtil.UpdateAll();
                Debug.Print("After update object");
                Debug.Print($"simple0.GetValue().IntProperty: {simple0.GetValue().IntProperty}");
                Debug.Print($"simple1.GetValue().StrProperty: {simple1.GetValue().StrProperty}");
            }
            Debug.Print("After Rollback");
            Debug.Print($"simple0.GetValue().IntProperty: {simple0.GetValue().IntProperty}");
            Debug.Print($"simple1.GetValue().StrProperty: {simple1.GetValue().StrProperty}");
            try
            {
                using (var ts = TransactionScopeUtil.GetTransactionScope())
                using (var dbConn = TransactionScopeUtil.CreateDbConnection(connStr))
                {
                    dbConn.Open();
                    // retrieve object's value
                    var v1 = simple0.GetValue();
                    var v2 = simple1.GetValue();
                    // change value (would not affect object)
                    v1.IntProperty++;
                    v2.StrProperty = "Updated!";
                    // update object's value
                    simple0.SetValue(v1);
                    simple1.SetValue(v2);
                    // update to collection
                    TransactionScopeUtil.UpdateAll();
                    ts.Complete();
                }
                Debug.Print("After Commit");
                Debug.Print($"simple0.GetValue().IntProperty: {simple0.GetValue().IntProperty}");
                Debug.Print($"simple1.GetValue().StrProperty: {simple1.GetValue().StrProperty}");
            }
            catch (Exception e)
            {
                Debug.Print($"Transaction failure: {e.Message}");
            }
            // set value back to original without transaction
            // retrieve object's value
            var val1 = simple0.GetValue();
            var val2 = simple1.GetValue();
            // change value (would not affect object)
            val1.IntProperty = origIntProperty;
            val2.StrProperty = origStrProperty;
            // update object's value
            simple0.SetValue(val1);
            simple1.SetValue(val2);
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