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
using ObjectRepositoryImpl;
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
            TestInsertObj();
            TestLoadObj();
        }

        private void TestInsertObj()
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

        private void TestLoadObj()
        {
            var c1List = CollectionRepository.GetCollection(typeof(C1)).GetAllObjects(typeof(C1));
            var c2List = CollectionRepository.GetCollection(typeof(C2)).GetAllObjects(typeof(C2));
            var c3List = CollectionRepository.GetCollection(typeof(C3)).GetAllObjects(typeof(C3));
            var c2 = c2List[0] as TransactionalObject<C2>;
            var c3 = c3List[0] as TransactionalObject<C3>;
            MessageBox.Show($"c2.Value.S2:{c2.Value.S2}, c3.Value.C1List[0].Value.S1:{c3.Value.C1List[0].Value.S1}");
            MessageBox.Show("Done");
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

}
