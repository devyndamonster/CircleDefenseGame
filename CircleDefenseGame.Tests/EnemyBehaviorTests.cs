using CircleDefenseGame.Game;
using CircleDefenseGame.Game.Behaviour;
using CircleDefenseGame.Game.Enemies;

namespace CircleDefenseGame.Tests;

public class EnemyBehaviorTests
{
    private static readonly GameSettings Settings = new()
    {
        GameSeed = 12343,
        GridHeight = 100,
        GridWidth = 100,
        TileSize = 10
    };

    [Test]
    public async Task ItWillMoveAnEnemyTowardTheScreenCenter()
    {
        var game = new GameManager(Settings);
        var enemy = new Enemy { X = -9, Y = 500 };
        game.Enemies.Add(enemy);

        new UpdateEnemies().Tick(game);

        await Assert.That(enemy.X).IsEqualTo(-7);
        await Assert.That(enemy.Y).IsEqualTo(500);
    }
}
