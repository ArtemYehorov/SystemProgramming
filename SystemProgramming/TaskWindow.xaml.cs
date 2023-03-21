using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SystemProgramming
{
    public partial class TaskWindow : Window
    {
        public Random random = new Random();
        public TaskWindow()
        {
            InitializeComponent();
        }

        private async void ButtonStart1_Click(object sender, RoutedEventArgs e)
        {
            sum = 100;
            ConsoleBlock.Text = "";
            for (int i = 0; i < 12; i++)
            {
                // Task.Run(PlusPercent).Wait();
                await PlusPercent();
            }
        }
        private void ButtonStop1_Click(object sender, RoutedEventArgs e)
        {

        }
        private double sum;
        int month = 0;
        private async Task PlusPercent()
        {
            await Task.Delay(300);
            sum *= 1.1;
            ConsoleBlock.Text += $"{sum}\n";
            // Dispatcher.Invoke(() => ConsoleBlock.Text += $"{sum}\n");
        }

        private async Task<double> GetPercentageAsync(int month)
        {
            await Task.Delay(random.Next(250, 350)); // случайная задержка
            double percentage = 0.0;

            switch (month)
            {
                case 1:
                    percentage = 0.05;
                    break;
                case 2:
                    percentage = 0.06;
                    break;
                case 3:
                    percentage = 0.07;
                    break;
                case 4:
                    percentage = 0.08;
                    break;
                case 5:
                    percentage = 0.09;
                    break;
                case 6:
                    percentage = 0.1;
                    break;
                case 7:
                    percentage = 0.11;
                    break;
                case 8:
                    percentage = 0.12;
                    break;
                case 9:
                    percentage = 0.13;
                    break;
                case 10:
                    percentage = 0.14;
                    break;
                case 11:
                    percentage = 0.15;
                    break;
                case 12:
                    percentage = 0.16;
                    break;
                default:
                    percentage = 0.0;
                    break;
            }
            progressBar2.Value += 100.0 / 12;
            return percentage;

        }

        private void ButtonStart2_Click(object sender, RoutedEventArgs e)
        {
            // О задачах детальнее (Demo 1)
            // Задача представляет собой одно выполнение одного метода/функции
            Task task1 = new Task(proc1);   // создаем объект, выполнение не запускается
            // task1.RunSynchronously();    // запуск в синхронном контексте
            task1.Start();                  // запуск асинхронно (здесь - в новом потоке)
            Task.Run(proc1);                // тоже асинхронный запуск - параллельно с предыдущим
        }
        private void proc1()
        {
            ConsoleWrite("proc1 started\n");
            Thread.Sleep(1000);
            ConsoleWrite("proc1 finished\n");
        }
        private void ConsoleWrite(Object item)
        {
            this.Dispatcher.Invoke(
                () => ConsoleBlock.Text += item is null ? "NULL" : item.ToString());
        }

        private void ButtonStop2_Click(object sender, RoutedEventArgs e)
        {
            // О задачах детальнее (Demo 2)
            // Варианты запуска задач по очереди:
            // - синхронный запуск
            // - ожидание
            // - продолжение
            Task task1 = new Task(procN, 1);   // параметр для метода - в конструкторе
            Task task2 = new Task(procN, 2);

            // task1.Start();  // Такой запуск - параллельный, обе сразу начинают работать
            // task2.Start();  // Причем иногда надпись task2 появляется раньше, чем task1

            task1.RunSynchronously();  // Работают последовательно, но первая задача
            task2.Start();             // блокирует UI

            /* Click - task1.RS[-proc(1)-] - task2.Start() - end (UI свободен)
             *                                        \
             *                                         proc(2).......
             */
        }
        private void procN(object? item)
        {
            ConsoleWrite($"proc{item?.ToString()} started\n");
            Thread.Sleep(1000);
            ConsoleWrite($"proc{item?.ToString()} finished\n");
        }

        private async void ButtonDemo3_Click(object sender, RoutedEventArgs e)
        {
            // О задачах детальнее (Demo 3)
            // Варианты запуска задач по очереди: ожидание
            Task task1 = new Task(procN, 1);   // параметр для метода - в конструкторе
            Task task2 = new Task(procN, 2);
            task1.Start();
             await task1; //ожидание см TAsk.txt - UI vs Dispatcher
            task2.Start();
            /* Button (UI) - UI Свободен           Suspending mode - поток блокируеться и ожидает 
             *       \                          ---|---
             *        async Click   task1.Start()      await task1; task2.Start() - end
             *                       \                /             \
             *                       proc1(1).......                 proc(2).......
             */

        }

        private void ButtonDemo4_Click(object sender, RoutedEventArgs e)
        {
            // О задачах детальнее (Demo 4)
            // Варианты запуска задач по очереди: - продолжение
            Task task1 = new Task(procN, 1);   // параметр для метода - в конструкторе
            Task task2 = new Task(procN, 2);
            task1.ContinueWith(_ => task2.Start())
                .ContinueWith(_ => new Task(procN, 3).Start());
            task1.Start();
            /* Эта схема называеться "ниткой"
             * Click - task1.Start() - end ( UI свободен )
             *                   \
             *                    proc(1) -- task2.Start() -- new Task(procN, 3).Start()
             */
        }

        private void ButtonDemo1_2_Click(object sender, RoutedEventArgs e)
        {
            ConsoleWrite("funcN(1) started!\n");
            var task1 = funcN(1);   // вызов, но возврат - Task, отвечающий за исполнение 
                                    // аналог task1 = new(funcN); task1.Start();
            ConsoleWrite(task1.Result); // .Result также комбинация  - .Wait() - .GetResult()
                                        // .Wait - приводит к зависанию из-за Dispetcher
        }

        private async void ButtonDemo2_2_Click(object sender, RoutedEventArgs e)
        {
            ConsoleWrite("funcN(2) started!\n");       // funcN(2) - возврат Task<String>
            ConsoleWrite(await funcN(2));              // await - "Извлекает" сам String (.Result)
        }

        private async void ButtonDemo3_2_Click(object sender, RoutedEventArgs e)
        {
            ConsoleWrite("funcN(1) started!\n");       // Выполнение последовательное 
            ConsoleWrite(await funcN(1));              // await - ожидает завершения

            ConsoleWrite("funcN(2) started!\n");       // эти команды выполнятся
            ConsoleWrite(await funcN(2));              // после окончания  funcN(1)
        }

        private async void ButtonDemo4_2_Click(object sender, RoutedEventArgs e)
        {
            // Указания await funcN(1) не даёёт их заупскать паралельно 
            Task<String> task1 = funcN(1);  // Запуск + возврат задачи
            Task<String> task2 = funcN(2);  // Запуск паралельно с task1
            ConsoleWrite("funcN(1) started!\n");    // этото код выполняется уже во время 
            ConsoleWrite("funcN(2) started!\n");    // работы запущенных више задач
            // String res1 = task1.Result;  - зависание (При синхронном методе)
            ConsoleWrite(await task1);
            ConsoleWrite(await task2);
        }

        private async void ButtonDemo5_2_Click(object sender, RoutedEventArgs e)
        {
            // работа с множеством задач 
            Task<String>[] tasks = new Task<String>[7];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = funcN(i);  //запуск 7 задач паралельно
            }
            ConsoleWrite("tasks started!\n");
            //Task.WaitAll(tasks);  //Ожидание окончания всех задач
            //Task.WaitAny(tasks)  //ожидание окончания одной любой
            foreach (var task in tasks)
            {
                ConsoleWrite(await task);
            }
        }

        private async Task<String> funcN(int n)   //  тип функции Task<String>
        {                                         // 
            await Task.Delay(1000);               //
            return $"funcN({n}) result\n";             //  возврат - просто String
        }                                         //

        private async void ButtonStart21_Click(object sender, RoutedEventArgs e)
        {
            sum = 100;
            ConsoleBlock.Text = "";
            for (int i = 0; i < 12; i++)
            {
                // Task.Run(PlusPercent).Wait();
                sum *= (1 + await GetPercentageAsync(month));
                month++;
                ConsoleBlock.Text += $"{sum} - {month}\n";
            }
        }

        private void ButtonStop1_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
/* Д.З. Реализовать решение задачи рассчета годовых процентов при помощи многозадачности
 * Использовать случайную задержку 250-350 мс
 * Отображать номер месяца, процент и полученный результат, а также двигать индикатор прогресса
 */

