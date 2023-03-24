using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// <summary>
    /// Логика взаимодействия для DLLWindow.xaml
    /// </summary>
    public partial class DLLWindow : Window
    {
        [DllImport("User32.dll")]
        public static extern
           int MessageBoxA(
               IntPtr hWnd,        // HWND  
               String lpText,      // LPCSTR
               String lpCaption,   // LPCSTR
               uint uType        // UINT  
           );

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBoxW(IntPtr hWnd, String lpText, String lpCaption, uint uType);


        [DllImport("Kernel32.dll", EntryPoint = "Beep")]
        public static extern bool Sound(uint dwFreq, uint dwDuration);

        /* 
         * HANDLE CreateThread(
              [in, optional]  LPSECURITY_ATTRIBUTES   lpThreadAttributes,
              [in]            SIZE_T                  dwStackSize,
              [in]            LPTHREAD_START_ROUTINE  lpStartAddress,
              [in, optional]  __drv_aliasesMem LPVOID lpParameter,
              [in]            DWORD                   dwCreationFlags,
              [out, optional] LPDWORD                 lpThreadId
            );
         */
        [DllImport("Kernel32.dll", EntryPoint = "CreateThread")]
        public static extern
            IntPtr CreateThread(
                    IntPtr lpThreadAttributes,  // указатель на структуру с параметрами безопасности (NULL)
                    uint dwStackSize,         // граничный размер стека - 0 (по умолчанию)
              ThreadMethod lpStartAddress,      // указатель на стартовый адрес (функции)
                    IntPtr lpParameter,         // указатель на объект с параметрами для ф-ции
                    uint dwCreationFlags,     // флаги запуска - 0 (по умолчанию)
                    IntPtr lpThreadId           // возврат id потока (NULL - не возвращать)
            );
        // главный вопрос - как получить адрес метода в .NET и передать его в неуправляемый код
        // 1. Описываем делегат по документации на функцию (CreateThread)
        public delegate void ThreadMethod();
        // 2. Заменяем IntPtr в декларации ф-ции на делегат ThreadMethod
        // 3. Описываем метод с сигнатурой делегата
        public void SayHello()
        {
            Dispatcher.Invoke(() => SayHelloLabel.Content = "Hello");
            sayHelloHandle.Free();  // по окончанию работы - освобождаем (расфиксируем) объект
        }
        // 4. При вызове ф-ции CreateThread в параметре lpStartAddress указываем SayHello
        //    (см. SayHelloButton_Click)

        //Делегат для передачи адресса переодически вызываемого метода
        delegate void TimerMethod(uint uTimerID, uint uMsg, ref uint dwUser, uint dw1, uint dw2);
        [DllImport("winmm.dll", EntryPoint = "timeSetEvent")]
        static extern uint TimeSetEvent(
            uint uDeley,
            uint uResolution,
            TimerMethod lpTimeProc,
            ref uint dwUser,
            uint eventType
            );

        [DllImport("winmm.dll", EntryPoint = "timeKillEvent")]
        static extern void TimeKillEvent(uint uTimerID);

        const uint TIME_ONESHOT = 0; //eventType
        const uint TIME_PERIODIC = 1;

        uint uDelay;
        uint timerID;
        uint uResolution;
        uint dwUser = 0;
        TimerMethod timerMethod = null!;
        GCHandle timerHandle;

        int ticks;
        void TimerTick(uint uTimerID, uint uMsg, ref uint dwUser, uint dw1, uint dw2)
        {
            ticks++;
            Dispatcher.Invoke(() => { TicksLable.Content = ticks.ToString(); });
        }

        int sec = 0;
        int min = 0;
        int hour = 0;

        void TimerTick2(uint uTimerID, uint uMsg, ref uint dwUser, uint dw1, uint dw2)
        {
            ticks++;
            Dispatcher.Invoke(() => { TicksLable1.Content = $"{hour}:{min}:{sec}." + ticks.ToString(); });
            if (ticks == 99)
            {
                Dispatcher.Invoke(() => { TicksLable1.Content = $"00:00:{++sec}." + ticks.ToString(); });
                ticks = 0;
            }

            if (sec == 59)
            {
                Dispatcher.Invoke(() => { TicksLable1.Content = $"00:{++min}:{sec}." + ticks.ToString(); });
                sec = 0;
            }

            if (min == 59)
            {
                Dispatcher.Invoke(() => { TicksLable1.Content = $"{++hour}:{min}:{sec}." + ticks.ToString(); });
                min = 0;
            }
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            uDelay = 100;    //задержка между вызовами 100 милисекунд = 10fps
            uResolution = 10; //допустимая точность\отклонение\погрежность для uDelay
            ticks = 0;
            timerMethod = new TimerMethod(TimerTick);
            timerHandle = GCHandle.Alloc(timerHandle);
            timerID = TimeSetEvent(uDelay, uResolution, timerMethod , ref dwUser, TIME_PERIODIC);

            if (timerID != 0)
            {
                StopTimer.IsEnabled = true;
                StartTimer.IsEnabled = false;
            }
            else
            {
                timerHandle.Free();
                timerMethod = null!;
            }
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            TimeKillEvent(timerID);
            timerHandle.Free();
            StopTimer.IsEnabled = false;
            StartTimer.IsEnabled = true;
        }

        private void StartTimer1_Click(object sender, RoutedEventArgs e)
        {
            uDelay = 10;
            uResolution = 10;
            ticks = 0;
            timerMethod = new TimerMethod(TimerTick2);
            timerHandle = GCHandle.Alloc(timerMethod); // Исправлено
            timerID = TimeSetEvent(uDelay, uResolution, timerMethod, ref dwUser, TIME_PERIODIC);

            if (timerID != 0)
            {
                StopTimer1.IsEnabled = true;
                StartTimer1.IsEnabled = false;
            }
            else
            {
                timerHandle.Free();
                timerMethod = null!;
            }
        }

        private void StopTimer1_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, активен ли таймер
            if (timerID != 0)
            {
                TimeKillEvent(timerID);
                timerID = 0; // Установка ID таймера в 0 при остановке
                timerHandle.Free();
                StopTimer1.IsEnabled = false;
                StartTimer1.IsEnabled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Closing += Window_Closing!;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (timerID != 0)
            {
                TimeKillEvent(timerID);
                timerHandle.Free();
                MessageBox.Show("Вы забыли остановить таймер! Таймер был остановлен автоматически!");
            }
        }

        public DLLWindow()
        {
            InitializeComponent();
        }

        private void PlaySound(uint frequency, uint duration)
        {
            Thread soundThread = new Thread(() => Sound(frequency, duration));
            soundThread.Start();
        }

        private void MsgA_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxA(IntPtr.Zero, "Message A", "Title", 1);
        }

        private void MsgW_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxW(IntPtr.Zero, "Message W", "Title", 1);
        }

        private void MsgRetry_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxA(IntPtr.Zero, "Повторить попытку соеденения?", 
                "Соеденение не установлено!", 0x35);
        }

        private void ErrorAlert(String message)
        {
            MessageBoxW(IntPtr.Zero, message, null!, 0x10);
        }

        private void MsgError_Click(object sender, RoutedEventArgs e)
        {
            ErrorAlert("Ошибка!");
        }

        private void MsgSpam_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                ErrorAlert("Ошибка!");
            }
        }

        private bool? ConfirmMessage(String message)
        {
            int result = MessageBoxW(IntPtr.Zero, message, "", 0x46);
            return result switch
            {
                11 => true, // Continue
                10 => false, // Retry
                _ => null // Cansel (res = 2)
            };
        }

        private bool Ask(String message)
        {
            int result = MessageBoxW(IntPtr.Zero, message, "", 0x24);
            if (result == 6)
            {
                MessageBox.Show("Действие подтверждено!");
                return true;
            }
            else if (result == 7)
            {
                MessageBox.Show("Действие отменено!");
                return false;
            }
            return false;
        }

        private void MsgConfirm_Click(object sender, RoutedEventArgs e)
        {
            ConfirmMessage("Процесс занимает много времени!");
        }

        private void MsgQuestion_Click(object sender, RoutedEventArgs e)
        {
            Ask("Подтверждаете действие?");
        }

        private void Beep1_Click(object sender, RoutedEventArgs e)
        {
            PlaySound(420, 250);
        }

        private void Beep2_Click(object sender, RoutedEventArgs e)
        {
            PlaySound(350, 100);
        }

        private void Beep3_Click(object sender, RoutedEventArgs e)
        {
            PlaySound(500, 150);
        }

        GCHandle sayHelloHandle;
        private void SayHelloButton_Click(object sender, RoutedEventArgs e)
        {
            // CreateThread(IntPtr.Zero, 0, SayHello, IntPtr.Zero, 0, IntPtr.Zero);
            // Потенциальная проблема - сборщик мусора. При работе он дефрагментирует
            //  память, перенося объекты между поколениями
            // [.][..][.][.x.][..][.] ==> [.][..][.]     [..][.] ==> [.][..][.][..][.]
            //                                                                 эти два
            // объекта поменяют свой адрес в памяти
            // Необходимо "сказать" сборщику мусора о том, что объект не нужно перемещать
            // Для того чтобы не "фиксировать" целое окно, отделим метод в новый объект
            var sayHelloObject = new ThreadMethod(SayHello);
            // и укажем сборщику мусора (GC) разместить этот объект на постоянном месте
            sayHelloHandle = GCHandle.Alloc(sayHelloObject);
            // передаем в неуправляемый код ссылку на объект sayHelloObject
            CreateThread(IntPtr.Zero, 0, sayHelloObject, IntPtr.Zero, 0, IntPtr.Zero);
            // долго удерживать объекты на одном месте нежелательно, после использования
            // нужно их "расфиксировать" - см. SayHello()
        }
    }
}
