using CircleDefenseGame.Tests.TestUtilities;
using CircleDefenseGame.Tests.TestUtilities.Extensions;
using System.Drawing;

namespace CircleDefenseGame.Tests;

public class SnapshotTests
{
    [Test]
    public async Task ItWillRemoveCoin_WhenCoinIsClicked()
    {
        using GameRunner gameRunner = new();
        gameRunner.StartGame();

        await Task.Delay(TimeSpan.FromMilliseconds(1200));

        await Assert.That(gameRunner.Game.Coins.Count).IsGreaterThan(0);
        await Assert.That(gameRunner).MatchesExistingSnapshot("BeforeClick");

        var coin = gameRunner.Game.Coins[0];
        gameRunner.ClickGameWindow(new Point(coin.X, coin.Y));

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        await Assert.That(gameRunner.Game.Coins.Contains(coin)).IsFalse();
        await Assert.That(gameRunner).MatchesExistingSnapshot("AfterClick");
    }

    [Test]
    public async Task ItWillDrawASmileyFace_WhenDrawnWithMouseClicks()
    {
        using GameRunner gameRunner = new();
        gameRunner.StartGame();

        Point[] smileyFace =
        [
            new(425, 425), new(435, 425),
            new(425, 435), new(435, 435),
            new(555, 425), new(565, 425),
            new(555, 435), new(565, 435),
            new(455, 535), new(545, 535),
            new(455, 545), new(545, 545),
            new(465, 555), new(535, 555),
            new(475, 565), new(525, 565),
            new(485, 575), new(495, 575),
            new(505, 575), new(515, 575)
        ];

        foreach (Point point in smileyFace)
        {
            gameRunner.ClickGameWindow(point);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        await Assert.That(gameRunner).MatchesExistingSnapshot("SmileyFaceAfterClicks");
    }

    [Test]
    public async Task ItWillDisplayInitialScreen()
    {
        using GameRunner gameRunner = new();
        gameRunner.StartGame();

        await Assert.That(gameRunner).MatchesExistingSnapshot("InitialScreen");
    }

    [Test]
    public async Task ItWillDrawCoinsRandomly_AfterTimeHasPassed()
    {
        using GameRunner gameRunner = new();
        gameRunner.StartGame();

        await Task.Delay(TimeSpan.FromSeconds(3));

        await Assert.That(gameRunner).MatchesExistingSnapshot("CoinsAfterThreeSeconds");
        await Assert.That(gameRunner.Game.Coins.Count).IsGreaterThan(0);
    }


}
