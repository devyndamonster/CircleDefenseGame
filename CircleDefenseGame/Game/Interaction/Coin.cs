using CircleDefenseGame.Game.Rendering;
using Raylib_cs;

namespace CircleDefenseGame.Game.Interaction
{
    public class Coin : IEntity, IInteractable, IRenderable
    {
        public required int X { get; set; }

        public required int Y { get; set; }

        public required int Radius { get; set; }

        public void MouseDown()
        {
            throw new NotImplementedException();
        }

        public void MouseUp()
        {
            throw new NotImplementedException();
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
