using Raylib_cs;

namespace CircleDefenseGame;

internal static class Program
{
    private const int GridSize = 100;
    private const int CellSize = 10;
    private const int CircleRadiusInSquares = 30;
    private const int WindowSize = GridSize * CellSize;

    [STAThread]
    private static void Main()
    {
        Color[,] gridColors = CreateGridColors();

        Raylib.InitWindow(WindowSize, WindowSize, "Circle Defense Game");
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            DrawGrid(gridColors);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    private static Color[,] CreateGridColors()
    {
        var random = new Random();
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
