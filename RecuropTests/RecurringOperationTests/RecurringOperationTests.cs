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
        public void PublicPropertiesCorrectlyInitializedOnOperationInitialization()
        {
            var operation = new RecurringOperation(name: "Test");

            Assert.IsNotNull(operation.GetName());
            Assert.IsTrue(operation.GetName().Length > 0);
            Assert.IsNotNull(operation.IsRecurring);
            Assert.IsFalse(operation.IsRecurring);
            Assert.IsNotNull(operation.IsExecuting);
            Assert.IsFalse(operation.IsExecuting);
            Assert.IsNotNull(operation.IsStopped);
            Assert.IsTrue(operation.IsStopped);
            Assert.IsNotNull(operation.Status);
            Assert.IsTrue(operation.Status == RecurringOperationStatus.Stopped);
            Assert.IsNull(operation.Exception);
        }

        [TestMethod]
        public void ToStringReturnsRecurringOperationName()
        {
            var name = "Test";
            var operation = new RecurringOperation(name);

            Assert.IsTrue(operation.ToString().Equals(name));
        }

        [TestMethod]
        public void GetNameReturnsRecurringOperationName()
        {
            var name = "Test";
            var operation = new RecurringOperation(name);

            Assert.IsTrue(operation.GetName().Equals(name));
        }

        [TestMethod]
        public void InvokeDelegateOnOperationFault()
        {
            bool delegateInvoked = false;

            // A recurring action that throws an exception (faults)
            void FaultingOperation()
            {
                throw new InvalidOperationException("Error");
            }

            // The method to invoke when a fault occurs
            void OnFaulted(Exception ex)
            {
                delegateInvoked = true;
            }

            // Create the recurring operation
            var operation = new RecurringOperation("Test");

            // Attach method to the fault event
            operation.OperationFaulted += OnFaulted;

            // Start recurring, will throw an exception after 0.2 second
            RecurringOperationManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.2), FaultingOperation);

            // Wait for the operation to occur atleast once
            Thread.Sleep(250);

            // Abort the operation, so that other tests can run an operation with the same name
            RecurringOperationManager.Instance.Abort(operation);

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
            var operation = new RecurringOperation("Test");
            operation.StatusChanged += OnStatusChanged;

            // Start recurring
            RecurringOperationManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.2), DoWork);

            // Wait for the operation to occur atleast once
            Thread.Sleep(250);

            // Abort the operation, so that other tests can run an operation with the same name
            RecurringOperationManager.Instance.Abort(operation);

            // If statusChanged is true, the event handler works
            Assert.IsTrue(statusChanged);
        }

        [TestMethod]
        public void LastRunStartHasLatestRunStartTime()
        {
            DateTime beforeRecurring = DateTime.Now;

            void DoWork()
            {
                Thread.Sleep(100);
            }

            // Create the recurring operation
            var operation = new RecurringOperation("Test");

            // Start recurring
            RecurringOperationManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.1), DoWork);

            // Wait for the operation to occur atleast once
            Thread.Sleep(250);

            // Abort the operation, so that other tests can run an operation with the same name
            RecurringOperationManager.Instance.Abort(operation);

            // Assert that LastRunStart is greater than the time before recurring
            Assert.IsTrue(operation.LastRunStart > beforeRecurring);
        }

        [TestMethod]
        public void LastRunFinishHasLatestRunFinishTime()
        {
            DateTime beforeRecurring = DateTime.Now;

            void DoWork()
            {
                Thread.Sleep(100);
            }

            // Create the recurring operation
            var operation = new RecurringOperation("Test");

            // Start recurring
            RecurringOperationManager.Instance.StartRecurring(
                operation, TimeSpan.FromSeconds(0.1), DoWork);

            // Wait for the operation to occur atleast once
            Thread.Sleep(250);

            // Abort the operation, so that other tests can run an operation with the same name
            RecurringOperationManager.Instance.Abort(operation);

            // Assert that LastRunFinish is greater than the time before recurring
            Assert.IsTrue(operation.LastRunFinish > beforeRecurring);
        }
    }
}
