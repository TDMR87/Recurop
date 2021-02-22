# Recurop
A .NET standard library for creating and managing recurring background operations.

What you can do with this library:
1) Runs a method in specific intervals (a.k.a recurring operation)
2) Stops, resumes, cancels the execution of the recurring operation
3) Bind UI elements to the status of the recurring operation. For example, disable a button when the operation is in the middle of execution.
4) Runs operations in the threadpool.

The operations run with this library will not queue up. That means if the execution of the specified method takes longer than the specified interval, then the next execution happens only after the long running operation has finished running.

Run the ConsoleClientCore console project for an example and test how the library works.

******************************

Initialize a named recurring operation object
```c#
var MyRecurringOperation = new RecurringOperation(name: "IncrementCounter");
```

Optionally, set callback methods for events
```c#
MyRecurringOperation.StatusChanged += OnStatusChanged;
MyRecurringOperation.OperationFaulted += OnOperationFaulted;
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

React to changes in the operation status using a callback methods.
```c#
static void OnStatusChanged()
{
    if (MyRecurringOperation.Status == RecurringOperationStatus.Aborted)
        Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has been aborted.");
        
    else if (MyRecurringOperation.Status == RecurringOperationStatus.Stopped)
        Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has stopped.");
}
```

React to faulted executions using a callback method, when the recurring operation throws an exception.
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
