using System.Globalization;
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
    private const double RedTileIntervalSeconds = 1;

    [STAThread]
    private static void Main(string[] args)
    {
        ScreenshotRequest? screenshotRequest = GetScreenshotRequest(args);
        var random = new Random(RandomSeed);
        Color[,] gridColors = CreateGridColors(random);

        Raylib.InitWindow(WindowSize, WindowSize, "Circle Defense Game");
        Raylib.SetTargetFPS(60);

        int renderedFrames = 0;
        double nextRedTileTime = RedTileIntervalSeconds;

        while (!Raylib.WindowShouldClose())
        {
            while (Raylib.GetTime() >= nextRedTileTime)
            {
                TurnRandomTileRed(gridColors, random);
                nextRedTileTime += RedTileIntervalSeconds;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            DrawGrid(gridColors);

            Raylib.EndDrawing();
            renderedFrames++;

            if (screenshotRequest is not null
                && (screenshotRequest.CaptureAfterSeconds is null
                    ? renderedFrames == ScreenshotFrameCount
                    : Raylib.GetTime() >= screenshotRequest.CaptureAfterSeconds))
            {
                Raylib.TakeScreenshot(screenshotRequest.Path);
                break;
            }
        }

        Raylib.CloseWindow();
    }

    private static ScreenshotRequest? GetScreenshotRequest(string[] args)
    {
        if (args.Length == 0)
        {
            return null;
        }

        if ((args.Length != 2 && args.Length != 4)
            || args[0] != "--screenshot"
            || string.IsNullOrWhiteSpace(args[1])
            || (args.Length == 4 && args[2] != "--screenshot-after-seconds"))
        {
            throw new ArgumentException(
                "Usage: CircleDefenseGame [--screenshot <path> [--screenshot-after-seconds <seconds>]]");
        }

        string screenshotPath = Path.GetFullPath(args[1]);
        string screenshotDirectory = Path.GetDirectoryName(screenshotPath)
            ?? throw new InvalidOperationException("The screenshot path does not include a directory.");

        Directory.CreateDirectory(screenshotDirectory);
        Directory.SetCurrentDirectory(screenshotDirectory);

        if (args.Length == 2)
        {
            return new ScreenshotRequest(Path.GetFileName(screenshotPath), null);
        }

        if (!double.TryParse(
                args[3],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double captureAfterSeconds)
            || captureAfterSeconds <= 0)
        {
            throw new ArgumentException("The screenshot capture delay must be a positive number of seconds.");
        }

        return new ScreenshotRequest(Path.GetFileName(screenshotPath), captureAfterSeconds);
    }

    private static Color[,] CreateGridColors(Random random)
    {
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

    private static void TurnRandomTileRed(Color[,] gridColors, Random random)
    {
        int row = random.Next(GridSize);
        int column = random.Next(GridSize);
        gridColors[row, column] = Color.Red;
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

    private sealed record ScreenshotRequest(string Path, double? CaptureAfterSeconds);
}
