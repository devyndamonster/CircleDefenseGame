using System.Drawing;

namespace CircleDefenseGame.Tests;

public class BasicTests
{
    private static readonly SemaphoreSlim VisualTestLock = new(1, 1);

    [Test]
    public async Task InitialGridScreenshot_MatchesOrCreatesBaseline()
    {
        await AssertScreenshotMatchesOrCreatesBaseline("InitialGrid");
    }

    [Test]
    public async Task RedTilesAfterFiveSecondsScreenshot_MatchesOrCreatesBaseline()
    {
        await AssertScreenshotMatchesOrCreatesBaseline(
            "RedTilesAfterFiveSeconds",
            screenshotDelaySeconds: 5);
    }

    private static async Task AssertScreenshotMatchesOrCreatesBaseline(
        string snapshotName,
        double? screenshotDelaySeconds = null)
    {
        await VisualTestLock.WaitAsync();
        try
        {
            string baselinePath = Path.Combine(GetProjectDirectory(), "Snapshots", $"{snapshotName}.png");
            string screenshotPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

            try
            {
                await ScreenshotCapture.CaptureGameScreenshotAsync(
                    screenshotPath,
                    screenshotDelaySeconds is null
                        ? null
                        : TimeSpan.FromSeconds(screenshotDelaySeconds.Value));

                if (!File.Exists(baselinePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
                    File.Move(screenshotPath, baselinePath);

                    await Assert.That(File.Exists(baselinePath)).IsTrue();
                    return;
                }

                using var expectedImage = new Bitmap(baselinePath);
                using var actualImage = new Bitmap(screenshotPath);

                ImageDifference? difference = ImageComparison.FindFirstDifference(expectedImage, actualImage);

                if (difference is not null)
                {
                    string comparisonPath = ImageComparison.CreateSideBySideComparison(expectedImage, actualImage);

                    TestContext.Current!.Output.AttachArtifact(new Artifact
                    {
                        File = new FileInfo(comparisonPath),
                        DisplayName = "Expected vs. actual screenshot",
                        Description = difference.Description,
                    });
                }

                await Assert.That(difference).IsNull();
            }
            finally
            {
                if (File.Exists(screenshotPath))
                {
                    File.Delete(screenshotPath);
                }
            }
        }
        finally
        {
            VisualTestLock.Release();
        }
    }

    private static string GetProjectDirectory()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CircleDefenseGame.Tests.csproj")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not find the test project directory.");
    }

}
