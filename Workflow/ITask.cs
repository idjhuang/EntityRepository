namespace Workflow
{
    public interface ITask
    {
        void Execute(WorkflowContext context, TaskExecutionRecord execRecord, object parameters);
    }
}
