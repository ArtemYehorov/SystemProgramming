using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
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
    /// Логика взаимодействия для HookWindow.xaml
    /// </summary>
    public partial class HookWindow : Window
    {
        #region Api and DLL
        //Сигнатура хук-процедуры: нашего обработчика системных событий 
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        //установшик хука - вызов функции встроит наши обработчики в систему виндовс 
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook,  HookProc lpfn, IntPtr hMod, uint dwThreadId);

        //Отмена хука - искелючение нашего обработчика из системы
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);

        //Передача управления следующему хуку(перед которым мы строили свой код)
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);


        //Определение адресса модуля - исполнимого кода нашей программы
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(String? lpModuleName);

        private const int 
            WH_KEYBOARD_LL = 13, // Номер ячейки прирывания клавиатуры 
            WM_KEYDOWN = 0x0100; // Номер сообщения нажатия кнопки

        #endregion

        private IntPtr kbHook; // Созраненный адресс старого обработчика
        private HookProc kbHookProc; // Объект который будет зафикисирован от перемещений 
        private GCHandle kbHookHandle; //дискриптор зафиксированного объекта

        [StructLayout(LayoutKind.Sequential)]
        struct KBDLLHOOKSTRUCT
        {
            public int vcCode;
            public int ScanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        private IntPtr kbHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam); // Маршализация - передача данных (IntPtr) из адресса (lParam) в управляемый код (int vkCode)

                KBDLLHOOKSTRUCT keyData = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam); //ПОЛНАЯ ВЕРСИЯ МАРШАЛИЗАЦИИ СТРУКТУРЫ , краткая орма с ReadInt32(lParam) возможна потому, что vkCode идёт первым по структуре , то есть считать 32 бита от начала струкетуры , это и есть считать vkCode

                Key key = KeyInterop.KeyFromVirtualKey(vkCode);
                if (key == Key.LWin)
                {
                    HookLogs.Text += "(block)";
                    return (IntPtr)1;
                }
                if (key == Key.Enter)
                {
                    HookLogs.Text += "\n";
                   
                }
                HookLogs.Text += key;
                
            }
            return CallNextHookEx(kbHook, nCode, wParam, lParam);
        }

        public HookWindow()
        {
            InitializeComponent();
            StopkbHook.IsEnabled = false;
        }

        private void StartkbHook_Click(object sender, RoutedEventArgs e)
        {
            StartkbHook.IsEnabled = false;
            StopkbHook.IsEnabled = true;
            using Process thisProcess = Process.GetCurrentProcess();
            using ProcessModule? thisModule = thisProcess.MainModule;
            if (thisModule == null)
            {
                HookLogs.Text += "Error in MainModule\n";
                return;          
            }

            kbHookProc = new HookProc(kbHookCallback); //отделяем метод от окна в новый объект 
            kbHookHandle = GCHandle.Alloc(kbHookProc); //Закрепляем - GC - не будте перемещать объект

            kbHook = SetWindowsHookEx(                 // Принцип выталкивания - новый адрес устанавливаеться а старый возвращаеться 
                WH_KEYBOARD_LL,                        //
                kbHookProc,                        // и сохраняется в kbHook
                GetModuleHandle(thisModule.ModuleName),
                0);
            HookLogs.Text += "Hook Activated\n";
        }

        private void StopkbHook_Click(object sender, RoutedEventArgs e)
        {
            UnhookWindowsHookEx(kbHook);
            kbHookHandle.Free();
            kbHookProc = null!;
            StartkbHook.IsEnabled = true;
            StopkbHook.IsEnabled = false;
            HookLogs.Text += "Hook Deactivated\n";
        }
    }
}
