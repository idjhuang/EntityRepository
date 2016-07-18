using System;
using System.Collections.Generic;
using System.Diagnostics;
using ObjectRepositoryContract;

namespace Test
{
    public static class TestCases
    {
        public static void InsertObjects()
        {
            var simple_1 = new TransactionalObject<Simple>(new Simple(Guid.NewGuid()) { IntProperty = 100, StrProperty = "StrProperty_1" });
            CollectionRepository.GetCollection(typeof(Simple)).InsertObject(simple_1);
            var simple_2 = new TransactionalObject<Simple>(new Simple(Guid.NewGuid()) { IntProperty = 200, StrProperty = "StrProperty_2" });
            CollectionRepository.GetCollection(typeof(Simple)).InsertObject(simple_2);
            var simple_3 = new TransactionalObject<Simple>(new Simple(Guid.NewGuid()) { IntProperty = 300, StrProperty = "StrProperty_3" });
            CollectionRepository.GetCollection(typeof(Simple)).InsertObject(simple_3);

            var container =
                new TransactionalObject<Container>(new Container(Guid.NewGuid())
                {
                    StrProperty = "S2",
                    SimpleReference = new ObjectReference<Simple>(simple_1)
                });
            CollectionRepository.GetCollection(typeof(Container)).InsertObject(container);

            var objList =
                new TransactionalObject<ObjList>(new ObjList(Guid.NewGuid())
                {
                    SimpleList = new ObjectList<Simple>(),
                    IntProperty = 123
                });
            objList.Value.SimpleList.Add(simple_1);
            objList.Value.SimpleList.Add(simple_2);
            objList.Value.SimpleList.Add(simple_3);
            CollectionRepository.GetCollection(typeof(ObjList)).InsertObject(objList);
            Debug.Print("Done");
        }

        public static void LoadObjects()
        {
            var simpleList = CollectionRepository.GetCollection(typeof(Simple)).GetAllObjects(typeof(Simple));
            var simple = simpleList[1] as TransactionalObject<Simple>;
            var simple1 = CollectionRepository.GetCollection(typeof (Simple)).GetObject(simple.Value.Id) as TransactionalObject<Simple>;
            Debug.Print($"simple1.Value.StrProperty: {simple1.Value.StrProperty}");
            var containerList = CollectionRepository.GetCollection(typeof(Container)).GetAllObjects(typeof(Container));
            var container = containerList[0] as TransactionalObject<Container>;
            Debug.Print($"container.Value.StrProperty: {container.Value.StrProperty}");
            var objListList = CollectionRepository.GetCollection(typeof(ObjList)).GetAllObjects(typeof(ObjList));
            var objList = objListList[0] as TransactionalObject<ObjList>;
            Debug.Print($"objList.Value.SimpleList[0].Value.StrProperty: {objList.Value.SimpleList[0].Value.StrProperty}");
            Debug.Print("Done");
        }

        public static void InsertAndLoadExtensionObjects()
        {
            var type = TypeRepository.GetType("TestExtension.M1");
            dynamic d = Activator.CreateInstance(type, Guid.NewGuid());
            d.StrProp = "Test Extension";
            d.IntProp = 1000;
            var obj = TransactionalObject.CreateTransactionalObject(type, d);
            CollectionRepository.GetCollection(type).InsertObject(obj);
            var mList = CollectionRepository.GetCollection(type).GetAllObjects(type);
            dynamic m1 = mList[0];
            Debug.Print($"m1.Value.StrProp: {m1.Value.StrProp}");
            Debug.Print("Done!");
        }
    }
}