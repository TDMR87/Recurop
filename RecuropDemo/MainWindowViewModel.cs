using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Recurop;

namespace RecuropDemo
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TimeSpan> LapTimes { get; set; }
        public ObservableCollection<string> ExceptionMessages { get; set; }
        public RecurringOperation MyRecurringOperation { get; }
        public TimeSpan DisplayTime
        {
            get => _displayTime ?? TimeSpan.Zero;
            private set
            {
                _displayTime = value;
                NotifyPropertyChanged();
            }
        }
        public MainWindowViewModel()
        {
            // Initialize collections
            LapTimes = new ObservableCollection<TimeSpan>();
            ExceptionMessages = new ObservableCollection<string>();

            // Initialize the recurring operation
            MyRecurringOperation = new RecurringOperation(name: "TimerOperation");
            MyRecurringOperation.OperationFaulted += OnOperationFaulted;
            MyRecurringOperation.StatusChanged += OnOperationStatusChanged;
        }

        private int elapsedSeconds;
        private bool throwException;
        private TimeSpan? _displayTime;

        private void OnOperationFaulted(Exception ex)
        {
            // Because this exception event handler delegate is invoked on a
            // threadpool thread, we can only modify the list on the
            // thread it was created on (in this case the UI thread).
            App.Current.Dispatcher.Invoke(() =>
            {
                ExceptionMessages.Add(ex.Message);
            });
        }

        private void OnOperationStatusChanged()
        {
            if (MyRecurringOperation.Status == RecurringOperationStatus.Cancelled)
            {

            }
        }

        public ICommand StartTimerCommand => new RelayCommand(() =>
        {
            RecurringOperations.Manager.StartRecurring(
                MyRecurringOperation, TimeSpan.FromSeconds(1), () =>
                {
                    elapsedSeconds++;

                    DisplayTime = TimeSpan.FromSeconds(elapsedSeconds);

                    Thread.Sleep(500);

                    if (throwException)
                    {
                        throwException = false;
                        throw new InvalidOperationException("An exception was thrown inside the recurring operation.");
                    }
                });
        });

        public ICommand PauseTimerCommand => new RelayCommand(() =>
        {
            RecurringOperations.Manager.PauseRecurring(MyRecurringOperation);
        });

        public ICommand ContinueTimerCommand => new RelayCommand(() =>
        {
            RecurringOperations.Manager.ResumeRecurring(MyRecurringOperation);
        });

        public ICommand CancelTimerCommand => new RelayCommand(() =>
        {
            RecurringOperations.Manager.Cancel(MyRecurringOperation);

            DisplayTime = TimeSpan.Zero;

            elapsedSeconds = 0;
        });

        public ICommand ThrowCommand => new RelayCommand(() =>
        {
            throwException = true;
        });

        public ICommand LapTimeCommand => new RelayCommand(() =>
        {
            LapTimes.Add(DisplayTime);
        });

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
