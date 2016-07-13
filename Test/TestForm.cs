using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using IDesign.System.Collections.Transactional;
using Newtonsoft.Json;
using ObjectRepositoryContract;
using ObjectResourceManager;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Test
{
    public partial class TestForm : Form
    {
        private readonly Transactional<Value> _obj = new Transactional<Value>(new Value()); 
        private Transactional<int> _value = new Transactional<int>();
        private TransactionalList<Value> _list = new TransactionalList<Value>(); 

        public TestForm()
        {
            InitializeComponent();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            Test1();
        }

        private void Test1()
        {
            var testCollection = new TestCollection();
            CollectionRepository.SetCollections(new Dictionary<string, ICollection>() {{"", testCollection}});

            var c1_1 = new TransactionalObject<C1>(new C1("Test.C1", Guid.NewGuid()) { I1 = 100, S1 = "S1_1" });
            CollectionRepository.GetCollection("Test.C1").InsertObject(c1_1);
            var c1_2 = new TransactionalObject<C1>(new C1("Test.C1", Guid.NewGuid()) { I1 = 200, S1 = "S1_2" });
            CollectionRepository.GetCollection("Test.C1").InsertObject(c1_2);
            var c1_3 = new TransactionalObject<C1>(new C1("Test.C1", Guid.NewGuid()) { I1 = 300, S1 = "S1_3" });
            CollectionRepository.GetCollection("Test.C1").InsertObject(c1_3);

            var c2 =
                new TransactionalObject<C2>(new C2("Test.C2", Guid.NewGuid())
                {
                    S2 = "S2",
                    C1Reference = new ObjectReference<C1>(c1_1)
                });
            CollectionRepository.GetCollection("Test.C2").InsertObject(c2);

            var c3 =
                new TransactionalObject<C3>(new C3("Test.C3", Guid.NewGuid())
                {
                    C1List = new ObjectList<C1>()
                });
            c3.Value.C1List.Add(c1_1);
            c3.Value.C1List.Add(c1_2);
            c3.Value.C1List.Add(c1_3);
            CollectionRepository.GetCollection("Test.C3").InsertObject(c3);

            var jsonC1 = JsonConvert.SerializeObject(c1_1,
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Serialize});
            var jsonC2 = JsonConvert.SerializeObject(c2,
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Serialize});
            var jsonC3 = JsonConvert.SerializeObject(c3,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Serialize });

            var dc1 = JsonConvert.DeserializeObject(jsonC1, typeof(TransactionalObject<C1>));
            var dc2 = JsonConvert.DeserializeObject(jsonC2, typeof(TransactionalObject<C2>)) as TransactionalObject<C2>;
            MessageBox.Show($"dc2.C1Reference.Reference.Value.I1: {dc2.Value.C1Reference.Reference.Value.I1}", "Reference");
            var dc3 = JsonConvert.DeserializeObject(jsonC3, typeof(TransactionalObject<C3>)) as TransactionalObject<C3>;
            //MessageBox.Show($"dc3.C1Reference.Reference.Value.I1: {dc3.Value.C1Reference.Reference.Value.I1}", "Reference");
            MessageBox.Show($"dc3.Value.C1List[0].Value.S1: {dc3.Value.C1List[0].Value.S1}", "List");
            dc2.Value.S2 = "done!";
        }

        private void TestList()
        {
            _list.AddRange(new List<Value>{new Value {IntValue = 1, StrValue = "First"}, new Value {IntValue = 2, StrValue = "Second"}});
            using (var ts = TransactionScopeUtil.GetTransactionScope())
            {
                _list[0].IntValue = -1;
                _list.Add(new Value {IntValue = 3, StrValue = "Third"});
            }
            MessageBox.Show($"I: {_list[0].IntValue}, S: {_list[0].StrValue}, Count: {_list.Count}");
        }

        private void TestTransaction()
        {
            _obj.Value.IntValue = 1;
            _obj.Value.StrValue = "ready to test!";
            _value.Value = 1;
            var thread2 = new Thread(Task2);
            thread2.Start();
            MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}", "Main");
            var thread1 = new Thread(Task1);
            thread1.Start();
            MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}", "Main");
            using (var ts = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = new TimeSpan(1, 0, 0, 0) }))
            {
                MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}", "Main");
                MessageBox.Show("Ready to write", "Main");
                _obj.Value.IntValue = 2;
                _obj.Value.StrValue = "test";
                MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}", "Main");
                ts.Complete();
            }
        }

        public void Task1()
        {
            using (var ts = new TransactionScope())
            {
                MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}", "Task 1");
                MessageBox.Show("Ready to write", "Task 1");
                _obj.Value.IntValue = 3;
                _obj.Value.StrValue = "serializable write!";
                MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}", "Task 1");
                ts.Complete();
            }
        }

        public void Task2()
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = new TimeSpan(2, 0, 0, 0) }))
            {
                MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}", "Task 2");
                MessageBox.Show("Ready to write", "Task 2");
                _obj.Value.IntValue = 4;
                _obj.Value.StrValue = "uncommitted write!";
                MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}", "Task 2");
                //ts.Complete();
            }
        }
    }

    [Serializable]
    public class Value
    {
        public int IntValue { get; set; }
        public string StrValue { get; set; }
    }

    public class TestCollection : ICollection
    {
        private readonly Dictionary<Guid,object> _objTable = new Dictionary<Guid, object>();

        public object GetObject(object id)
        {
            var guid = Guid.Empty;
            if (id is string) guid = new Guid(id as string);
            if (id is Guid) guid = (Guid) id;
            return _objTable.ContainsKey(guid) ? _objTable[guid] : null;
        }

        public void InsertObject(object obj)
        {
            dynamic o = obj;
            var value = o.Value;
            _objTable.Add((Guid)value.Id, obj);
        }

        public void UpdateObject(object obj)
        {
            dynamic o = obj;
            var value = o.Value;
            var id = (Guid) value.Id;
            if (_objTable.ContainsKey(id))
            _objTable[id] = obj;
        }

        public void DeleteObject(object obj)
        {
            throw new NotImplementedException();
        }

        public void RegisterReference(IReference reference)
        {
            throw new NotImplementedException();
        }
    }
}
