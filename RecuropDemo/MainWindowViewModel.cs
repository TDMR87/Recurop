using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Recurop;

namespace RecuropDemo
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public RecurringOperation MyRecurringOperation { get; } = new RecurringOperation(name: "TimerOperation");
        private int elapsedSeconds;

        public TimeSpan DisplayTime
        {
            get => _displayTime ?? TimeSpan.Zero;
            private set
            {
                _displayTime = value;
                NotifyPropertyChanged();
            }
        }
        private TimeSpan? _displayTime;

        public ICommand StartCommand => new RelayCommand(() =>
        {
            RecurringOperationManager.Instance.StartRecurring(
                MyRecurringOperation, TimeSpan.FromSeconds(1), () =>
                {
                    DisplayTime = TimeSpan.FromSeconds(elapsedSeconds);
                    elapsedSeconds++;
                });
        });

        public ICommand StopCommand => new RelayCommand(() =>
        {
            RecurringOperationManager.Instance.StopRecurring(MyRecurringOperation);
        });

        public ICommand ContinueCommand => new RelayCommand(() =>
        {
            RecurringOperationManager.Instance.ResumeRecurring(MyRecurringOperation);
        });

        public ICommand CancelCommand => new RelayCommand(() =>
        {
            RecurringOperationManager.Instance.Abort(MyRecurringOperation);

            DisplayTime = TimeSpan.Zero;

            elapsedSeconds = 0;
        });

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
