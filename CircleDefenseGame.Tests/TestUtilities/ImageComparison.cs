using System.Drawing;
using System.Drawing.Imaging;

namespace CircleDefenseGame.Tests.TestUtilities;

internal static class ImageComparison
{
    public static bool DoesImageMatchExistingSnapshot(Bitmap currentScreen, string snapshotName)
    {
        string baselinePath = Path.Combine(GetProjectDirectory(), "Snapshots", $"{snapshotName}.png");

        if (!File.Exists(baselinePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            currentScreen.Save(baselinePath, ImageFormat.Png);
            return true;
        }

        using var expectedImage = new Bitmap(baselinePath);
        ImageDifference? difference = FindFirstDifference(expectedImage, currentScreen);

        if (difference is null)
        {
            return true;
        }

        string comparisonPath = CreateSideBySideComparison(expectedImage, currentScreen);
        TestContext.Current!.Output.AttachArtifact(new Artifact
        {
            File = new FileInfo(comparisonPath),
            DisplayName = "Expected vs. actual screenshot",
            Description = difference.Description,
        });

        return false;
    }

    public static ImageDifference? FindFirstDifference(Bitmap expectedImage, Bitmap actualImage)
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

    public static string CreateSideBySideComparison(Bitmap expectedImage, Bitmap actualImage)
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
                bool pixelsDiffer = column >= expectedImage.Width
                    || row >= expectedImage.Height
                    || column >= actualImage.Width
                    || row >= actualImage.Height
                    || expectedImage.GetPixel(column, row) != actualImage.GetPixel(column, row);

                if (pixelsDiffer)
                {
                    graphics.FillRectangle(Brushes.Red, xOffset + column, yOffset + row, 1, 1);
                }
            }
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

internal sealed record ImageDifference(string Description);
