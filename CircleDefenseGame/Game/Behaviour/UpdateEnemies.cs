namespace CircleDefenseGame.Game.Behaviour
{
    public class UpdateEnemies : IBehaviour
    {
        private const float EnemySpeedPerFrame = 2f;

        public void Tick(GameManager game)
        {
            int centerX = game.GameSettings.GridWidth * game.GameSettings.TileSize / 2;
            int centerY = game.GameSettings.GridHeight * game.GameSettings.TileSize / 2;

            foreach (var enemy in game.Enemies)
            {
                enemy.MoveTowards(centerX, centerY, EnemySpeedPerFrame);
            }
        }
    }
}
