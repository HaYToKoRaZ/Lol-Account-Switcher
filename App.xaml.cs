using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace LolAccountSwitcher
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? _singleInstanceMutex;
        private static EventWaitHandle? _wakeupEvent;
        private static volatile bool _keepListening = true;
        private static readonly CancellationTokenSource _ipcCancelSource = new();
        private const string MutexName = "Local\\LolAccountSwitcher_SingleInstance_Mutex_2026";
        private const string EventName = "Local\\LolAccountSwitcher_Wakeup_Event_2026";
        private const string PipeName = "LolAccountSwitcher_Ipc_Pipe_2026";

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        protected override void OnStartup(StartupEventArgs e)
        {
            _singleInstanceMutex = new Mutex(true, MutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                // Zaten çalışan bir kopya var. Komut satırı argümanı varsa IPC Named Pipe ile ilet
                if (e.Args != null && e.Args.Length > 0)
                {
                    bool sent = SendArgumentsToRunningInstance(string.Join(" ", e.Args));
                    if (!sent)
                    {
                        BringExistingInstanceToFront();
                    }
                }
                else
                {
                    BringExistingInstanceToFront();
                }

                Shutdown();
                return;
            }

            // IPC Named Pipe dinleyicisini başlat (JumpList ve dış komutlar için)
            StartIpcServer();

            // Wakeup event dinleyicisini başlat (Pencereyi öne getirmek için)
            StartWakeupListener();

            try
            {
                base.OnStartup(e);

                // 1. Yapılandırma dosyasını (config.ini) yükle
                ConfigManager.Load();

                // 2. Dil tercihini (TR / EN) yükle
                LocalizationManager.Initialize();

                // 3. Tema tercihini (Sistem / Koyu / Açık) yükle ve uygula
                ThemeManager.InitializeTheme();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_error.log"), ex.ToString());
                MessageBox.Show(ex.ToString(), "Startup Error");
            }
        }

        private static bool SendArgumentsToRunningInstance(string commandLine)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(600);
                using var writer = new StreamWriter(client) { AutoFlush = true };
                writer.WriteLine(commandLine);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void StartIpcServer()
        {
            Task.Run(async () =>
            {
                var token = _ipcCancelSource.Token;
                while (_keepListening && !token.IsCancellationRequested)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(
                            PipeName,
                            PipeDirection.In,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous);

                        await server.WaitForConnectionAsync(token).ConfigureAwait(false);

                        if (!_keepListening || token.IsCancellationRequested) break;

                        using var reader = new StreamReader(server);
                        string? line = await reader.ReadLineAsync(token).ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            Current?.Dispatcher?.InvokeAsync(() =>
                            {
                                HandleCommandLine(line);
                            });
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        if (!_keepListening || token.IsCancellationRequested) break;
                        try { await Task.Delay(50, token).ConfigureAwait(false); } catch { break; }
                    }
                }
            });
        }

        private static void StartWakeupListener()
        {
            try
            {
                _wakeupEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);
                var listenerThread = new Thread(() =>
                {
                    while (_keepListening)
                    {
                        try
                        {
                            if (_wakeupEvent.WaitOne())
                            {
                                if (!_keepListening) break;
                                Current?.Dispatcher?.Invoke(() =>
                                {
                                    TrayService.ShowMainWindow();
                                });
                            }
                        }
                        catch { break; }
                    }
                })
                {
                    IsBackground = true
                };
                listenerThread.Start();
            }
            catch { }
        }

        public static void HandleCommandLine(string commandLine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commandLine)) return;

                var mainWin = Current?.MainWindow as MainWindow;

                if (commandLine.IndexOf("--kill-riot", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mainWin?.Dispatcher?.Invoke(() =>
                    {
                        mainWin.TriggerKillLeague();
                    });
                    return;
                }

                if (commandLine.IndexOf("--launch-riot", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mainWin?.Dispatcher?.Invoke(() =>
                    {
                        mainWin.TriggerLaunchLeague();
                    });
                    return;
                }

                const string switchKey = "--switch-account";
                int idx = commandLine.IndexOf(switchKey, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string accountId = commandLine.Substring(idx + switchKey.Length).Trim();
                    accountId = accountId.Trim('"', '\'');
                    if (!string.IsNullOrWhiteSpace(accountId))
                    {
                        mainWin?.Dispatcher?.InvokeAsync(async () =>
                        {
                            await mainWin.SwitchToAccountById(accountId);
                        });
                        return;
                    }
                }

                // Tanınmayan komut veya boş tıklama durumunda pencereyi öne getir
                mainWin?.Dispatcher?.Invoke(() =>
                {
                    TrayService.ShowMainWindow();
                });
            }
            catch { }
        }

        private static void BringExistingInstanceToFront()
        {
            try
            {
                using var ev = EventWaitHandle.OpenExisting(EventName);
                ev.Set();
                return;
            }
            catch { }

            try
            {
                int currentPid = Process.GetCurrentProcess().Id;
                Process[] processes = Process.GetProcessesByName("LolAccountSwitcher");
                foreach (var proc in processes)
                {
                    if (proc.Id != currentPid && proc.MainWindowHandle != IntPtr.Zero)
                    {
                        IntPtr hWnd = proc.MainWindowHandle;
                        if (IsIconic(hWnd))
                        {
                            ShowWindowAsync(hWnd, SW_RESTORE);
                        }
                        else
                        {
                            ShowWindowAsync(hWnd, SW_SHOW);
                        }
                        SetForegroundWindow(hWnd);
                        break;
                    }
                }
            }
            catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _keepListening = false;
                _ipcCancelSource.Cancel();
                _wakeupEvent?.Set();
                _wakeupEvent?.Dispose();

                // Wake up any blocked NamedPipeServerStream
                try
                {
                    using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                    client.Connect(50);
                }
                catch { }

                _singleInstanceMutex?.ReleaseMutex();
                _singleInstanceMutex?.Dispose();
            }
            catch { }
            base.OnExit(e);
        }
    }
}
