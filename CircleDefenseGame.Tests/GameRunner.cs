using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CircleDefenseGame.Tests;

internal sealed class GameRunner : IDisposable
{
    private const uint InputMouse = 0;
    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;
    private static readonly SemaphoreSlim GameLock = new(1, 1);
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan WindowReadyDelay = TimeSpan.FromMilliseconds(200);

    private readonly object sync = new();
    private Process? currentGame;
    private bool holdsGameLock;

    public Process StartGame()
    {
        DisposeCurrentGame();
        GameLock.Wait();

        lock (sync)
        {
            holdsGameLock = true;
        }

        try
        {
            string gameExecutablePath = Path.Combine(AppContext.BaseDirectory, "CircleDefenseGame.exe");

            if (!File.Exists(gameExecutablePath))
            {
                throw new FileNotFoundException(
                    "The game executable was not copied to the test output directory.",
                    gameExecutablePath);
            }

            var startInfo = new ProcessStartInfo(gameExecutablePath)
            {
                UseShellExecute = false,
                WorkingDirectory = AppContext.BaseDirectory,
            };
            var game = Process.Start(startInfo)
                ?? throw new InvalidOperationException("The game process could not be started.");

            lock (sync)
            {
                currentGame = game;
            }

            WaitForWindow(game);
            Thread.Sleep(WindowReadyDelay);

            return Process.GetProcessById(game.Id);
        }
        catch
        {
            DisposeCurrentGame();
            throw;
        }
    }

    public void Dispose()
    {
        DisposeCurrentGame();
        GC.SuppressFinalize(this);
    }

    public void ClickGameWindow(Process game, Point clientPoint)
    {
        if (game.HasExited)
        {
            throw new InvalidOperationException("The game exited before it could receive a click.");
        }

        SetProcessDpiAwarenessContext(new IntPtr(-4));

        var screenPoint = new NativePoint
        {
            X = clientPoint.X,
            Y = clientPoint.Y
        };

        if (!ClientToScreen(game.MainWindowHandle, ref screenPoint))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not locate the game client area.");
        }

        if (!SetCursorPos(screenPoint.X, screenPoint.Y))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not move the cursor to the game window.");
        }

        Thread.Sleep(TimeSpan.FromMilliseconds(20));
        SendMouseInput(MouseEventLeftDown);
        Thread.Sleep(TimeSpan.FromMilliseconds(30));
        SendMouseInput(MouseEventLeftUp);
        Thread.Sleep(TimeSpan.FromMilliseconds(20));
    }

    private void DisposeCurrentGame()
    {
        Process? game;
        bool releaseGameLock;

        lock (sync)
        {
            game = currentGame;
            currentGame = null;
            releaseGameLock = holdsGameLock;
            holdsGameLock = false;
        }

        try
        {
            if (game is not null && !game.HasExited)
            {
                game.Kill(entireProcessTree: true);
                game.WaitForExit();
            }
        }
        finally
        {
            game?.Dispose();

            if (releaseGameLock)
            {
                GameLock.Release();
            }
        }
    }

    private void WaitForWindow(Process game)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < StartupTimeout)
        {
            game.Refresh();

            if (game.MainWindowHandle != IntPtr.Zero)
            {
                return;
            }

            if (game.HasExited)
            {
                throw new InvalidOperationException(
                    $"The game exited with code {game.ExitCode} before creating its window.");
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(50));
        }

        throw new TimeoutException("The game did not create a window within 10 seconds.");
    }

    private static void SendMouseInput(uint flags)
    {
        Input[] inputs =
        [
            new Input { Type = InputMouse, MouseInput = new MouseInput { Flags = flags } }
        ];

        if (SendInput(1, inputs, Marshal.SizeOf<Input>()) != 1)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not send a click to the game window.");
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClientToScreen(IntPtr windowHandle, ref NativePoint point);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiAwarenessContext);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public MouseInput MouseInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }
}
