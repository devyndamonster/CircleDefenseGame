using CircleDefenseGame.Game;
using Raylib_cs;

namespace CircleDefenseGame;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        Run(CreateDefaultSettings(), CancellationToken.None, null);
    }

    public static void Run(
        GameSettings settings,
        CancellationToken cancellationToken,
        Action<GameManager, IntPtr>? gameStarted)
    {
        var game = new GameManager(settings);
        int windowWidth = settings.GridWidth * settings.TileSize;
        int windowHeight = settings.GridHeight * settings.TileSize;

        Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow);
        Raylib.InitWindow(windowWidth, windowHeight, "Circle Defense Game");
        Raylib.SetTargetFPS(60);

        try
        {
            gameStarted?.Invoke(game, GetWindowHandle());

            while (!cancellationToken.IsCancellationRequested && !Raylib.WindowShouldClose())
            {
                game.Tick();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                foreach (var renderable in game.GetRenderables())
                {
                    renderable.Render();
                }

                Raylib.EndDrawing();
            }
        }
        finally
        {
            Raylib.CloseWindow();
        }
    }

    private static GameSettings CreateDefaultSettings()
    {
        return new GameSettings
        {
            GameSeed = 12343,
            GridHeight = 100,
            GridWidth = 100,
            TileSize = 10
        };
    }

    private static unsafe IntPtr GetWindowHandle()
    {
        return new IntPtr(Raylib.GetWindowHandle());
    }
}
