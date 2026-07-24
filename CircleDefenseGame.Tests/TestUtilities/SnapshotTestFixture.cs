namespace CircleDefenseGame.Tests.TestUtilities
{
    public class SnapshotTestFixture : IDisposable
    {
        public GameRunner GameRunner { get; private set; } = new GameRunner();

        public void Dispose()
        {
            GameRunner.Dispose();
        }
    }
}
