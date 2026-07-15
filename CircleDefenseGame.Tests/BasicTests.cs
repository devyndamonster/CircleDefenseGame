namespace CircleDefenseGame.Tests;

public class BasicTests
{
    [Test]
    public async Task True_IsTrue()
    {
        bool value = true;

        await Assert.That(value).IsTrue();
    }
}
