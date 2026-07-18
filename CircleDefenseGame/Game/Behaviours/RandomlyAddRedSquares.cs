using Raylib_cs;

namespace CircleDefenseGame.Game.Behaviours
{
    public class RandomlyAddRedSquares : IBehaviour
    {
        private const double RedTileIntervalSeconds = 1;

        private double nextRedTileTime = RedTileIntervalSeconds;

        public void Tick(GameManager game)
        {
            if (Raylib.GetTime() >= nextRedTileTime)
            {
                TurnRandomTileRed(game);
                nextRedTileTime += RedTileIntervalSeconds;
            }
        }

        private void TurnRandomTileRed(GameManager game)
        {
            int row = game.Random.Next(game.Tiles.Count);
            int column = game.Random.Next(game.Tiles[row].Count);
            game.Tiles[row][column].Color = Color.Red;
        }
    }
}
