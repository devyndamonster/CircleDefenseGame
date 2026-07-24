using CircleDefenseGame.Game.Rendering;
using Raylib_cs;

namespace CircleDefenseGame.Game.Players
{
    public class Player : IEntity, IRenderable
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Radius { get; init; } = 8;

        public void Render()
        {
            Raylib.DrawCircle(X, Y, Radius, Color.Blue);
        }

        public void MoveDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    X -= 1;
                    break;
                case Direction.Right:
                    X += 1;
                    break;
                case Direction.Up:
                    Y -= 1;
                    break;
                case Direction.Down:
                    Y += 1;
                    break;
            }
        }
    }
}
