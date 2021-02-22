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

Start the recurring operation. In this example, the method CountToMaxInt will be executed every 2 seconds.
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

Through the BackgroundOperationManager singleton class you can control and manage MyBackgroundOperation.

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
