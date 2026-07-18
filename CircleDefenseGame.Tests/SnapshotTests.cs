using CircleDefenseGame.Tests.TestUtilities;
using System.Diagnostics;
using System.Drawing;

namespace CircleDefenseGame.Tests;

public class SnapshotTests
{
    [Test]
    public async Task ItWillRemoveCoin_WhenCoinIsClicked()
    {

    }

    [Test]
    public async Task ItWillDrawASmileyFace_WhenDrawnWithMouseClicks()
    {
        using GameRunner gameRunner = new();
        using Process game = gameRunner.StartGame();

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
            gameRunner.ClickGameWindow(game, point);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(100));

        using Bitmap currentScreen = ScreenshotCapture.CaptureGameScreenshot(game);

        await Assert.That(ImageComparison.DoesImageMatchExistingSnapshot(currentScreen, "SmileyFaceAfterClicks")).IsTrue();
    }

    [Test]
    public async Task ItWillDisplayInitialScreen()
    {
        using GameRunner gameRunner = new();
        using Process game = gameRunner.StartGame();

        Bitmap currentScreen = ScreenshotCapture.CaptureGameScreenshot(game);

        //If the baseline image doesn't exist, this will create it. If it does exist, it will compare the current screenshot to the baseline.
        await Assert.That(ImageComparison.DoesImageMatchExistingSnapshot(currentScreen, "InitialGrid")).IsTrue();
    }

    [Test]
    public async Task ItWillDrawCoinsRandomly_AfterTimeHasPassed()
    {
        using GameRunner gameRunner = new();
        using Process game = gameRunner.StartGame();

        await Task.Delay(TimeSpan.FromSeconds(3));

        Bitmap currentScreen = ScreenshotCapture.CaptureGameScreenshot(game);

        //If the baseline image doesn't exist, this will create it. If it does exist, it will compare the current screenshot to the baseline.
        await Assert.That(ImageComparison.DoesImageMatchExistingSnapshot(currentScreen, "CoinsAfterThreeSeconds")).IsTrue();
    }


}
