using System;

public abstract class Task<T_Input, T_Output> where T_Input : ITaskIO where T_Output : ITaskIO {

    // Return a description of what the task does
    public abstract string GetDescription();

    // The task might need the output produced by a previous task, so it would take it before starting processing.
    public abstract void TakeInput(T_Input input);

    // This method will be called a few times per second, so that the task continues to process.
    // It is expected to return the progress of the task, a number in [0, 1].
    public abstract float ContinueProcessingAndReportProgress();

    // Return the result of the task, if any. This method should be called after the method above reports the
    // progress as 1.
    public abstract T_Output GetResult();

    public Type InputType { get { return typeof(T_Input); } }
    public Type OutputType { get { return typeof(T_Output); } }

}

public interface ITaskIO { }

