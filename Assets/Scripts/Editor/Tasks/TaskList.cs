using System;
using System.Collections.Generic;

public class TaskList {

    private List<dynamic> tasks;

    public float Count { get; private set; }

    private TaskList() {
        this.tasks = new List<dynamic>();
        this.Count = 0;
    }

    public TaskList With<T_Input, T_Output>(Task<T_Input, T_Output> nextTask) where T_Input : ITaskIO where T_Output : ITaskIO {
        if (this.Count > 0)
            Assert(nextTask.InputType != tasks[tasks.Count - 1].OutputType, "The input type of the new task must match the output type of the last task already in the list");
        tasks.Add(nextTask);
        Count++;
        return this;
    }

    public dynamic GetTask(int taskIndex) {
        return tasks[taskIndex];
    }

    public static TaskList From<T_Input, T_Output>(Task<T_Input, T_Output> firstTask) where T_Input : ITaskIO where T_Output : ITaskIO{
        TaskList newTaskList = new TaskList();
        newTaskList.With(firstTask);
        return newTaskList;
    }

    private static void Assert(bool expression, string message) {
        if (expression) {
            throw new Exception(message);
        }
    }

}