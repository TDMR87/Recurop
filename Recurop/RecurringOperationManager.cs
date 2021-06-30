using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Recurop
{
    /// <summary>
    /// Background Operation Manager is responsible
    /// for handling the user defined Background Operation
    /// objects.
    /// </summary>
    public sealed class RecurringOperationsManager
    {
        /// <summary>
        /// The Recurring Operations Manager private singleton instance
        /// </summary>
        private static RecurringOperationsManager instance = null;
        private static readonly object instanceLock = new object();
        private readonly List<Operation> recurringOperations;

        /// <summary>
        /// Private constructor.
        /// </summary>
        private RecurringOperationsManager()
        {
            // Instansiate the internal collection of operations
            recurringOperations = new List<Operation>();
        }

        /// <summary>
        /// The Recurring Operations Manager private singleton instance
        /// </summary>
        public static RecurringOperationsManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null) instance = new RecurringOperationsManager();
                    return instance;
                }
            }
        }

        /// <summary>
        /// Resumes the execution of the specified recurring operation 
        /// if currently in Paused state. Resuming a cancelled operation
        /// throws an exception.
        /// </summary>
        /// <param name="recurringOperation"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="startImmediately"></param>
        public void ResumeRecurring(RecurringOperation recurringOperation, bool startImmediately = false)
        {
            // If operation is not initialized
            if (recurringOperation == null || string.IsNullOrWhiteSpace(recurringOperation.GetName()))
            {
                throw new InvalidOperationException(Constants.UninitializedOperationException);
            }

            // If cancelled
            if (recurringOperation.Status == RecurringOperationStatus.Cancelled)
            {
                var exception = new InvalidOperationException(Constants.CancelledOperationException);
                recurringOperation.Exception = exception;
                throw exception;
            }

            // Search for the operation reference
            var operation = recurringOperations.Find(op => op.Name.Equals(recurringOperation.GetName()));

            // If an operation was found
            if (operation != null)
            {
                // Set operation status
                recurringOperation.Status = RecurringOperationStatus.Idle;
                recurringOperation.IsIdle = true;
                recurringOperation.IsRecurring = true;
                recurringOperation.IsNotRecurring = false;
                recurringOperation.IsPaused = false;
                recurringOperation.IsExecuting = false;

                // Resume the operation
                operation.Timer.Change(
                    startImmediately ? TimeSpan.Zero : operation.Interval, operation.Interval);
            }
        }

        /// <summary>
        /// Creates and/or starts a recurring operation.
        /// If an operation has already been started,
        /// an exception will be thrown.
        /// </summary>
        /// <param name="recurringOperation"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <param name="startImmediately"></param>
        public void StartRecurring(RecurringOperation recurringOperation, TimeSpan interval, Action action = null, bool startImmediately = false)
        {
            if (recurringOperation == null) throw new InvalidOperationException(Constants.UninitializedOperationException);
            if (action != null && recurringOperation.Operation != null) throw new InvalidOperationException(Constants.ActionAlreadySpecifiedException);
            if (string.IsNullOrWhiteSpace(recurringOperation.GetName())) throw new InvalidOperationException(Constants.UnnamedOperationException);
            if (recurringOperations.Any(op => op.Name.Equals(recurringOperation.GetName()))) throw new InvalidOperationException($"{Constants.RecurringOperationAlreadyRunningException}. Operation name '{recurringOperation.GetName()}'.");

            // Create a new internal operation object
            var internalOperation = new Operation
            {
                Name = recurringOperation.GetName(),
                Interval = interval
            };

            // Create a callback function for the timer. This function is to be run in specified intervals.
            void TimerCallback(object state)
            {
                // To indicate if a thread has entered a block of code.
                bool lockAcquired = false;

                try
                {
                    // Try to acquire a lock.
                    // Sets the value of the lockAcquired, even if the method throws an exception, 
                    // so the value of the variable is a reliable way to test whether the lock has to be released.
                    Monitor.TryEnter(recurringOperation.CallbackLock, ref lockAcquired);

                    if (lockAcquired)
                    {
                        // Set recurring operation status
                        recurringOperation.LastRunStart = DateTime.Now;
                        recurringOperation.IsExecuting = true;
                        recurringOperation.IsIdle = false;
                        recurringOperation.Status = RecurringOperationStatus.Executing;

                        try
                        {
                            if (recurringOperation.Operation != null) recurringOperation.Operation?.Invoke();
                            else action?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            // Set the exception
                            recurringOperation.Exception = ex;
                        }
                        finally
                        {
                            // Set the time when the run finished
                            recurringOperation.LastRunFinish = DateTime.Now;

                            // Set recurring operation status flags
                            recurringOperation.IsExecuting = false;
                            recurringOperation.IsIdle = true;

                            // If not cancelled
                            if (recurringOperation.Status != RecurringOperationStatus.Cancelled)
                            {
                                // Set status to idle
                                recurringOperation.Status = RecurringOperationStatus.Idle;
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
                        Monitor.Exit(recurringOperation.CallbackLock);
                    }
                }
            } // End of local funtion

            // Create and set a timer for the internal operation.
            internalOperation.Timer = new Timer(
                new TimerCallback(TimerCallback), null, startImmediately ? TimeSpan.Zero : interval, interval);

            // Add the internal operation to the Manager's collection of operations
            recurringOperations.Add(internalOperation);

            // Set the status flags of the public facing recurring operation object
            recurringOperation.Status = RecurringOperationStatus.Idle;
            recurringOperation.IsRecurring = true;
            recurringOperation.IsNotRecurring = false;
            recurringOperation.IsPaused = false;
            recurringOperation.IsIdle = false;
            recurringOperation.CanBeStarted = false;
        }

        /// <summary>
        /// Pauses the recurring execution of the operation.
        /// Execution can be continued with a call to ResumeRecurring().
        /// </summary>
        /// <param name="recurringOperation"></param>
        public void PauseRecurring(RecurringOperation recurringOperation)
        {
            // Find a reference to an internal operation
            var internalOperation =
                recurringOperations.Find(
                    operation => operation.Name.Equals(recurringOperation.GetName()));

            // If found
            if (internalOperation != null)
            {
                // Pause the timer
                internalOperation.Timer.Change(Timeout.Infinite, Timeout.Infinite);

                // Set status flags of the public facing background operation object
                recurringOperation.Status = RecurringOperationStatus.Paused;
                recurringOperation.IsRecurring = false;
                recurringOperation.IsNotRecurring = true;
                recurringOperation.IsPaused = true;
                recurringOperation.IsIdle = true;
                recurringOperation.IsExecuting = false;
            }
        }

        /// <summary>
        /// Cancels the specified recurring operation. 
        /// Cancelled operations cannot be resumed, they must be restarted
        /// with a call to StartRecurring().
        /// </summary>
        /// <param name="backgroundOperation"></param>
        public void CancelRecurring(RecurringOperation backgroundOperation)
        {
            // Find a reference to an internal operation
            var internalOperation =
                recurringOperations.Find(
                    operation => operation.Name.Equals(backgroundOperation.GetName()));

            // If found
            if (internalOperation != null)
            {
                // Dispose the timer and the operation object
                internalOperation.Timer.Dispose();
                recurringOperations.Remove(internalOperation);

                // Set the status flags of the public facing background operation object
                backgroundOperation.Status = RecurringOperationStatus.Cancelled;
                backgroundOperation.IsRecurring = false;
                backgroundOperation.IsNotRecurring = true;
                backgroundOperation.IsPaused = false;
                backgroundOperation.IsExecuting = false;
                backgroundOperation.IsIdle = false;
                backgroundOperation.IsCancelled = true;
                backgroundOperation.IsNotCancelled = false;
                backgroundOperation.CanBeStarted = true;
            }
        }

        /// <summary>
        /// Private operation object.
        /// </summary>
        private class Operation
        {
            internal string Name { get; set; }
            internal Timer Timer { get; set; }
            internal TimeSpan Interval { get; set; }
        }
    }
}
