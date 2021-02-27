using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Recurop
{
    /// <summary>
    /// Background Operation Manager is responsible
    /// for handling the user defined Background Operation
    /// objects.
    /// </summary>
    public sealed class RecurringOperations
    {
        private static RecurringOperations instance = null;
        private static readonly object instanceLock = new object();
        private readonly List<Operation> recurringOperations;

        /// <summary>
        /// Private singleton constructor.
        /// </summary>
        private RecurringOperations()
        {
            recurringOperations = new List<Operation>();
        }

        /// <summary>
        /// Public singleton instance.
        /// </summary>
        public static RecurringOperations Manager
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null) instance = new RecurringOperations();
                    return instance;
                }
            }
        }

        /// <summary>
        /// Resumes the execution of the specified recurring operation 
        /// if currently in Paused state. Resuming a cancelled operation
        /// throws an exception.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="startImmediately"></param>
        public void ResumeRecurring(RecurringOperation operation, bool startImmediately = false)
        {
            // If an uninitialized background operation object was given as an argument
            if (operation == null || string.IsNullOrWhiteSpace(operation.GetName()))
                throw new InvalidOperationException(Constants.UninitializedOperationException);

            // If cancelled
            if (operation.Status == RecurringOperationStatus.Cancelled)
            {
                var exception = new InvalidOperationException(Constants.CancelledOperationException);

                operation.Exception = exception;

                throw exception;
            }

            // Search the Manager's collection of operations for an operation
            var recurringOperation =
                recurringOperations.Find(op => op.Name.Equals(operation.GetName()));

            // If the operation was found and is not recurring
            if (recurringOperation != null)
            {
                // Set background operation status
                operation.Status = RecurringOperationStatus.Idle;
                operation.IsIdle = true;
                operation.IsRecurring = true;
                operation.IsNotRecurring = false;
                operation.IsPaused = false;
                operation.IsExecuting = false;

                // Re-start the operation
                recurringOperation.Timer.Change(
                    startImmediately ? TimeSpan.Zero : recurringOperation.Interval, recurringOperation.Interval);
            }
        }

        /// <summary>
        /// Creates and/or starts a recurring operation.
        /// If an operation has already been started,
        /// an exception will be thrown.
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

            // If operation has already been registered
            if (recurringOperation != null)
            {
                throw new InvalidOperationException($"{Constants.RecurringOperationException}. " +
                                                    $"Operation name '{recurringOperation.Name}'.");
            }

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
                        backgroundOperation.IsIdle = false;
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
                            // Set last run finish time
                            backgroundOperation.LastRunFinish = DateTime.Now;

                            // Set background operation state
                            backgroundOperation.IsExecuting = false;
                            backgroundOperation.IsIdle = true;

                            // If not cancelled
                            if (backgroundOperation.Status != RecurringOperationStatus.Cancelled)
                            {
                                // Set status to idle
                                backgroundOperation.Status = RecurringOperationStatus.Idle;
                            }
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
            backgroundOperation.Status = RecurringOperationStatus.Idle;
            backgroundOperation.IsRecurring = true;
            backgroundOperation.IsNotRecurring = false;
            backgroundOperation.IsPaused = false;
            backgroundOperation.IsIdle = false;
        }

        /// <summary>
        /// Pauses the recurring execution of the operation.
        /// Execution can be continued with a call to ResumeRecurring().
        /// </summary>
        /// <param name="backgroundOperation"></param>
        public void PauseRecurring(RecurringOperation backgroundOperation)
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
                backgroundOperation.Status = RecurringOperationStatus.Idle;
                backgroundOperation.IsRecurring = false;
                backgroundOperation.IsNotRecurring = true;
                backgroundOperation.IsPaused = true;
                backgroundOperation.IsIdle = true;
                backgroundOperation.IsExecuting = false;
            }
        }

        /// <summary>
        /// Cancels the specified recurring operation. 
        /// Cancelled operations cannot be resumed, they must be restarted
        /// with a call to StartRecurring.
        /// </summary>
        /// <param name="backgroundOperation"></param>
        public void Cancel(RecurringOperation backgroundOperation)
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
                backgroundOperation.Status = RecurringOperationStatus.Cancelled;
                backgroundOperation.IsRecurring = false;
                backgroundOperation.IsNotRecurring = true;
                backgroundOperation.IsPaused = false;
                backgroundOperation.IsExecuting = false;
                backgroundOperation.IsIdle = true;
                backgroundOperation.IsCancelled = true;
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
