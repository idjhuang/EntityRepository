using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EntityRepository;
using Test;
using Workflow;
using WorkflowImpl;

namespace TestWinForms
{
    public partial class TestForm : Form
    {
        private IList<TransactionalEntity<WorkflowContext>> _workflows;
        public TestForm()
        {
            InitializeComponent();
            CollectionRepository.Init();
            TaskRepository.Init();
            WorkflowRepository.Init();
            TestWorkflow.Init(Properties.Settings.Default.EntityRepositoryConnStr);
            TestWorkflow.InitData();
            _workflows = WorkflowRepository.GetAll();
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
                case "Transaction":
                    TestCases.Transaction(Properties.Settings.Default.EntityRepositoryConnStr);
                    break;
                case "MultiTask":
                    TestCases.MultiTask(Properties.Settings.Default.EntityRepositoryConnStr);
                    break;
                case "School-Create":
                    SchoolModels.BusinessMethods.CreateEntities();
                    break;
                case "School-Load":
                    SchoolModels.BusinessMethods.LoadEntities();
                    break;
                case "Workflow":
                    var form = new WorkflowForm();
                    form.ShowDialog();
                    break;
            }
        }
    }
}
