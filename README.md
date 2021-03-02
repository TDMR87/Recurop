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

Available on NuGet: https://www.nuget.org/packages/Recurop/

******************************
Code examples:

Initialize a named recurring operation
```c#
var MyRecurringOperation = new RecurringOperation(name: "MyRecurringOperation");
```

Use the Manager class to start the recurring operation. In this example, the recurring operation will write "Hello world!" to the console every 5 seconds.
```c#
RecurringOperations.Manager.StartRecurring(
  MyRecurringOperation, TimeSpan.FromSeconds(5), () => Console.WriteLine("Hello world!"));
  
//
// Or, instead of a lambda expression, you can use an existing method
//

RecurringOperations.Manager.StartRecurring(
  MyRecurringOperation, TimeSpan.FromSeconds(5), PrintHelloWorld);
```

MyRecurringOperation object now represents the state of the recurring operation. For example, you can poll the status of the operation.
```c#
while (MyRecurringOperation.Status != RecurringOperationStatus.Cancelled)
{
    // Do something
}
```

***

The RecurringOperations.Manager singleton class provides mechanisms for controlling the recurring operation.

Pause the recurring operation:
```c#
RecurringOperations.Manager.PauseRecurring(MyRecurringOperation);
```

Resume a paused recurring operation:
```c#
RecurringOperations.Manager.ResumeRecurring(MyRecurringOperation);
```

Cancel a recurring operation (cannot be resumed):
```c#
RecurringOperations.Manager.Cancel(MyRecurringOperation);
```

***

Optionally, you can set callback methods to react to events.

React to changes in the operation status.
```c#
MyRecurringOperation.StatusChanged += OnStatusChanged;

static void OnStatusChanged()
{
    if (MyRecurringOperation.Status == RecurringOperationStatus.Cancelled)
        Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has been cancelled.");
        
    else if (MyRecurringOperation.Status == RecurringOperationStatus.Paused)
        Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has been paused.");
}
```

React to exceptions in the operation execution.
```c#
MyRecurringOperation.OperationFaulted += OnOperationFaulted;

static void OnOperationFaulted(Exception ex)
{
    Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has faulted: " +
                      $"{MyRecurringOperation.Exception?.Message}");
}
```

Get a long running recurring operation's last start and finishing time.
```c#
DateTime lastRunStart = MyRecurringOperation.LastRunStart;

DateTime lastRunFinish = MyRecurringOperation.LastRunFinish;
```

Bind XAML control properties to the bindable properties of the recurring operation
```xaml
<Button Text="Start" IsEnabled="{Binding MyBackgroundOperation.CanBeStarted}"/>

<Button Text="Pause" IsEnabled="{Binding MyBackgroundOperation.IsExecuting}" />

<Button Text="Continue" IsEnabled="{Binding MyBackgroundOperation.IsPaused}" />

<Button Text="Cancel" IsEnabled="{Binding MyBackgroundOperation.IsRecurring}" />
```
