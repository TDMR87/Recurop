# Recurop
A .NET standard library for creating and managing recurring background operations.

Run the ConsoleClientCore console project for an example of how the library works.

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
