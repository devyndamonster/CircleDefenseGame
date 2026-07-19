using CircleDefenseGame.Tests.TestUtilities.Assertions;
using TUnit.Assertions.Core;

namespace CircleDefenseGame.Tests.TestUtilities.Extensions
{
    public static class SnapshotAssertionExtensions
    {
        public static MatchesSnapshotAssertion MatchesExistingSnapshot(
            this IAssertionSource<GameRunner> source,
            string expectedSnapshot)
        {
            return new MatchesSnapshotAssertion(
                source.Context,
                expectedSnapshot);
        }
    }
}
