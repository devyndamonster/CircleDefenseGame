using Raylib_cs;

namespace CircleDefenseGame;

internal static class Program
{
    private const int GridSize = 100;
    private const int CellSize = 10;
    private const int CircleRadiusInSquares = 30;
    private const int WindowSize = GridSize * CellSize;
    private const int RandomSeed = 12343;
    private const int ScreenshotFrameCount = 2;

    [STAThread]
    private static void Main(string[] args)
    {
        string? screenshotPath = GetScreenshotPath(args);
        Color[,] gridColors = CreateGridColors();

        Raylib.InitWindow(WindowSize, WindowSize, "Circle Defense Game");
        Raylib.SetTargetFPS(60);

        int renderedFrames = 0;

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            DrawGrid(gridColors);

            Raylib.EndDrawing();
            renderedFrames++;

            if (screenshotPath is not null && renderedFrames == ScreenshotFrameCount)
            {
                Raylib.TakeScreenshot(screenshotPath);
                break;
            }
        }

        Raylib.CloseWindow();
    }

    private static string? GetScreenshotPath(string[] args)
    {
        if (args.Length == 0)
        {
            return null;
        }

        if (args.Length != 2 || args[0] != "--screenshot" || string.IsNullOrWhiteSpace(args[1]))
        {
            throw new ArgumentException("Usage: CircleDefenseGame [--screenshot <path>]");
        }

        string screenshotPath = Path.GetFullPath(args[1]);
        string screenshotDirectory = Path.GetDirectoryName(screenshotPath)
            ?? throw new InvalidOperationException("The screenshot path does not include a directory.");

        Directory.CreateDirectory(screenshotDirectory);
        Directory.SetCurrentDirectory(screenshotDirectory);

        return Path.GetFileName(screenshotPath);
    }

    private static Color[,] CreateGridColors()
    {
        var random = new Random(RandomSeed);
        var colors = new Color[GridSize, GridSize];

        for (int row = 0; row < GridSize; row++)
        {
            for (int column = 0; column < GridSize; column++)
            {
                colors[row, column] = IsInsideCenterCircle(row, column)
                    ? Color.White
                    : new Color(
                        (byte)random.Next(256),
                        (byte)random.Next(256),
                        (byte)random.Next(256),
                        byte.MaxValue);
            }
        }

        return colors;
    }

    private static bool IsInsideCenterCircle(int row, int column)
    {
        double center = GridSize / 2.0;
        double horizontalDistance = column + 0.5 - center;
        double verticalDistance = row + 0.5 - center;

        return horizontalDistance * horizontalDistance + verticalDistance * verticalDistance
            <= CircleRadiusInSquares * CircleRadiusInSquares;
    }

    private static void DrawGrid(Color[,] gridColors)
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int column = 0; column < GridSize; column++)
            {
                Raylib.DrawRectangle(
                    column * CellSize,
                    row * CellSize,
                    CellSize,
                    CellSize,
                    gridColors[row, column]);
            }
        }
    }
}
