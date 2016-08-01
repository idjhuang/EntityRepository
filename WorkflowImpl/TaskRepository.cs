using EntityRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using Workflow;

namespace WorkflowImpl
{
    public static class TaskRepository
    {
        private static readonly Dictionary<string, ITask> Tasks = new Dictionary<string, ITask>();

        public static void Init()
        {
            // explict init the CollectionRepository
            // init type repository
            TypeRepository.Init();
            // load all tasks
            var taskTypes = TypeRepository.LoadModules(typeof(ITask));
            foreach (var task in from taskType in taskTypes where taskType.IsClass select Activator.CreateInstance(taskType))
            {
                AddTask(task as ITask);
            }
        }

        public static IList<ITask> GetAllTasks()
        {
            return Tasks.Values.ToList();
        } 

        public static void AddTask(ITask task)
        {
            if (!Tasks.ContainsKey(task.GetType().FullName)) Tasks.Add(task.GetType().FullName, task);
        }

        public static ITask GetTask(string taskName)
        {
            // return default collection implementation when type not found in collection table
            return Tasks.ContainsKey(taskName) ? Tasks[taskName] : null;
        }

        public static void ExecuteTask(string taskName, WorkflowContext context, TaskExecutionRecord execRecord, object parameters)
        {
            if (!Tasks.ContainsKey(taskName)) throw new ArgumentException("Task not found.");
            Tasks[taskName].Execute(context, execRecord, parameters);
        }
    }
}
