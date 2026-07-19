using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using CircleDefenseGame.Game;

namespace CircleDefenseGame.Tests.TestUtilities;

internal sealed class GameRunner : IDisposable
{
    private const uint InputMouse = 0;
    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;
    private static readonly SemaphoreSlim GameLock = new(1, 1);
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan WindowReadyDelay = TimeSpan.FromMilliseconds(200);

    private readonly object sync = new();
    private CancellationTokenSource? cancellationSource;
    private Thread? gameThread;
    private bool holdsGameLock;
    private Exception? gameException;

    public GameManager Game { get; private set; } = null!;

    public IntPtr WindowHandle { get; private set; }

    public GameManager StartGame()
    {
        DisposeCurrentGame();
        GameLock.Wait();

        lock (sync)
        {
            holdsGameLock = true;
            gameException = null;
        }

        try
        {
            var gameStarted = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            CancellationTokenSource source = new();
            var thread = new Thread(() => RunGame(source, gameStarted))
            {
                IsBackground = true
            };
            thread.SetApartmentState(ApartmentState.STA);

            lock (sync)
            {
                cancellationSource = source;
                gameThread = thread;
            }

            thread.Start();
            if (!gameStarted.Task.Wait(StartupTimeout))
            {
                throw new TimeoutException("The game did not create a window within 10 seconds.");
            }

            gameStarted.Task.GetAwaiter().GetResult();
            ThrowIfGameExited();
            Thread.Sleep(WindowReadyDelay);

            return Game;
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

    public void ClickGameWindow(Point clientPoint)
    {
        ThrowIfGameExited();

        SetProcessDpiAwarenessContext(new IntPtr(-4));

        if (!SetForegroundWindow(WindowHandle))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not activate the game window.");
        }

        var screenPoint = new NativePoint
        {
            X = clientPoint.X,
            Y = clientPoint.Y
        };

        if (!ClientToScreen(WindowHandle, ref screenPoint))
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
        CancellationTokenSource? source;
        Thread? thread;
        bool releaseGameLock;

        lock (sync)
        {
            source = cancellationSource;
            cancellationSource = null;
            thread = gameThread;
            gameThread = null;
            releaseGameLock = holdsGameLock;
            holdsGameLock = false;
        }

        try
        {
            if (source is not null)
            {
                source.Cancel();
            }

            if (thread is not null && !thread.Join(StartupTimeout))
            {
                throw new TimeoutException("The game did not stop within 10 seconds.");
            }
        }
        finally
        {
            source?.Dispose();
            Game = null!;
            WindowHandle = IntPtr.Zero;

            if (releaseGameLock)
            {
                GameLock.Release();
            }
        }
    }

    private void RunGame(CancellationTokenSource source, TaskCompletionSource gameStarted)
    {
        try
        {
            Program.Run(
                new GameSettings
                {
                    GameSeed = 12343,
                    GridHeight = 100,
                    GridWidth = 100,
                    TileSize = 10
                },
                source.Token,
                (game, windowHandle) =>
                {
                    if (windowHandle == IntPtr.Zero)
                    {
                        throw new InvalidOperationException("The game created a window without a handle.");
                    }

                    Game = game;
                    WindowHandle = windowHandle;
                    gameStarted.TrySetResult();
                });
        }
        catch (Exception exception)
        {
            gameException = exception;
            gameStarted.TrySetException(exception);
        }
    }

    private void ThrowIfGameExited()
    {
        if (gameException is not null)
        {
            throw new InvalidOperationException("The game exited unexpectedly.", gameException);
        }

        if (gameThread is null || !gameThread.IsAlive)
        {
            throw new InvalidOperationException("The game exited before it could be used.");
        }
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
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

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
