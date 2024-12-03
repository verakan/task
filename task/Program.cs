using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        int maxNumber = 1000000;

        // Создание источника токена отмены
        CancellationTokenSource cts = new CancellationTokenSource();

        // Создание и запуск задачи поиска простых чисел с токеном отмены
        Task primeTask = Task.Run(() => FindPrimes(maxNumber, cts.Token), cts.Token);
        Console.WriteLine($"Task ID: {primeTask.Id}");

        // Проверка состояния задачи во время выполнения
        while (!primeTask.IsCompleted)
        {
            Console.WriteLine($"Task Status: {primeTask.Status}");
            await Task.Delay(500); // Задержка для периодической проверки статуса
        }

        // Отмена задачи через 2 секунды
        Task.Delay(2000).ContinueWith(_ => cts.Cancel());

        try
        {
            await primeTask;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Task was cancelled.");
        }

        // Оценка производительности
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 5; i++)
        {
            FindPrimes(maxNumber, CancellationToken.None);
        }
        stopwatch.Stop();
        Console.WriteLine($"Average Time: {stopwatch.ElapsedMilliseconds / 5} ms");

        // Проверка производительности с помощью Task
        stopwatch.Restart();
        for (int i = 0; i < 5; i++)
        {
            await Task.Run(() => FindPrimes(maxNumber, CancellationToken.None));
        }
        stopwatch.Stop();
        Console.WriteLine($"Average Task Time: {stopwatch.ElapsedMilliseconds / 5} ms");

        // Создание трёх задач с возвратом результата
        Task<int> task1 = Task.Run(() => Calculate(10));
        Task<int> task2 = Task.Run(() => Calculate(20));
        Task<int> task3 = Task.Run(() => Calculate(30));

        // Четвёртая задача, которая суммирует результаты предыдущих задач с использованием ContinueWith
        Task<int> finalTaskWithContinueWith = Task.WhenAll(task1, task2, task3).ContinueWith(t =>
        {
            int result = t.Result[0] + t.Result[1] + t.Result[2];
            Console.WriteLine($"Final Result with ContinueWith: {result}");
            return result;
        });

        await finalTaskWithContinueWith; // Ожидание завершения четвёртой задачи

        // Создание трёх задач с возвратом результата
        Task<int> task4 = Task.Run(() => Calculate(10));
        Task<int> task5 = Task.Run(() => Calculate(20));
        Task<int> task6 = Task.Run(() => Calculate(30));

        // Четвёртая задача с использованием GetAwaiter() и GetResult()
        int result1 = task4.GetAwaiter().GetResult();
        int result2 = task5.GetAwaiter().GetResult();
        int result3 = task6.GetAwaiter().GetResult();
        int finalResultWithAwaiter = result1 + result2 + result3;
        Console.WriteLine($"Final Result with GetAwaiter().GetResult(): {finalResultWithAwaiter}");
    }

    static void FindPrimes(int maxNumber, CancellationToken token)
    {
        // Решето Эратосфена
        bool[] isPrime = new bool[maxNumber + 1];
        for (int i = 2; i <= maxNumber; i++)
        {
            isPrime[i] = true;
        }

        for (int i = 2; i * i <= maxNumber; i++)
        {
            token.ThrowIfCancellationRequested(); // Проверка токена отмены
            if (isPrime[i])
            {
                for (int j = i * i; j <= maxNumber; j += i)
                {
                    isPrime[j] = false;
                }
            }
        }

        // Подсчет количества простых чисел
        int primeCount = 0;
        for (int i = 2; i <= maxNumber; i++)
        {
            if (isPrime[i])
            {
                primeCount++;
            }
        }

        Console.WriteLine($"Found {primeCount} primes.");
    }

    static int Calculate(int value)
    {
        // Вычисление квадрата числа
        return value * value;
    }
}
