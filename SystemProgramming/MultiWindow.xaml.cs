﻿using System;
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
using System.Windows.Threading;

namespace SystemProgramming
{
    /// <summary>
    /// Логика взаимодействия для MultiWindow.xaml
    /// </summary>
    public partial class MultiWindow : Window
    {
        private readonly Random random = new();
        public MultiWindow()
        {
            InitializeComponent();
        }

        #region variant 1
        private void ButtonStart1_Click(object sender, RoutedEventArgs e)
        {
            sum = 100;
            progressBar1.Value = 0;
            for (int i = 0; i < 12; i++)
            {
                new Thread(plusPercent).Start();
            }
        }

        private void ButtonStop1_Click(object sender, RoutedEventArgs e)
        {

        }

        private double sum;

        private void plusPercent()
        {
            // в первом варианте каждый месяц 10%
            double val = sum;   // получаем предыдущие данные
            Thread.Sleep(random.Next(250, 350));  // имитируем длительный запрос данных
            double percent = 10;
            // рассчитываем итог
            val *= 1 + percent / 100;
            // сохраняем изменения в общей сумме
            sum = val;
            // выводим данные о своей работе
            Dispatcher.Invoke(() =>
            {
                ConsoleBlock.Text += sum + "\n";
                progressBar1.Value += 100.0 / 12;
            });
        }
        #endregion

        #region variant 2
        private CancellationTokenSource cts;   // Источник токенов отмены потоков
        private void ButtonStart2_Click(object sender, RoutedEventArgs e)
        {
            sum2 = 100;
            progressBar2.Value = 0;
            cts = new();
            for (int i = 0; i < 12; i++)
            {
                new Thread(plusPercent2).Start(cts.Token);
            }
        }

        private void ButtonStop2_Click(object sender, RoutedEventArgs e)
        {
            cts?.Cancel();
        }

        private double sum2;
        private readonly object locker2 = new();     // объект для синхронизации

        private void plusPercent2(object? token)
        {
            if (token is not CancellationToken) return;

            CancellationToken cancellationToken = (CancellationToken)token;
            // отмена токена (cts?.Cancel()) сама по себе не остановит поток, а только
            // установит токен в отмененное состояние. Поток должен проверять токен
            // в разрешенных для отмены местах и принимать решение об отмене.     

            double val;
            lock (locker2)                           // синхро-блок
            {                                        // поток, который первым входит в
                if (cancellationToken.IsCancellationRequested) return;
                // cancellationToken.ThrowIfCancellationRequested();
                val = sum2;                          // блок, закрывает locker2 и открывает
                Thread.Sleep(random.Next(250, 350)); // его по выходу из блока.
                double percent = 10;                 // Другие потоки, дойдя до lock
                val *= 1 + percent / 100;            // видят, что объект закрыт и переходят
                sum2 = val;                          // в ждущее состояние до его открытия.
            }                                        // Первый из дождавшихся снова его закроет и т.д.

            Dispatcher.Invoke(() =>
            {
                ConsoleBlock.Text += val + "\n";
                progressBar2.Value += 100.0 / 12;
            });
        }
        #endregion


        #region variant 3
        private CancellationTokenSource cts3;

        private void ButtonStart3_Click(object sender, RoutedEventArgs e)
        {
            sum3 = 100;
            progressBar3.Value = 0;
            cts3 = new CancellationTokenSource();

            for (int i = 0; i < 12; i++)
            {
                var data = new ThreadData3 { Month = i + 1, Token = cts3.Token };
                new Thread(plusPercent3).Start(data);
            }
        }

        private void ButtonStop3_Click(object sender, RoutedEventArgs e)
        {
            cts3?.Cancel();
        }

        private double sum3;
        private readonly object locker3 = new();

        private void plusPercent3(object? token)
        {
            if (token is not ThreadData3 data) return;

            double val;

            for (int i = 0; i < 10; i++)
            {
                if (data.Token.IsCancellationRequested) return;
                Thread.Sleep(random.Next(250, 350));
            }

            double percent = 10 + data.Month;
            double factor = 1 + percent / 100;

            lock (locker3)
            {
                val = sum3;
                val *= factor;
                sum3 = val;
            }

            Dispatcher.Invoke(() =>
            {
                ConsoleBlock.Text += data.Month + " " + percent + " " + val + "\n";
                progressBar3.Value += 100.0 / 12;
            });
        }
        #endregion


        class ThreadData3   // комплексный тип для передачи нескольких данных в поток
        {
            public int Month { get; set; }
            public CancellationToken Token { get; set; }
        }


        #region variante 4 + номер месяца
        private void ButtonStart4_Click(object sender, RoutedEventArgs e)
        {
            sum4 = 100;
            for (int i = 0; i < 12; i++)
            {
                new Thread(plusPersent4).Start(i + 1);
            }
        }

        private void ButtonStop4_Click(object sender, RoutedEventArgs e)
        {

        }

        private double sum4;
        private readonly object locker4 = new object(); // обьект для срнхранизации.

        private void plusPersent4(object? month)
        {
            if (month is not int) return;

            double val;

            Thread.Sleep(random.Next(250, 350));
            double perсent = 10 + (int)month;
            double factor = 1 + perсent / 100;

            lock (locker4)
            {
                val = sum4;
                val *= factor;
                sum4 = val;
            }
            this.Dispatcher.Invoke(() =>
            {
                ConsoleBlock.Text += val + " Номер месяца - " + month + " Процент - " + perсent + "\n";
                progressBar4.Value += 100.0 / 12;
            });

        }
        #endregion

        #region Thread Pool
        CancellationTokenSource cts5;
        private void ButtonStart5_Click(object sender, RoutedEventArgs e)
        {
            cts5 = new CancellationTokenSource();
            for (int i = 0; i < 25; i++)
            {
                ThreadPool
                    .QueueUserWorkItem(
                    plusPercent5,
                    new ThreadData3
                    {
                        Month = i,
                        Token = cts5.Token
                    });
            }
        }

        private void ButtonStop5_Click(object sender, RoutedEventArgs e)
        {
            cts5?.Cancel();
        }
        private double sum5;
        private readonly object locker5 = new();     // объект для синхронизации

        private void plusPercent5(object? data)
        {
            var threadData = data as ThreadData3;
            if (threadData is null) return;
            double val;
            try
            {

                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(random.Next(250, 350));   // часть рассчетов,
                    threadData.Token.ThrowIfCancellationRequested();  // место для возможной отмены потока
                }
                double percent = 10 + threadData!.Month;      // вынесенная
                double factor = 1 + percent / 100;     // за синхроблок
                lock (locker5)
                {                                      // внутри блока
                    val = sum5;                        // остается часть рассчетов
                    val *= factor;                     // которую нельзя более
                    sum5 = val;                        // разделять
                }
                Dispatcher.Invoke(() =>
                {
                    ConsoleBlock.Text += threadData!.Month + " " + percent + " " + val + "\n";
                    progressBar5.Value += 100.0 / 25;
                });
            }
            catch (OperationCanceledException)
            {
                Dispatcher.Invoke(() =>
                {
                    ConsoleBlock.Text += threadData!.Month + " Cancelled\n";
                });
            }
        }
    }
    #endregion
}



