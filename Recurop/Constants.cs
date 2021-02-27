namespace Recurop
{
    public static class Constants
    {
        public const string CancelledOperationException = "Cannot resume a cancelled background operation. Try restarting the operation.";
        public const string RecurringOperationException = "Cannot start an already running recurring operation";
        public const string UninitializedOperationException = "The background operation is uninitialized.";
        public const string UnnamedOperationException = "The background operation cannot be initialized with an empty name.";
    }
}

