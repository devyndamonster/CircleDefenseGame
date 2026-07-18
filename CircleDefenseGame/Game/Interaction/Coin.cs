using CircleDefenseGame.Game.Rendering;
using Raylib_cs;

namespace CircleDefenseGame.Game.Interaction
{
    public class Coin : IInteractable, IRenderable
    {
        public required int X { get; set; }

        public required int Y { get; set; }

        public required int Radius { get; set; }

        public bool IsMouseOver(int mouseX, int mouseY)
        {
            return Math.Sqrt(Math.Pow(mouseX - X, 2) + Math.Pow(mouseY - Y, 2)) <= Radius;
        }

        public void MouseDown(GameManager game)
        {
        }

        public void MouseUp(GameManager game)
        {
            DeleteCoin(game);
        }

        private void DeleteCoin(GameManager game)
        {
            game.Coins.Remove(this);
        }

        public void Render()
        {
            Color shadowColor = new(184, 134, 11, 255);
            const int centSignFontSize = 20;
            const string centSign = "¢";

            Raylib.DrawCircle(X, Y, Radius, Color.Gold);
            Raylib.DrawCircleLines(X, Y, Radius, shadowColor);

            int textWidth = Raylib.MeasureText(centSign, centSignFontSize);
            Raylib.DrawText(centSign, X - textWidth / 2, Y - centSignFontSize / 2, centSignFontSize, shadowColor);
        }
    }
}
