using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

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
                await CaptureScreenshot(screenshotPath, screenshotDelaySeconds);

                if (!File.Exists(baselinePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
                    File.Move(screenshotPath, baselinePath);

                    await Assert.That(File.Exists(baselinePath)).IsTrue();
                    return;
                }

                using var expectedImage = new Bitmap(baselinePath);
                using var actualImage = new Bitmap(screenshotPath);

                ImageDifference? difference = FindFirstImageDifference(expectedImage, actualImage);

                if (difference is not null)
                {
                    string comparisonPath = CreateSideBySideComparison(expectedImage, actualImage);

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

    private static async Task CaptureScreenshot(string screenshotPath, double? screenshotDelaySeconds)
    {
        string gameExecutablePath = Path.Combine(AppContext.BaseDirectory, "CircleDefenseGame.exe");

        if (!File.Exists(gameExecutablePath))
        {
            throw new FileNotFoundException("The game executable was not copied to the test output directory.", gameExecutablePath);
        }

        var startInfo = new ProcessStartInfo(gameExecutablePath)
        {
            UseShellExecute = false,
            WorkingDirectory = AppContext.BaseDirectory,
        };

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The game process could not be started.");
        using var cancellationTokenSource = new CancellationTokenSource(
            TimeSpan.FromSeconds((screenshotDelaySeconds ?? 0) + 10));

        try
        {
            IntPtr windowHandle = await WaitForWindowHandle(process, cancellationTokenSource.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationTokenSource.Token);

            if (screenshotDelaySeconds is not null)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(screenshotDelaySeconds.Value),
                    cancellationTokenSource.Token);
            }

            await WindowsGraphicsCapture.SaveWindowScreenshotAsync(
                windowHandle,
                screenshotPath,
                cancellationTokenSource.Token);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }

        if (!File.Exists(screenshotPath))
        {
            throw new FileNotFoundException("The window capture did not create its screenshot.", screenshotPath);
        }
    }

    private static async Task<IntPtr> WaitForWindowHandle(Process process, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            process.Refresh();

            if (process.MainWindowHandle != IntPtr.Zero)
            {
                return process.MainWindowHandle;
            }

            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    $"The game exited with code {process.ExitCode} before creating its window.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
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

    private static ImageDifference? FindFirstImageDifference(
        Bitmap expectedImage,
        Bitmap actualImage)
    {
        if (expectedImage.Width != actualImage.Width || expectedImage.Height != actualImage.Height)
        {
            return new ImageDifference(
                $"Expected {expectedImage.Width}x{expectedImage.Height}, "
                + $"but found {actualImage.Width}x{actualImage.Height}.");
        }

        for (int row = 0; row < expectedImage.Height; row++)
        {
            for (int column = 0; column < expectedImage.Width; column++)
            {
                Color expectedPixel = expectedImage.GetPixel(column, row);
                Color actualPixel = actualImage.GetPixel(column, row);

                if (expectedPixel != actualPixel)
                {
                    return new ImageDifference(
                        $"Pixel ({column}, {row}) expected {expectedPixel}, but found {actualPixel}.");
                }
            }
        }

        return null;
    }

    private static string CreateSideBySideComparison(Bitmap expectedImage, Bitmap actualImage)
    {
        const int HeaderHeight = 32;
        int differenceWidth = Math.Max(expectedImage.Width, actualImage.Width);
        int differenceHeight = Math.Max(expectedImage.Height, actualImage.Height);
        string comparisonPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-visual-comparison.png");

        using var comparisonImage = new Bitmap(
            expectedImage.Width + differenceWidth + actualImage.Width,
            differenceHeight + HeaderHeight);
        using Graphics graphics = Graphics.FromImage(comparisonImage);

        graphics.Clear(Color.DimGray);
        graphics.DrawString("Expected", SystemFonts.DefaultFont, Brushes.White, 4, 8);
        graphics.DrawString("Differences", SystemFonts.DefaultFont, Brushes.White, expectedImage.Width + 4, 8);
        graphics.DrawString(
            "Actual",
            SystemFonts.DefaultFont,
            Brushes.White,
            expectedImage.Width + differenceWidth + 4,
            8);
        graphics.DrawImageUnscaled(expectedImage, 0, HeaderHeight);
        graphics.FillRectangle(Brushes.White, expectedImage.Width, HeaderHeight, differenceWidth, differenceHeight);
        DrawDifferenceMask(graphics, expectedImage, actualImage, expectedImage.Width, HeaderHeight);
        graphics.DrawImageUnscaled(actualImage, expectedImage.Width + differenceWidth, HeaderHeight);
        comparisonImage.Save(comparisonPath, ImageFormat.Png);

        return comparisonPath;
    }

    private static void DrawDifferenceMask(
        Graphics graphics,
        Bitmap expectedImage,
        Bitmap actualImage,
        int xOffset,
        int yOffset)
    {
        int width = Math.Max(expectedImage.Width, actualImage.Width);
        int height = Math.Max(expectedImage.Height, actualImage.Height);

        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                bool isOutsideExpectedImage = column >= expectedImage.Width || row >= expectedImage.Height;
                bool isOutsideActualImage = column >= actualImage.Width || row >= actualImage.Height;
                bool pixelsDiffer = isOutsideExpectedImage
                    || isOutsideActualImage
                    || expectedImage.GetPixel(column, row) != actualImage.GetPixel(column, row);

                if (pixelsDiffer)
                {
                    graphics.FillRectangle(Brushes.Red, xOffset + column, yOffset + row, 1, 1);
                }
            }
        }
    }

    private sealed record ImageDifference(string Description);
}
