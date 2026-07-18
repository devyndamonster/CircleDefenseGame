namespace CircleDefenseGame.Game
{
    public class GameSettings
    {
        public required int GameSeed { get; init; }

        public required int GridHeight { get; init; }

        public required int GridWidth { get; init; }

        public required int TileSize { get; init; }
    }
}
