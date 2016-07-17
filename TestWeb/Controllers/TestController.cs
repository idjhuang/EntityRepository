using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using ObjectRepositoryContract;
using TestWeb.Models;

namespace TestWeb.Controllers
{
    public class TestController : ApiController
    {
        public string Get(string id)
        {
            var c1List = CollectionRepository.GetCollection(typeof(C1)).GetAllObjects(typeof(C1));
            switch (id)
            {
                case "Load":
                    TestLoad();
                    break;
                case "Insert":
                    TestInsert();
                    break;
                case "Dynamic":
                    TestDynamic();
                    break;
            }
            return "Done!";
        }

        private void TestLoad()
        {
            var c1List = CollectionRepository.GetCollection(typeof(C1)).GetAllObjects(typeof(C1));
            var c2List = CollectionRepository.GetCollection(typeof(C2)).GetAllObjects(typeof(C2));
            var c3List = CollectionRepository.GetCollection(typeof(C3)).GetAllObjects(typeof(C3));
            var c2 = c2List[0] as TransactionalObject<C2>;
            var c3 = c3List[0] as TransactionalObject<C3>;
            Debug.Print("done!");
        }

        private void TestInsert()
        {
            var c1_1 = new TransactionalObject<C1>(new C1(Guid.NewGuid()) { I1 = 100, S1 = "S1_1" });
            CollectionRepository.GetCollection(typeof(C1)).InsertObject(c1_1);
            var c1_2 = new TransactionalObject<C1>(new C1(Guid.NewGuid()) { I1 = 200, S1 = "S1_2" });
            CollectionRepository.GetCollection(typeof(C1)).InsertObject(c1_2);
            var c1_3 = new TransactionalObject<C1>(new C1(Guid.NewGuid()) { I1 = 300, S1 = "S1_3" });
            CollectionRepository.GetCollection(typeof(C1)).InsertObject(c1_3);

            var c2 =
                new TransactionalObject<C2>(new C2(Guid.NewGuid())
                {
                    S2 = "S2",
                    C1Reference = new ObjectReference<C1>(c1_1)
                });
            CollectionRepository.GetCollection(typeof(C2)).InsertObject(c2);

            var c3 =
                new TransactionalObject<C3>(new C3(Guid.NewGuid())
                {
                    C1List = new ObjectList<C1>()
                });
            c3.Value.C1List.Add(c1_1);
            c3.Value.C1List.Add(c1_2);
            c3.Value.C1List.Add(c1_3);
            CollectionRepository.GetCollection(typeof(C3)).InsertObject(c3);
        }

        private void TestDynamic()
        {
            var type = TypeRepository.GetType("TestData.M1");
            dynamic d = Activator.CreateInstance(type, Guid.NewGuid());
            d.M1S1 = "Test of dynamic obj";
            d.M1I1 = 1000;
            var obj = TransactionalObject.CreateTransactionalObject(type, d);
            CollectionRepository.GetCollection(type).InsertObject(obj);
            var mList = CollectionRepository.GetCollection(type).GetAllObjects(type);
            var m1 = mList[0];
            Debug.Print("Done!");
        }
    }
}
