using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using EntityRepository;
using Test;
using WorkflowImpl;
using Workflow;

namespace TestWinForms
{
    public partial class WorkflowForm : Form
    {
        private readonly BindingList<WorkflowContext> _workflowContexts; 
        public WorkflowForm()
        {
            InitializeComponent();
            _workflowContexts = new BindingList<WorkflowContext>(WorkflowRepository.GetAll().Select(w => w.GetEntity()).ToList());
            workflowList.DataSource = _workflowContexts;
            workflowList.DisplayMember = "Name";
            var tasks = TaskRepository.GetAllTasks().Select(task => task as InvokeTask).ToList();
            taskList.DataSource = tasks;
            taskList.DisplayMember = "Name";
        }

        private void execBtn_Click(object sender, EventArgs e)
        {
            var workflowContext = workflowList.SelectedItem as WorkflowContext;
            var task = taskList.SelectedItem as InvokeTask;
            var parameters = parameter.Text.Split(',');
            task.Invoke((Guid) workflowContext.Id, parameters);
            var workflow = CollectionRepository.GetCollection(typeof (WorkflowContext)).GetEntity(workflowContext.Id) as TransactionalEntity<WorkflowContext>;
            workflowContext = workflow.GetEntity();
            if (workflowContext.Status == WorkflowStatus.Failed)
            {
                MessageBox.Show(workflowContext.Message, "Workflow");
                workflowContext.Status = WorkflowStatus.Ready;
                workflow.Update(workflowContext);
            }
        }

        private void revokeBtn_Click(object sender, EventArgs e)
        {
            var workflow = workflowList.SelectedItem as WorkflowContext;
            WorkflowRepository.Revoke((Guid)workflow.Id);
        }

        private void doneBtn_Click(object sender, EventArgs e)
        {
            var workflow = workflowList.SelectedItem as WorkflowContext;
            WorkflowRepository.Done((Guid)workflow.Id);
            _workflowContexts.Remove(workflow);
        }

        private void abortBtn_Click(object sender, EventArgs e)
        {
            var workflow = workflowList.SelectedItem as WorkflowContext;
            WorkflowRepository.Abort((Guid) workflow.Id);
            _workflowContexts.Remove(workflow);
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            var workflow = WorkflowRepository.Create("Test", workflowName.Text);
            _workflowContexts.Add(workflow.GetEntity());
        }

        private void workflowTimeout_Tick(object sender, EventArgs e)
        {
            WorkflowRepository.ProcessTimeout();
        }
    }
}
