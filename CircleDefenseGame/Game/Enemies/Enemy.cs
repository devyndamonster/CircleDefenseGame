using CircleDefenseGame.Game.Rendering;
using Raylib_cs;
using System.Numerics;

namespace CircleDefenseGame.Game.Enemies
{
    public class Enemy : IRenderable, IEntity
    {
        private Vector2 position;

        public int X
        {
            get => (int)MathF.Round(position.X);
            set => position.X = value;
        }

        public int Y
        {
            get => (int)MathF.Round(position.Y);
            set => position.Y = value;
        }

        public int Radius { get; init; } = 8;

        public void MoveTowards(int targetX, int targetY, float distance)
        {
            Vector2 target = new(targetX, targetY);
            Vector2 direction = target - position;
            float remainingDistance = direction.Length();

            if (remainingDistance == 0)
            {
                return;
            }

            position += direction / remainingDistance * MathF.Min(distance, remainingDistance);
        }

        public void Render()
        {
            Raylib.DrawCircle(X, Y, Radius, Color.Red);
        }
    }
}
