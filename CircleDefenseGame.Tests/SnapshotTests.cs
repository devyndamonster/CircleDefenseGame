using CircleDefenseGame.Tests.TestUtilities;
using CircleDefenseGame.Tests.TestUtilities.Extensions;
using System.Drawing;

namespace CircleDefenseGame.Tests;

[ClassDataSource<SnapshotTestFixture>(Shared = SharedType.None)]
public class SnapshotTests(SnapshotTestFixture Fixture)
{
    [Test]

    public async Task ItWillRemoveCoin_WhenCoinIsClicked()
    {
        Fixture.GameRunner.StartGame();

        await Task.Delay(TimeSpan.FromMilliseconds(1200));

        await Assert.That(Fixture.GameRunner.Game.Coins.Count).IsGreaterThan(0);
        await Assert.That(Fixture.GameRunner).MatchesExistingSnapshot("BeforeClick");

        var coin = Fixture.GameRunner.Game.Coins[0];
        Fixture.GameRunner.ClickGameWindow(new Point(coin.X, coin.Y));

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        await Assert.That(Fixture.GameRunner.Game.Coins.Contains(coin)).IsFalse();
        await Assert.That(Fixture.GameRunner).MatchesExistingSnapshot("AfterClick");
    }

    [Test]
    public async Task ItWillDrawASmileyFace_WhenDrawnWithMouseClicks()
    {
        Fixture.GameRunner.StartGame();

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
            Fixture.GameRunner.ClickGameWindow(point);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        await Assert.That(Fixture.GameRunner).MatchesExistingSnapshot("SmileyFaceAfterClicks");
    }

    [Test]
    public async Task ItWillDisplayInitialScreen()
    {
        Fixture.GameRunner.StartGame();

        await Assert.That(Fixture.GameRunner).MatchesExistingSnapshot("InitialScreen");
    }

    [Test]
    public async Task ItWillDrawCoinsRandomly_AfterTimeHasPassed()
    {
        Fixture.GameRunner.StartGame();

        await Task.Delay(TimeSpan.FromSeconds(3));

        await Assert.That(Fixture.GameRunner).MatchesExistingSnapshot("CoinsAfterThreeSeconds");
        await Assert.That(Fixture.GameRunner.Game.Coins.Count).IsGreaterThan(0);
    }


}
