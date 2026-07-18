using Raylib_cs;

namespace CircleDefenseGame.Game.Behaviours
{
    public class InputManagement : IBehaviour
    {
        public void Tick(GameManager game)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                return;
            }

            var mousePosition = Raylib.GetMousePosition();
            ColorTileAt((int)mousePosition.X, (int)mousePosition.Y, Color.Blue, game);
        }

        private void ColorTileAt(int x, int y, Color color, GameManager game)
        {
            if (x < 0 || y < 0)
            {
                return;
            }

            int row = y / game.GameSettings.TileSize;
            int column = x / game.GameSettings.TileSize;

            if (game.Tiles.TryGetValue(row, out var tileRow) && tileRow.TryGetValue(column, out var tile))
            {
                tile.Color = color;
            }
        }
    }
}
