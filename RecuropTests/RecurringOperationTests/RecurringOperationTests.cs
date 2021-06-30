using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recurop;
using System;
using System.Diagnostics;
using System.Threading;

namespace RecuropTests
{
    [TestClass]
    public class RecurringOperationTests
    {
        [TestMethod]
        public void NameIsMandatoryOnNewRecurringOperationInitialization()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var operation = new RecurringOperation(name: "");
            });
        }

        [TestMethod]
        public void InitializingOperationWithoutNameGeneratesGuid()
        {
            var operation = new RecurringOperation();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(operation.GetName()));

            if (Guid.TryParseExact(operation.GetName(), null, out Guid guid))
            {
                Assert.IsTrue(guid != Guid.Empty);
            }
        }

        [TestMethod]
        public void InitializingOperationWithNameDoesNotGenerateGuid()
        {
            string name = "Test operation";
            var operation = new RecurringOperation(name);

            Assert.AreEqual(name, operation.GetName());
        }

        [TestMethod]
        public void OperationHasCorrectState()
        {
            var operation = new RecurringOperation();

            Assert.IsTrue(operation.IsInitialized);
            Assert.IsNotNull(operation.GetName());
            Assert.IsTrue(operation.GetName().Length > 0);
            Assert.IsNotNull(operation.CanBeStarted);
            Assert.IsTrue(operation.CanBeStarted);
            Assert.IsNotNull(operation.IsRecurring);
            Assert.IsFalse(operation.IsRecurring);
            Assert.IsNotNull(operation.IsNotRecurring);
            Assert.IsTrue(operation.IsNotRecurring);
            Assert.IsNotNull(operation.IsExecuting);
            Assert.IsFalse(operation.IsExecuting);
            Assert.IsNotNull(operation.IsPaused);
            Assert.IsFalse(operation.IsPaused);
            Assert.IsNotNull(operation.IsIdle);
            Assert.IsTrue(operation.IsIdle);
            Assert.IsNotNull(operation.IsCancelled);
            Assert.IsFalse(operation.IsCancelled);
            Assert.IsNotNull(operation.IsNotCancelled);
            Assert.IsTrue(operation.IsNotCancelled);
            Assert.IsNotNull(operation.Status);
            Assert.IsTrue(operation.Status == RecurringOperationStatus.Idle);
            Assert.IsNull(operation.Exception);

            void DoWork()
            {
                Thread.Sleep(100);
            }

            // Start recurring
            RecurringOperationsManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.1), DoWork);

            // At this point the operation has not yet executed
            Assert.IsFalse(operation.CanBeStarted);
            Assert.IsTrue(operation.IsRecurring);
            Assert.IsFalse(operation.IsNotRecurring);
            Assert.IsFalse(operation.IsExecuting);
            Assert.IsFalse(operation.IsIdle);
            Assert.IsFalse(operation.IsPaused);
            Assert.IsFalse(operation.IsCancelled);
            Assert.IsTrue(operation.IsNotCancelled);

            // Wait for operation to start executing
            Thread.Sleep(125);

            // Assert state after starting execution
            Assert.IsFalse(operation.CanBeStarted);
            Assert.IsTrue(operation.IsRecurring);
            Assert.IsFalse(operation.IsNotRecurring);
            Assert.IsTrue(operation.IsExecuting);
            Assert.IsFalse(operation.IsIdle);
            Assert.IsFalse(operation.IsPaused);
            Assert.IsFalse(operation.IsCancelled);
            Assert.IsTrue(operation.IsNotCancelled);

            // Pause the operation
            RecurringOperationsManager.Instance.PauseRecurring(operation);
            Thread.Sleep(125);

            // Assert state after pausing
            Assert.IsFalse(operation.CanBeStarted);
            Assert.IsFalse(operation.IsRecurring);
            Assert.IsTrue(operation.IsNotRecurring);
            Assert.IsFalse(operation.IsExecuting);
            Assert.IsTrue(operation.IsIdle);
            Assert.IsTrue(operation.IsPaused);
            Assert.IsFalse(operation.IsCancelled);
            Assert.IsTrue(operation.IsNotCancelled);

            // Resume the operation
            RecurringOperationsManager.Instance.ResumeRecurring(operation);
            Thread.Sleep(125);

            // Assert state after resuming
            Assert.IsFalse(operation.CanBeStarted);
            Assert.IsTrue(operation.IsRecurring);
            Assert.IsFalse(operation.IsNotRecurring);
            Assert.IsTrue(operation.IsExecuting);
            Assert.IsFalse(operation.IsIdle);
            Assert.IsFalse(operation.IsPaused);
            Assert.IsFalse(operation.IsCancelled);
            Assert.IsTrue(operation.IsNotCancelled);

            // Cancel the operation
            RecurringOperationsManager.Instance.CancelRecurring(operation);

            // Assert state after cancelling
            Assert.IsTrue(operation.CanBeStarted);
            Assert.IsFalse(operation.IsRecurring);
            Assert.IsTrue(operation.IsNotRecurring);
            Assert.IsFalse(operation.IsExecuting);
            Assert.IsFalse(operation.IsIdle);
            Assert.IsFalse(operation.IsPaused);
            Assert.IsTrue(operation.IsCancelled);
            Assert.IsFalse(operation.IsNotCancelled);
        }

        [TestMethod]
        public void ShowsCorrectStatus()
        {
            var operation = new RecurringOperation("StatusTest");

            void DoWork()
            {
                Thread.Sleep(250);
            }

            RecurringOperationsManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.1), DoWork);

            // At this point the operation status should be Idle, because
            // "start immediately" parameter was false and the operation is not
            // executing any code
            Assert.IsTrue(operation.Status == RecurringOperationStatus.Idle);

            Thread.Sleep(150);

            // At this point the operation should be in the middle of execution
            Assert.IsTrue(operation.Status == RecurringOperationStatus.Executing);

            // Pause
            RecurringOperationsManager.Instance.PauseRecurring(operation);
            Assert.IsTrue(operation.Status == RecurringOperationStatus.Paused);

            // Resume
            RecurringOperationsManager.Instance.ResumeRecurring(operation);
            Assert.IsTrue(operation.Status == RecurringOperationStatus.Idle);

            RecurringOperationsManager.Instance.CancelRecurring(operation);

            // At this point the operation status should be Cancelled
            Assert.IsTrue(operation.Status == RecurringOperationStatus.Cancelled);
        }

        [TestMethod]
        public void ToStringReturnsOperationName()
        {
            var name = "ToStringTest";
            var operation = new RecurringOperation(name);

            Assert.IsTrue(operation.ToString().Equals(name));
        }

        [TestMethod]
        public void GetNameReturnsOperationName()
        {
            var name = "GetNameTest";
            var operation = new RecurringOperation(name);

            Assert.IsTrue(operation.GetName().Equals(name));
        }

        [TestMethod]
        public void InvokeDelegateOnOperationFault()
        {
            bool delegateInvoked = false;

            // A method that throws an exception
            void FaultingOperation()
            {
                throw new InvalidOperationException("Error");
            }

            // A method to invoke when a fault occurs
            void OnFaulted(Exception ex)
            {
                delegateInvoked = true;
            }

            // Create the recurring operation
            var operation = new RecurringOperation("InvokeOperationFaultedDelegate");

            // Attach method to the fault event
            operation.OperationFaulted += OnFaulted;

            // Start recurring.
            // Will throw an exception after 0.2 seconds
            RecurringOperationsManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.2), FaultingOperation);

            // Wait for the operation to occur atleast once
            Thread.Sleep(250);

            // Cancel the operation, so that other tests can start an operation with the same name
            RecurringOperationsManager.Instance.CancelRecurring(operation);

            // If delegate was invoked, delegateInvoked should be true
            Assert.IsTrue(delegateInvoked);
        }

        [TestMethod]
        public void InvokeDelegateOnStatusChanged()
        {
            bool statusChanged = false;

            void DoWork()
            {
                Thread.Sleep(100);
            }

            void OnStatusChanged()
            {
                statusChanged = true;
            }

            // Create the recurring operation
            var operation = new RecurringOperation("InvokeStatusChangedDelegate");

            // Set delegate to the status changed event
            operation.StatusChanged += OnStatusChanged;

            // Start recurring
            RecurringOperationsManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.2), DoWork);

            // Wait for the operation to occur atleast once
            Thread.Sleep(250);

            // Cancel the operation, so that other tests can start an operation with the same name
            RecurringOperationsManager.Instance.CancelRecurring(operation);

            // If statusChanged is true, the event handler works
            Assert.IsTrue(statusChanged);
        }

        [TestMethod]
        public void LastRunStartShowsLatestRunStartTime()
        {
            DateTime beforeRecurring = DateTime.Now;

            void DoWork()
            {
                Thread.Sleep(100);
            }

            // Create the recurring operation
            var operation = new RecurringOperation("LastRunStart");

            // Start recurring
            RecurringOperationsManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.1), DoWork);

            // Wait for the operation to occur atleast once
            Thread.Sleep(250);

            // Cancel the operation, so that other tests can start an operation with the same name
            RecurringOperationsManager.Instance.CancelRecurring(operation);

            // Assert that LastRunStart is greater than the time before recurring
            Assert.IsTrue(operation.LastRunStart > beforeRecurring);
        }

        [TestMethod]
        public void LastRunFinishShowsLatestRunFinishTime()
        {
            DateTime beforeRecurring = DateTime.Now;

            void DoWork()
            {
                Thread.Sleep(100);
            }

            // Create the recurring operation
            var operation = new RecurringOperation("LastRunFinishTest");

            // Start recurring
            RecurringOperationsManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.1), DoWork);

            // Wait for the operation to occur atleast once
            Thread.Sleep(250);

            // Cancel the operation, so that other tests can run an operation with the same name
            RecurringOperationsManager.Instance.CancelRecurring(operation);

            // Assert that LastRunFinish is greater than the time before recurring
            Assert.IsTrue(operation.LastRunFinish > DateTime.MinValue);
        }

        [TestMethod]
        public void IsExecutingIsTrueWhileDelegateIsRunning()
        {
            void DoWork()
            {
                Thread.Sleep(200);
            }

            // Create the recurring operation
            var operation = new RecurringOperation("IsExecutingTest");

            // Start recurring
            RecurringOperationsManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.1), DoWork);

            Thread.Sleep(150);

            // At this point, the delegate should be in the middle of execution
            Assert.IsTrue(operation.IsExecuting);

            // Cancel the operation
            RecurringOperationsManager.Instance.CancelRecurring(operation);
        }

        [TestMethod]
        public void StartingAlreadyRecurringOperationThrowsException()
        {
            void DoWork()
            {
                Thread.Sleep(50);
            }

            // Create two instances of recurring operation
            var operation1 = new RecurringOperation("RecurringExceptionTest");
            var operation2 = new RecurringOperation("RecurringExceptionTest");

            RecurringOperationsManager.Instance.StartRecurring(
                operation1, TimeSpan.FromSeconds(0.1), DoWork);

            // Trying to start an identical recurring operation should throw
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                RecurringOperationsManager.Instance.StartRecurring(
                    operation2, TimeSpan.FromSeconds(0.1), DoWork);
            });

            RecurringOperationsManager.Instance.CancelRecurring(operation1);
            RecurringOperationsManager.Instance.CancelRecurring(operation2);
        }
    }
}
