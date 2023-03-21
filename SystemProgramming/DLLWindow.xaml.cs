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
            IntPtr hWnd,
            String lpText,
            String lpCaption,
            uint uType
            );
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern
           int MessageBoxW(
           IntPtr hWnd,
           String lpText,
           String lpCaption,
           uint uType
           );

        [DllImport("Kernel32.dll", EntryPoint = "Beep")]
        public static extern bool Sound(uint dwFreq, uint dwDuration);
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
    }
}
