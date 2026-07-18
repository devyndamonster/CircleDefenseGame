using CircleDefenseGame.Game;
using Raylib_cs;

namespace CircleDefenseGame;

internal static class Program
{
    private static GameManager Game = null!;
    private static int WindowWidth;
    private static int WindowHeight;

    [STAThread]
    private static void Main()
    {
        var settings = new GameSettings
        {
            GameSeed = 12343,
            GridHeight = 100,
            GridWidth = 100,
            TileSize = 10
        };

        Game = new GameManager(settings);

        WindowWidth = settings.GridWidth * settings.TileSize;
        WindowHeight = settings.GridHeight * settings.TileSize;

        Raylib.InitWindow(WindowWidth, WindowHeight, "Circle Defense Game");
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Game.Tick();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            foreach (var renderable in Game.GetRenderables())
            {
                renderable.Render();
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
