using System;
using EntityRepository;
using System.Collections.Generic;
using System.Linq;
using Workflow;

namespace WorkflowImpl
{
    public static class WorkflowRepository
    {
        private static IList<TransactionalEntity<WorkflowContext>> _workflows;
        private static ICollection _workflowCollection;

        public static void Init()
        {
            // get collection of workflow
            _workflowCollection = CollectionRepository.GetCollection(typeof (WorkflowContext));
            // reterive all workflow contexts
            _workflows = _workflowCollection.GetAllEntities(typeof(WorkflowContext)).Select(w => w as TransactionalEntity<WorkflowContext>).ToList();
            // process aborted workflow contexts
            var abortedWorkflows = _workflows.Where(w => w.GetEntity().Status == WorkflowStatus.Abort);
            foreach (var abortedWorkflow in abortedWorkflows)
            {
                Abort(abortedWorkflow);
            }
            // process done workflows
            var doneWorkflows = _workflows.Where(w => w.GetEntity().Status == WorkflowStatus.Done);
            foreach (var doneWorkflow in doneWorkflows)
            {
                Done(doneWorkflow);
            }
            // process timeout workflows
            ProcessTimeout();
        }

        public static void ProcessTimeout()
        {
            // retrieve timeout workflows
            var workflowEntities = _workflows.Select(w => w.GetEntity()).ToList();
            var timeoutWorkflows =
                workflowEntities.Where(
                    w =>
                        w.Deadline.CompareTo(DateTime.Now) < 0 && w.Status != WorkflowStatus.Abort &&
                        w.Status != WorkflowStatus.Done).ToList();
            // abort timeout workflows
            foreach (var workflow in timeoutWorkflows)
            {
                Abort((Guid) workflow.Id);
            }
        }

        public static IList<TransactionalEntity<WorkflowContext>> GetAll()
        {
            return _workflows;
        }

        public static IList<TransactionalEntity<WorkflowContext>> GetByUser(string user)
        {
            return _workflows.Where(w => w.GetEntity().User.Equals(user)) as IList<TransactionalEntity<WorkflowContext>>;
        }

        public static TransactionalEntity<WorkflowContext> Create(string user, string name)
        {
            var contextEntity = new WorkflowContext(Guid.NewGuid(), user, name);
            var context = new TransactionalEntity<WorkflowContext>(contextEntity);
            context.Update();
            _workflows.Add(context);
            return context;
        }

        public static void Execute(string taskName, Guid workflowId, object parameters)
        {
            var context = _workflowCollection.GetEntity(workflowId) as TransactionalEntity<WorkflowContext>;
            if (context == null) throw new ArgumentException($"Workflow ({workflowId}) not found.");
            Execute(taskName, context, parameters);
        }

        public static void Execute(string taskName, TransactionalEntity<WorkflowContext> context, object parameters)
        {
            var task = TaskRepository.GetTask(taskName);
            if (task == null) throw new ArgumentException($"Task ({taskName}) not found.");
            var contextEntity = context.GetEntity();
            if (contextEntity.Status != WorkflowStatus.Ready) throw new ArgumentException($"Workflow ({contextEntity.Id}) not ready.");
            var deadline = contextEntity.Deadline;
            var execRecord = new TaskExecutionRecord
            {
                Task = taskName,
                Parameters = parameters
            };
            TaskRepository.ExecuteTask(taskName, contextEntity, execRecord, parameters);
            // if deadline has not updated, then add 10 mins by default.
            if (DateTime.Compare(deadline, contextEntity.Deadline) == 0)
            {
                contextEntity.Deadline = deadline.Add(new TimeSpan(0, 10, 0));
            }
            context.Update(contextEntity);
            switch (contextEntity.Status)
            {
                case WorkflowStatus.Abort:
                    Abort(context);
                    return;
                case WorkflowStatus.Done:
                    Done(context);
                    return;
                case WorkflowStatus.Ready:
                    contextEntity.ExecutionRecords.Push(execRecord);
                    break;
                case WorkflowStatus.Failed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            context.Update(contextEntity);
        }

        public static void Done(Guid workflowId)
        {
            var context = _workflowCollection.GetEntity(workflowId) as TransactionalEntity<WorkflowContext>;
            if (context == null) throw new ArgumentException($"Workflow ({workflowId}) not found.");
            var contextEntity = context.GetEntity();
            contextEntity.Status = WorkflowStatus.Done;
            context.Update(contextEntity);
            Done(context);
        }

        private static void Done(TransactionalEntity<WorkflowContext> context)
        {
            var contextEntity = context.GetEntity();
            contextEntity.Deadline = contextEntity.Deadline.AddHours(1);
            context.Update(contextEntity);
            _workflows.Remove(context);
            _workflowCollection.DeleteEntity(context);
        }

        public static void Abort(Guid workflowId)
        {
            var context = _workflowCollection.GetEntity(workflowId) as TransactionalEntity<WorkflowContext>;
            if (context == null) throw new ArgumentException($"Workflow ({workflowId}) not found.");
            var contextEntity = context.GetEntity();
            contextEntity.Status = WorkflowStatus.Abort;
            context.Update(contextEntity);
            Abort(context);
        }

        private static void Abort(TransactionalEntity<WorkflowContext> context)
        {
            var contextEntity = context.GetEntity();
            contextEntity.Deadline = contextEntity.Deadline.AddHours(1);
            context.Update(contextEntity);
            var execRecords = contextEntity.ExecutionRecords;
            while (execRecords.Count > 0)
            {
                var execRecord = execRecords.Peek();
                TaskRepository.ExecuteTask(execRecord.Task, contextEntity, execRecord, execRecord.Parameters);
                execRecords.Pop();
                context.Update(contextEntity);
            }
            Done(context);
        }
    }
}
