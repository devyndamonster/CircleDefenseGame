using System.Diagnostics;
using System.Drawing;

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

            await Assert.That(actualImage.Width).IsEqualTo(expectedImage.Width);
            await Assert.That(actualImage.Height).IsEqualTo(expectedImage.Height);
            await Assert.That(FindFirstPixelDifference(expectedImage, actualImage)).IsNull();
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

    private static PixelDifference? FindFirstPixelDifference(
        Bitmap expectedImage,
        Bitmap actualImage)
    {
        for (int row = 0; row < expectedImage.Height; row++)
        {
            for (int column = 0; column < expectedImage.Width; column++)
            {
                Color expectedPixel = expectedImage.GetPixel(column, row);
                Color actualPixel = actualImage.GetPixel(column, row);

                if (expectedPixel != actualPixel)
                {
                    return new PixelDifference(column, row, expectedPixel, actualPixel);
                }
            }
        }

        return null;
    }

    private sealed record PixelDifference(int Column, int Row, Color Expected, Color Actual);
}
