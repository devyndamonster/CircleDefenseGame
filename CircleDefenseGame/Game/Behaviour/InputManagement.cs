using CircleDefenseGame.Game.Interaction;
using Raylib_cs;

namespace CircleDefenseGame.Game.Behaviour
{
    public class InputManagement : IBehaviour
    {
        private bool isMouseDown = false;

        private List<IInteractable> activeInteractions = [];

        public void Tick(GameManager game)
        {
            var mousePosition = Raylib.GetMousePosition();
            int x = (int)mousePosition.X;
            int y = (int)mousePosition.Y;

            if (isMouseDown && !Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                MouseUpAt(x, y, game);
            }
            else if (!isMouseDown && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                MouseDownAt(x, y, game);
            }

            isMouseDown = Raylib.IsMouseButtonPressed(MouseButton.Left);
        }

        private void MouseDownAt(int x, int y, GameManager game)
        {
            foreach (var coin in game.Coins)
            {
                if (coin.IsMouseOver(x, y))
                {
                    coin.MouseDown(game);
                    activeInteractions.Add(coin);
                    break;
                }
            }

            ColorTileAt(x, y, Color.Blue, game);
        }

        private void MouseUpAt(int x, int y, GameManager game)
        {
            foreach (var interactable in activeInteractions)
            {
                interactable.MouseUp(game);
            }

            activeInteractions.Clear();
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
