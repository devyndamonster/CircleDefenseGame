using Raylib_cs;
using Color = Raylib_cs.Color;

namespace CircleDefenseGame.Game.Renderables
{
    public class Tile : IRenderable
    {
        public required int X { get; set; }

        public required int Y { get; set; }

        public required int Size { get; set; }

        public required Color Color { get; set; }

        public void Render()
        {
            Raylib.DrawRectangle(X, Y, Size, Size, Color);
        }
    }
}
