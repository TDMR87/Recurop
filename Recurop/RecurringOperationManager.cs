using System;
using System.Collections.Generic;
using System.Threading;

namespace Recurop
{
    /// <summary>
    /// Background Operation Manager is responsible
    /// for handling the user defined Background Operation
    /// objects.
    /// </summary>
    public sealed class RecurringOperationManager
    {
        private static RecurringOperationManager instance = null;
        private static readonly object instanceLock = new object();
        private readonly List<Operation> recurringOperations;

        /// <summary>
        /// Private singleton constructor.
        /// </summary>
        private RecurringOperationManager()
        {
            recurringOperations = new List<Operation>();
        }

        /// <summary>
        /// Public singleton instance.
        /// </summary>
        public static RecurringOperationManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null) instance = new RecurringOperationManager();
                    return instance;
                }
            }
        }

        /// <summary>
        /// Re-starts the specified background operation if it is
        /// not recurring and not aborted.
        /// </summary>
        /// <param name="backgroundOperation"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="startImmediately"></param>
        public void ResumeRecurring(RecurringOperation backgroundOperation, bool startImmediately = false)
        {
            // If an uninitialized background operation object was given as an argument
            if (backgroundOperation == null || string.IsNullOrWhiteSpace(backgroundOperation.GetName()))
                throw new InvalidOperationException(Constants.UninitializedOperationException);

            // If already a recurring background operation
            if (backgroundOperation.Status == RecurringOperationStatus.Recurring)
            {
                return;
            }

            // If aborted background operation
            else if (backgroundOperation.Status == RecurringOperationStatus.Aborted)
            {
                // Set value to the background operation's exception and return
                backgroundOperation.Exception = new InvalidOperationException(Constants.AbortedOperationException);
                return;
            }

            // Search the Manager's collection of operations for an operation
            // that corresponds with the background operation
            var recurringOperation =
                recurringOperations.Find(op => op.Name.Equals(backgroundOperation.GetName()));

            // If the operation was found and is not recurring
            if (recurringOperation != null && backgroundOperation.Status != RecurringOperationStatus.Recurring)
            {
                // Re-start the operation
                recurringOperation.Timer.Change(
                    startImmediately ? TimeSpan.Zero : recurringOperation.Interval, recurringOperation.Interval);

                // Set background operation status
                backgroundOperation.Status = RecurringOperationStatus.Recurring;
                backgroundOperation.IsRecurring = true;
                backgroundOperation.IsStopped = false;
            }
        }

        /// <summary>
        /// Creates a new recurring background operation and starts it.
        /// If the specified background operation already exists, the
        /// Manager will ignore this call. Trying to start an aborted background
        /// operation will throw an exception.
        /// </summary>
        /// <param name="backgroundOperation"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="startImmediately"></param>
        public void StartRecurring(RecurringOperation backgroundOperation, TimeSpan interval, Action action, bool startImmediately = false)
        {
            // If an uninitialized background operation object was given as an argument
            if (backgroundOperation == null || string.IsNullOrWhiteSpace(backgroundOperation.GetName()))
                throw new InvalidOperationException(Constants.UninitializedOperationException);

            // Search the Manager's collection of operations for an operation
            // that corresponds with the background operation
            var recurringOperation =
                recurringOperations.Find(op => op.Name.Equals(backgroundOperation.GetName()));

            // If no operation exists yet
            if (recurringOperation == null)
            {
                // Create a new recurring operation object
                var operation = new Operation
                {
                    Name = backgroundOperation.GetName(),
                    Interval = interval
                };

                // This is a local function here..
                // ..Create a callback function for the timer
                void TimerCallback(object state)
                {
                    // To indicate if a thread has entered 
                    // a block of code.
                    bool lockAcquired = false;

                    try
                    {
                        // Try to acquire a lock.
                        // Sets the value of the lockAcquired, even if the method throws an exception, 
                        // so the value of the variable is a reliable way to test whether the lock has to be released.
                        Monitor.TryEnter(backgroundOperation.CallbackLock, ref lockAcquired);

                        // If lock acquired
                        if (lockAcquired)
                        {
                            // Set background operation status
                            backgroundOperation.LastRunStart = DateTime.Now;
                            backgroundOperation.IsExecuting = true;
                            backgroundOperation.Status = RecurringOperationStatus.Executing;

                            try
                            {
                                action();
                            }
                            catch (Exception ex)
                            {
                                // Set reference to the catched exception in the background operations exception property
                                backgroundOperation.Exception = ex;
                            }
                            finally
                            {
                                // Set background operation status
                                backgroundOperation.IsExecuting = false;
                                backgroundOperation.Status = RecurringOperationStatus.Recurring;
                                backgroundOperation.LastRunFinish = DateTime.Now;
                            }
                        }
                    }
                    finally
                    {
                        // If a thread acquired the lock
                        if (lockAcquired)
                        {
                            // Release the lock
                            Monitor.Exit(backgroundOperation.CallbackLock);
                        }
                    }
                } // End local funtion

                // Create a timer that calls the action in specified intervals
                // and save the timer in the recurring operations Timer property
                operation.Timer = new Timer(
                    new TimerCallback(TimerCallback), null, startImmediately ? TimeSpan.Zero : interval, interval);

                // Add the recurring operation to the Manager's collection
                recurringOperations.Add(operation);

                // Set status of the client-side background operation
                backgroundOperation.Status = RecurringOperationStatus.Recurring;
                backgroundOperation.IsRecurring = true;
                backgroundOperation.IsStopped = false;
            }
        }

        /// <summary>
        /// Stops the recurring execution of the background operation.
        /// Can be restarted with a call to ResumeRecurring().
        /// </summary>
        /// <param name="backgroundOperation"></param>
        public void StopRecurring(RecurringOperation backgroundOperation)
        {
            // Search the Manager's collection of operations for an operation
            // that corresponds with the background operation
            var privateOperation =
                recurringOperations.Find(
                    operation => operation.Name.Equals(backgroundOperation.GetName()));

            // If found
            if (privateOperation != null)
            {
                // Pause the timer
                privateOperation.Timer.Change(Timeout.Infinite, Timeout.Infinite);

                // Set the status of the client-side background operation
                backgroundOperation.Status = RecurringOperationStatus.Stopped;
                backgroundOperation.IsRecurring = false;
                backgroundOperation.IsStopped = true;
            }
        }

        /// <summary>
        /// Aborts the specified background operation. 
        /// Aborted background operations cannot be restarted.
        /// </summary>
        /// <param name="backgroundOperation"></param>
        public void Abort(RecurringOperation backgroundOperation)
        {
            // Search the Manager's collection of operations for an operation
            // that corresponds with the background operation
            var privateOperation =
                recurringOperations.Find(
                    operation => operation.Name.Equals(backgroundOperation.GetName()));

            // If found
            if (privateOperation != null)
            {
                // Dispose the timer and the operation object
                privateOperation.Timer.Dispose();
                recurringOperations.Remove(privateOperation);

                // Set the status of the client-side background operation
                backgroundOperation.Status = RecurringOperationStatus.Aborted;
                backgroundOperation.IsRecurring = false;
                backgroundOperation.IsStopped = true;
                backgroundOperation.IsExecuting = false;
            }
        }

        /// <summary>
        /// Private operation data structure.
        /// </summary>
        private class Operation
        {
            internal string Name { get; set; }
            internal Timer Timer { get; set; }
            internal TimeSpan Interval { get; set; }
        }
    }
}
