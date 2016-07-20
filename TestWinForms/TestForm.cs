using System;
using System.Windows.Forms;
using EntityRepositoryContract;
using Test;

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
                case "Transaction":
                    TestCases.Transaction(Properties.Settings.Default.EntityRepositoryConnStr);
                    break;
                case "MultiTask":
                    TestCases.MultiTask(Properties.Settings.Default.EntityRepositoryConnStr);
                    break;
            }
        }
    }
}
