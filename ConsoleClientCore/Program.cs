using System;
using Recurop;

namespace ConsoleClientCore
{
    class Program
    {
        static RecurringOperation MyRecurringOperation;
        static int counter = 1;

        static void Main(string[] args)
        {
            // Initialize the recurring operation
            MyRecurringOperation = new RecurringOperation(name: "IncrementCounter");
            MyRecurringOperation.StatusChanged += OnStatusChanged;
            MyRecurringOperation.OperationFaulted += OnOperationFaulted;

            // Start recurring
            RecurringOperationManager.Instance.StartRecurring(
                MyRecurringOperation, TimeSpan.FromSeconds(2), CountToMaxInt);

            // While the operation is not aborted
            while (MyRecurringOperation.Status != RecurringOperationStatus.Aborted)
            {
                // Get console input from user
                GetUserInput();
            }

            Console.ReadKey();
        }

        static void OnOperationFaulted(Exception ex)
        {
            Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has faulted: " +
                              $"{MyRecurringOperation.Exception?.Message}");
        }

        static void OnStatusChanged()
        {
            if (MyRecurringOperation.Status == RecurringOperationStatus.Aborted)
                Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has been aborted.");
            else if (MyRecurringOperation.Status == RecurringOperationStatus.Stopped)
                Console.WriteLine($"Operation {MyRecurringOperation.GetName()} has stopped.");
        }

        static void GetUserInput()
        {
            var input = Console.ReadLine();

            if (input == "/stop")
                RecurringOperationManager.Instance.StopRecurring(MyRecurringOperation);
            else if (input == "/abort")
                RecurringOperationManager.Instance.Abort(MyRecurringOperation);
            else if (input == "/resume")
                RecurringOperationManager.Instance.ResumeRecurring(MyRecurringOperation);
            else if (input == "/status")
            {
                Console.WriteLine();
                Console.WriteLine($"Operation {MyRecurringOperation.GetName()}'s current status: {MyRecurringOperation.Status}");
                Console.WriteLine($"Last run start: {MyRecurringOperation.LastRunStart}");
                Console.WriteLine($"Last run finish: {MyRecurringOperation.LastRunFinish}");
                Console.WriteLine();
            }
        }

        static void CountToMaxInt()
        {
            Console.WriteLine($"Incrementing counter...");

            // Throw a random exception
            int random = new Random().Next(0, 5);
            if (random == 1)
            {
                throw new InvalidOperationException("A mysterious error occurred..\n");
            }

            Console.WriteLine($"Counter = {counter++}\n");
        }
    }
}
