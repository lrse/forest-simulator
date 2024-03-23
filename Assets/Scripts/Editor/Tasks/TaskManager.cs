public class TaskManager {

    public bool Processing { get; private set; }
    public float Progress { get; private set; }
    public string Description {
        get {
            if (currentTask < taskList.Count) {
                return taskList.GetTask(currentTask).GetDescription();
            } else {
                return "Finished!";
            }
        }
    }

    private int currentTask;
    private TaskList taskList;

    public void Run(TaskList taskList) {
        this.Progress = 0;
        this.Processing = true;
        this.taskList = taskList;
        this.currentTask = 0;
    }

    public void ContinueIfProcessing() {
        if (!Processing || Progress == 1) return;

        dynamic task = taskList.GetTask(currentTask);
        float taskProgress = task.ContinueProcessingAndReportProgress();
        this.Progress = (currentTask + taskProgress) / (float)taskList.Count;
        if (taskProgress == 1) {
            if (currentTask < taskList.Count - 1) {
                dynamic currentTaskOutput = task.GetResult();
                taskList.GetTask(currentTask + 1).TakeInput(currentTaskOutput);
            }
            currentTask++;
        }
    }

    public void Stop() {
        this.Progress = 0;
        this.Processing = false;
    }
}
