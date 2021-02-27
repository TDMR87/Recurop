using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Recurop
{
    /// <summary>
    /// Represents a recurring background job.
    /// </summary>
    public class RecurringOperation : INotifyPropertyChanged
    {
        public RecurringOperation(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException(Constants.UnnamedOperationException);

            _name = name;
            IsExecuting = false;
            IsRecurring = false;
            IsNotRecurring = true;
            IsPaused = false;
            IsIdle = true;
            IsCancelled = false;
            CallbackLock = new object();
            Status = RecurringOperationStatus.Idle;
        }
        private readonly string _name;

        /// <summary>
        /// Indicates whether the recurring background operation is 
        /// currently executing it's specified action.
        /// </summary>
        public bool IsExecuting
        {
            get => isExecuting;
            internal set
            {
                isExecuting = value;
                OnPropertyChanged();
            }
        }
        private bool isExecuting;

        /// <summary>
        /// Indicates whether the recurring background operation is 
        /// currently in recurring state (and not cancelled, for example).
        /// </summary>
        public bool IsRecurring
        {
            get => isRecurring;
            internal set
            {
                isRecurring = value;
                OnPropertyChanged();
            }
        }
        private bool isRecurring;

        /// <summary>
        /// Indicates whether the recurring background operation is 
        /// currently in recurring state.
        /// </summary>
        public bool IsNotRecurring
        {
            get => !isRecurring;
            internal set
            {
                isNotRecurring = value;
                OnPropertyChanged();
            }
        }
        private bool isNotRecurring;

        /// <summary>
        /// Indicates whether the recurring background operation is 
        /// currently in recurring state (and not cancelled, for example).
        /// </summary>
        public bool IsCancelled
        {
            get => isCancelled;
            internal set
            {
                isCancelled = value;
                OnPropertyChanged();
            }
        }
        private bool isCancelled;

        /// <summary>
        /// Indicates whether the recurring operation is currently paused.
        /// </summary>
        public bool IsPaused
        {
            get => isPaused;
            internal set
            {
                isPaused = value;
                OnPropertyChanged();
            }
        }
        private bool isPaused;

        /// <summary>
        /// Indicates whether the recurring operation is idle. An idle state means that 
        /// the operation is not yet started or it has been aborted.
        /// </summary>
        public bool IsIdle
        {
            get => isIdle;
            set
            {
                isIdle = value;
                OnPropertyChanged();
            }
        }
        private bool isIdle;

        /// <summary>
        /// The start time of the latest background operation execution.
        /// </summary>
        public DateTime LastRunStart
        {
            get => lastRunStart;
            internal set
            {
                lastRunStart = value;
                OnPropertyChanged();
            }
        }
        private DateTime lastRunStart;

        /// <summary>
        /// The start time of the latest background operation execution.
        /// </summary>
        public DateTime LastRunFinish
        {
            get => lastRunFinish;
            internal set
            {
                lastRunFinish = value;
                OnPropertyChanged();
            }
        }
        private DateTime lastRunFinish;

        public RecurringOperationStatus Status
        {
            get => status;
            internal set
            {
                status = value;
                OnStatusChanged();
                OnPropertyChanged();
            }
        }
        private RecurringOperationStatus status;

        /// <summary>
        /// A lock object used to prevent overlapping threads from modifying
        /// the properties of the Background Operation object. Use with lock-statement.
        /// </summary>
        internal object CallbackLock { get; }

        /// <summary>
        /// When the Background Operation's action delegate throws an exception,
        /// the exception is accessible through this property. The next exception
        /// in the action delegate will override any previous value in this property.
        /// </summary>
        public Exception Exception
        {
            get => exception;
            internal set
            {
                exception = value;
                OnOperationFaulted();
            }
        }
        private Exception exception;

        /// <summary>
        /// The event handler is triggered whenever the status of
        /// the background operation changes.
        /// </summary>
        public event Action StatusChanged;

        /// <summary>
        /// The event handler is triggered when the
        /// Background Operation's action delegate throws an exception.
        /// </summary>
        public event Action<Exception> OperationFaulted;

        /// <summary>
        /// The event handler is triggered when properties of the Background Operation
        /// change value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnStatusChanged()
        {
            StatusChanged?.Invoke();
        }

        protected void OnOperationFaulted()
        {
            OperationFaulted?.Invoke(Exception);
        }

        /// <summary>
        /// Returns the identifying name of this background operation.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                throw new InvalidOperationException(Constants.UninitializedOperationException);
            }

            return _name;
        }

        /// <summary>
        /// Returns the identifying name of this background operation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                throw new InvalidOperationException(Constants.UninitializedOperationException);
            }

            return _name;
        }
    }
}

