using System.Diagnostics;
using System.Drawing;

namespace CircleDefenseGame.Tests;

public class BasicTests
{
    [Test]
    public async Task InitialGridScreenshot_MatchesOrCreatesBaseline()
    {
        using GameRunner gameRunner = new();
        using Process game = gameRunner.StartGame();

        Bitmap currentScreen = ScreenshotCapture.CaptureGameScreenshot(game);

        //If the baseline image doesn't exist, this will create it. If it does exist, it will compare the current screenshot to the baseline.
        await Assert.That(ImageComparison.DoesImageMatchExistingSnapshot(currentScreen, "InitialGrid")).IsTrue();
    }

    [Test]
    public async Task RedTilesAfterFiveSecondsScreenshot_MatchesOrCreatesBaseline()
    {
        using GameRunner gameRunner = new();
        using Process game = gameRunner.StartGame();

        await Task.Delay(TimeSpan.FromSeconds(5));

        Bitmap currentScreen = ScreenshotCapture.CaptureGameScreenshot(game);

        //If the baseline image doesn't exist, this will create it. If it does exist, it will compare the current screenshot to the baseline.
        await Assert.That(ImageComparison.DoesImageMatchExistingSnapshot(currentScreen, "RedTilesAfterFiveSeconds")).IsTrue();
    }


}
