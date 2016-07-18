using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
using Test;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace TestWinForms
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
            CollectionRepository.Init();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            var testCase = this.testCase.SelectedItem as string;
            switch (testCase)
            {
                case "Insert":
                    TestCases.InsertObjects();
                    break;
                case "Load":
                    TestCases.LoadObjects();
                    break;
                case "Extension":
                    TestCases.InsertAndLoadExtensionObjects();
                    break;
            }
        }

/*
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
*/
    }
}
