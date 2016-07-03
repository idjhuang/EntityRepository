using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using ObjectResourceManager;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Test
{
    public partial class TestForm : Form
    {
        private readonly Transactional<Value> _obj = new Transactional<Value>(new Value()); 
        private Transactional<int> _value = new Transactional<int>(); 

        public TestForm()
        {
            InitializeComponent();
        }

        private void startBtn_Click(object sender, EventArgs e)
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
                new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted, Timeout = new TimeSpan(1,0,0,0)}))
            {
                MessageBox.Show($"Value: {_obj.Value.IntValue}, {_obj.Value.StrValue}" , "Main");
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
