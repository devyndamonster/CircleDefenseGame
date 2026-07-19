using CircleDefenseGame.Game.Enemies;

namespace CircleDefenseGame.Game.Behaviour
{
    public class RandomlySpawnEnemies : IBehaviour
    {
        private const int SpawnIntervalFrames = 300;
        private const int EnemyRadius = 8;

        private int framesUntilSpawn = SpawnIntervalFrames;

        public void Tick(GameManager game)
        {
            if (--framesUntilSpawn > 0)
            {
                return;
            }

            SpawnEnemy(game);
            framesUntilSpawn = SpawnIntervalFrames;
        }

        private static void SpawnEnemy(GameManager game)
        {
            int screenWidth = game.GameSettings.GridWidth * game.GameSettings.TileSize;
            int screenHeight = game.GameSettings.GridHeight * game.GameSettings.TileSize;
            int offScreenPosition = EnemyRadius + 1;
            var enemy = new Enemy { Radius = EnemyRadius };

            switch (game.Random.Next(4))
            {
                case 0:
                    enemy.X = game.Random.Next(screenWidth);
                    enemy.Y = -offScreenPosition;
                    break;
                case 1:
                    enemy.X = screenWidth + offScreenPosition;
                    enemy.Y = game.Random.Next(screenHeight);
                    break;
                case 2:
                    enemy.X = game.Random.Next(screenWidth);
                    enemy.Y = screenHeight + offScreenPosition;
                    break;
                default:
                    enemy.X = -offScreenPosition;
                    enemy.Y = game.Random.Next(screenHeight);
                    break;
            }

            game.Enemies.Add(enemy);
        }
    }
}
