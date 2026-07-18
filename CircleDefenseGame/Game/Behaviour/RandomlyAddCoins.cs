using CircleDefenseGame.Game.Interaction;

namespace CircleDefenseGame.Game.Behaviour
{
    public class RandomlyAddCoins : IBehaviour
    {
        private const double CoinIntervalSeconds = 1;

        private double nextCoinTime = CoinIntervalSeconds;

        public void Tick(GameManager game)
        {
            if (Raylib_cs.Raylib.GetTime() >= nextCoinTime)
            {
                AddCoinAtRandomTile(game);
                nextCoinTime += CoinIntervalSeconds;
            }
        }

        private void AddCoinAtRandomTile(GameManager game)
        {
            int row = game.Random.Next(game.Tiles.Count);
            int column = game.Random.Next(game.Tiles[row].Count);
            int tileSize = game.GameSettings.TileSize;

            game.Coins.Add(new Coin
            {
                X = column * tileSize + tileSize / 2,
                Y = row * tileSize + tileSize / 2,
                Radius = tileSize
            });
        }
    }
}
