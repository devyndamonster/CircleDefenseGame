using System.Drawing;
using TUnit.Assertions.Core;

namespace CircleDefenseGame.Tests.TestUtilities.Assertions
{
    public class MatchesSnapshotAssertion : Assertion<GameRunner>
    {
        private readonly string _expectedSnapshot;

        public MatchesSnapshotAssertion(AssertionContext<GameRunner> context, string expectedSnapshot) : base(context)
        {
            _expectedSnapshot = expectedSnapshot;
        }

        protected override Task<AssertionResult> CheckAsync(EvaluationMetadata<GameRunner> metadata)
        {
            var game = metadata.Value;

            if (game == null)
            {
                return Task.FromResult(AssertionResult.Failed("GameRunner was null"));
            }

            Bitmap currentScreen = ScreenshotCapture.CaptureGameScreenshot(game.WindowHandle);

            if (ImageComparison.DoesImageMatchExistingSnapshot(currentScreen, _expectedSnapshot))
            {
                return Task.FromResult(AssertionResult.Passed);
            }

            return Task.FromResult(AssertionResult.Failed($"'screen does not match expected snapshot '{_expectedSnapshot}'"));
        }

        protected override string GetExpectation()
            => $"to match snapshot \"{_expectedSnapshot}\"";
    }
}
