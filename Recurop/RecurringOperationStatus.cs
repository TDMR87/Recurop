namespace Recurop
{
    public enum RecurringOperationStatus
    {
        Idle, // Recurring, but not executing code
        Paused, // Paused, but can be resumed
        Executing, // Exzecuting code
        Cancelled // Permanently stopped, cant be resumed
    }
}
