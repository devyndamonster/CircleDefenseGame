using CircleDefenseGame.Game.Behaviour;
using CircleDefenseGame.Game.Interaction;
using CircleDefenseGame.Game.Rendering;
using Raylib_cs;

namespace CircleDefenseGame.Game
{
    public class GameManager
    {
        public Dictionary<int, Dictionary<int, Tile>> Tiles { get; init; }

        public GameSettings GameSettings { get; init; }

        public Random Random { get; init; }

        public List<Coin> Coins { get; } = [];

        private List<IBehaviour> Behaviours { get; set; } = [];

        public GameManager(GameSettings settings)
        {
            GameSettings = settings;
            Random = new Random(settings.GameSeed);

            Behaviours = [
                new RandomlyAddCoins(),
                new InputManagement()
            ];

            Tiles = CreateRandomGrid();
            GenerateMiddleCircle();
        }

        public void Tick()
        {
            foreach (var behaviour in Behaviours)
            {
                behaviour.Tick(this);
            }
        }

        public IEnumerable<IRenderable> GetRenderables()
        {
            return Tiles.Values
                .SelectMany(row => row.Values)
                .OfType<IRenderable>()
                .Concat(Coins);
        }

        private Dictionary<int, Dictionary<int, Tile>> CreateRandomGrid()
        {
            var colors = new Dictionary<int, Dictionary<int, Tile>>();

            for (int row = 0; row < GameSettings.GridHeight; row++)
            {
                colors[row] = new Dictionary<int, Tile>();

                for (int col = 0; col < GameSettings.GridWidth; col++)
                {
                    colors[row][col] = new Tile
                    {
                        X = col * GameSettings.TileSize,
                        Y = row * GameSettings.TileSize,
                        Size = GameSettings.TileSize,
                        Color = new Color((byte)Random.Next(256), (byte)Random.Next(256), (byte)Random.Next(256), byte.MaxValue)
                    };
                }
            }

            return colors;
        }

        private void GenerateMiddleCircle()
        {
            for (int row = 0; row < Tiles.Count; row++)
            {
                for (int col = 0; col < Tiles[row].Count; col++)
                {
                    if (IsInsideCenterCircle(row, col))
                    {
                        Tiles[row][col].Color = Color.White;
                    }
                }
            }
        }

        private bool IsInsideCenterCircle(int row, int column)
        {
            double centerX = GameSettings.GridWidth / 2.0;
            double centerY = GameSettings.GridHeight / 2.0;
            double horizontalDistance = column + 0.5 - centerX;
            double verticalDistance = row + 0.5 - centerY;
            int radiusInSquares = 30;

            return horizontalDistance * horizontalDistance + verticalDistance * verticalDistance <= radiusInSquares * radiusInSquares;
        }
    }
}
