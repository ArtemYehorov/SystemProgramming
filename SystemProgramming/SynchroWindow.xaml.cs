using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Sockets;
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
    public partial class SynchroWindow : Window
    {
        public SynchroWindow()
        {
            InitializeComponent();
        }
        private volatile bool stopAll = false;
        #region 1. lock

        private void StartLock_Click(object sender, RoutedEventArgs e)
        {
            stopAll = false;
            for (int i = 1; i <= 5; ++i)
            {
                new Thread(DoWork1).Start(i);
            }
        }
        private void StopLock_Click(object sender, RoutedEventArgs e)
        {
            stopAll = true;
        }
        private readonly object locker = new();   // Вместо системного ресурса синхронизации
                                                  // мы используем возможности ссылочных типов -
                                                  // встроенное наличие "критической секции"
        private void DoWork1(object? state)
        {
            lock (locker)
            {
                while (!stopAll)
                {
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " start\n");
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " finish\n");
                }
            }
        }

        #endregion

        #region 2. Monitor
        private void StartMonitor_Click(object sender, RoutedEventArgs e)
        {
            stopAll = false;
            for (int i = 1; i <= 5; ++i)
            {
                new Thread(DoWork2).Start(i);
            }
        }
        private void StopMonitor_Click(object sender, RoutedEventArgs e)
        {
            stopAll = true;
        }

        private readonly object monitor = new();
        private void DoWork2(object? state)
        {
            try
            {
                while (!stopAll)
                {
                    Monitor.Enter(monitor);
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " start\n");
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " finish\n");
                }

            }
            finally
            {
                Monitor.Exit(monitor);  // Выход == разблокирование
            }
        }
        #endregion

        #region 3. Mutex
        private void StartMutex_Click(object sender, RoutedEventArgs e)
        {
            stopAll = false;
            for (int i = 1; i <= 5; ++i)
            {
                new Thread(DoWork3).Start(i);
            }
        }
        private void StopMutex_Click(object sender, RoutedEventArgs e)
        {
            stopAll = true;
        }
        private Mutex mutex = new();

        private void DoWork3(object? state)
        {
            try
            {
                mutex.WaitOne();
                while (!stopAll)
                {
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " start\n");
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " finish\n");
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        #endregion

        #region 4. EventWaitHandle
        private void StartEWH_Click(object sender, RoutedEventArgs e)
        {
            stopAll = false;
            for (int i = 1; i <= 5; ++i)
            {
                new Thread(DoWork4).Start(i);
            }
            // тут можно сделать работу до начала работы потоков
            gates.Set();  // подать сигнал первого открытия
        }
        private void StopEWH_Click(object sender, RoutedEventArgs e)
        {
            stopAll = true;
        }

        private EventWaitHandle gates = new AutoResetEvent(false);  // объект с авто-разблокировкой по событию, true - изначально открытый

        private void DoWork4(object? state)
        {
            gates.WaitOne();
            while (!stopAll)
            {
                Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " start\n");
                Thread.Sleep(1000);
                Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " finish\n");

                gates.Set();
            }
        }
        #endregion

        #region 5. Semaphore
        private void StartSemaphore_Click(object sender, RoutedEventArgs e)
        {
            stopAll = false;
            for (int i = 1; i < 5; i++)
            {
                new Thread(DoWork5).Start(i);
            }
            semaphore.Release(2);
            Task.Run(async () =>
            {
                await Task.Delay(200);
                semaphore.Release(1);
            });
        }
        private void StopSemaphore_Click(object sender, RoutedEventArgs e)
        {
            stopAll = true;
        }


        // private Semaphore semaphore = new(3, 3);   // первая 3 - свободные места, вторая 3 - максимальное кол-во
        private Semaphore semaphore = new(0, 3);  // изначально нет свободных мест

        private void DoWork5(object? state)
        {
            semaphore.WaitOne();
            try
            {
                while (!stopAll)
                {
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " start\n");
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " finish\n");
                }
            }
            finally
            {
                semaphore.Release(1);
            }
        }
        #endregion

        #region 6. SemaphoreSlim

        private void StartSemaphoreSlim_Click(object sender, RoutedEventArgs e)
        {
            stopAll = false;
            for (int i = 1; i < 5; i++)
            {
                new Thread(doWork6).Start(i);
            }
            semaphoreSlim.Release(2);
            Task.Run(async () =>
            {
                await Task.Delay(200);
                semaphoreSlim.Release(1);
            });
        }
        private void StopSemaphoreSlim_Click(object sender, RoutedEventArgs e)
        {
            stopAll = true;
        }

        private readonly SemaphoreSlim semaphoreSlim = new(0, 3);
        private void doWork6(object? state)
        {
            semaphoreSlim.Wait();
            try
            {
                while (!stopAll)
                {
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " start\n");
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() => ConsoleBlock.Text += state?.ToString() + " finish\n");
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        #endregion
    }
}



