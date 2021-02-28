# Recurop
What is Recurop?

A .NET standard library for creating, managing and monitoring recurring operations easily. For example: make your method execute in certain intervals.

Have you ever coded timer-based methods that execute in certain intervals? Recurop will handle that for you and make sure that running those operations happen in a controlled manner.

- Easily stop, resume or cancel the execution of a recurring operation.
- Monitor and react to changes in the status of the recurring operation (running, executing, paused, cancelled etc).
- Bind XAML elements to the status of the recurring operation. For example, disable a button when the operation is in the middle of execution.
- React to events that happen inside the recurring operation, for example if the operation throws an exception in the middle of execution.

Also:
- Runs the recurring operations in the threadpool.
- Thread safe. Multiple threads cannot execute the operation and alter state in parallel.
- No unintended operation queueing if the method execution takes longer than the specified interval.

******************************

Initialize a named recurring operation object
```c#
var MyRecurringOperation = new RecurringOperation(name: "IncrementCounter");
```

Start recurring operations. In this example, the method CountToMaxInt will be executed every 2 seconds.
```c#
RecurringOperationManager.Instance.StartRecurring(
  MyRecurringOperation, TimeSpan.FromSeconds(2), CountToMaxInt);
```

MyRecurringOperation object now represents the state of the recurring operation. For example, you can poll the status of the operation.
```c#
while (MyRecurringOperation.Status != RecurringOperationStatus.Aborted)
{
    // Get input from user
    GetUserInput();
}
```

***

The static BackgroundOperationManager singleton class provides mechanisms for controlling  and managing the background operations.

Stopping the recurring operation:
```c#
RecurringOperationManager.Instance.StopRecurring(MyRecurringOperation);
```

Resuming a stopped recurring operation:
```c#
RecurringOperationManager.Instance.ResumeRecurring(MyRecurringOperation);
```

Aborting a recurring operation (cannot be resumed):
```c#
RecurringOperationManager.Instance.Abort(MyRecurringOperation);
```

***

Optionally, set callback methods for events
```c#
MyRecurringOperation.StatusChanged += OnStatusChanged;
MyRecurringOperation.OperationFaulted += OnOperationFaulted;
```

React to changes in the operation status.
```c#
static void OnStatusChanged()
{
    if (MyRecurringOperation.Status == RecurringOperationStatus.Aborted)
        Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has been aborted.");
        
    else if (MyRecurringOperation.Status == RecurringOperationStatus.Stopped)
        Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has stopped.");
}
```

React to faulted executions.
```c#
static void OnOperationFaulted(Exception ex)
{
    Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has faulted: " +
                      $"{MyRecurringOperation.Exception?.Message}");
}
```

Use the recurring operation object's public properties with MVVM and property binding to dynamically show changes in the recurring operations status in your applications UI.
```xaml
<Button Text="Start" IsEnabled="{Binding MyBackgroundOperation.IsStopped}"/>

<Button Text="Pause" IsEnabled="{Binding MyBackgroundOperation.IsExecuting}" />

<Button Text="Cancel" IsEnabled="{Binding MyBackgroundOperation.IsRecurring}" />
```
