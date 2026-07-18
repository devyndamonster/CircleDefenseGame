using System.Diagnostics;
using System.Drawing;

namespace CircleDefenseGame.Tests;

public class BasicTests
{
    [Test]
    public async Task ItWillDrawASmileyFace_WhenDrawnWithMouseClicks()
    {
        using GameRunner gameRunner = new();
        using Process game = gameRunner.StartGame();

        Point[] smileyFace =
        [
            new(435, 435), new(565, 435),
            new(465, 555), new(535, 555),
            new(475, 575), new(525, 575),
            new(485, 585), new(505, 585),
            new(495, 595)
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
    public async Task ItWillDrawRedSquaresRandomly_AfterTimeHasPassed()
    {
        using GameRunner gameRunner = new();
        using Process game = gameRunner.StartGame();

        await Task.Delay(TimeSpan.FromSeconds(5));

        Bitmap currentScreen = ScreenshotCapture.CaptureGameScreenshot(game);

        //If the baseline image doesn't exist, this will create it. If it does exist, it will compare the current screenshot to the baseline.
        await Assert.That(ImageComparison.DoesImageMatchExistingSnapshot(currentScreen, "RedTilesAfterFiveSeconds")).IsTrue();
    }


}
