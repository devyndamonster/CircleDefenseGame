namespace CircleDefenseGame.Game
{
    /// <summary>
    /// Represents an object with a physical location in the world
    /// </summary>
    public interface IEntity
    {
        public int X { get; set; }

        public int Y { get; set; }
    }
}
