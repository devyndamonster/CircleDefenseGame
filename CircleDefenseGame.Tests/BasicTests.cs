using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace CircleDefenseGame.Tests;

public class BasicTests
{
    [Test]
    public async Task InitialGridScreenshot_MatchesOrCreatesBaseline()
    {
        string baselinePath = Path.Combine(GetProjectDirectory(), "Snapshots", "InitialGrid.png");
        string screenshotPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

        try
        {
            await CaptureScreenshot(screenshotPath);

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

    private static async Task CaptureScreenshot(string screenshotPath)
    {
        string gameExecutablePath = Path.Combine(AppContext.BaseDirectory, "CircleDefenseGame.exe");

        if (!File.Exists(gameExecutablePath))
        {
            throw new FileNotFoundException("The game executable was not copied to the test output directory.", gameExecutablePath);
        }

        var startInfo = new ProcessStartInfo(gameExecutablePath)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = AppContext.BaseDirectory,
        };
        startInfo.ArgumentList.Add("--screenshot");
        startInfo.ArgumentList.Add(screenshotPath);

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The game process could not be started.");
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        try
        {
            await process.WaitForExitAsync(cancellationTokenSource.Token);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"The game screenshot process exited with code {process.ExitCode}.");
        }

        if (!File.Exists(screenshotPath))
        {
            throw new FileNotFoundException("The game did not create its screenshot.", screenshotPath);
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
        string comparisonPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-visual-comparison.png");

        using var comparisonImage = new Bitmap(
            expectedImage.Width + actualImage.Width,
            Math.Max(expectedImage.Height, actualImage.Height) + HeaderHeight);
        using Graphics graphics = Graphics.FromImage(comparisonImage);

        graphics.Clear(Color.DimGray);
        graphics.DrawString("Expected", SystemFonts.DefaultFont, Brushes.White, 4, 8);
        graphics.DrawString("Actual", SystemFonts.DefaultFont, Brushes.White, expectedImage.Width + 4, 8);
        graphics.DrawImageUnscaled(expectedImage, 0, HeaderHeight);
        graphics.DrawImageUnscaled(actualImage, expectedImage.Width, HeaderHeight);
        comparisonImage.Save(comparisonPath, ImageFormat.Png);

        return comparisonPath;
    }

    private sealed record ImageDifference(string Description);
}
