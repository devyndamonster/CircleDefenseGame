using System.Diagnostics;

namespace CircleDefenseGame.Tests;

internal sealed class GameRunner : IDisposable
{
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
}
