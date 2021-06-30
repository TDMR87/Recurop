namespace Recurop
{
    public static class Constants
    {
        public const string CancelledOperationException = "Cannot resume a cancelled recurring operation. " +
                                                          "Try restarting the operation by calling StartRecurring().";
        
        public const string RecurringOperationAlreadyRunningException = "Cannot start an already running recurring operation";

        public const string UninitializedOperationException = "The recurring operation is uninitialized.";

        public const string UnnamedOperationException = "The recurring operation cannot be initialized with an empty name.";

        public const string ActionAlreadySpecifiedException = "Attempted to set an Action to a recurring operation, " +
                                                                 "but the specified recurring operation has already been assigned an Action " +
                                                                 "through it's 'Operation' member. Try calling this method without specifying " +
                                                                 "an Action, or create a new Recurring operation object to specify a new Action.";
    }
}
