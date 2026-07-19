using CircleDefenseGame.Game.Behaviour;
using CircleDefenseGame.Game.Enemies;
using CircleDefenseGame.Game.Interaction;
using CircleDefenseGame.Game.Rendering;
using Raylib_cs;

namespace CircleDefenseGame.Game
{
    public class GameManager
    {
        private static readonly Color[] GrassColors =
        [
            new(54, 112, 47, 255),
            new(64, 124, 52, 255),
            new(74, 139, 57, 255),
            new(88, 151, 66, 255)
        ];

        private static readonly Color[] DirtColors =
        [
            new(119, 78, 42, 255),
            new(134, 90, 49, 255),
            new(151, 104, 58, 255),
            new(169, 120, 70, 255)
        ];

        public Dictionary<int, Dictionary<int, Tile>> Tiles { get; init; }

        public GameSettings GameSettings { get; init; }

        public Random Random { get; init; }

        public List<Coin> Coins { get; } = [];

        public List<Enemy> Enemies { get; } = [];

        private List<IBehaviour> Behaviours { get; set; } = [];

        public GameManager(GameSettings settings)
        {
            GameSettings = settings;
            Random = new Random(settings.GameSeed);

            Behaviours = [
                new RandomlyAddCoins(),
                new InputManagement(),
                new RandomlySpawnEnemies(),
                new UpdateEnemies(),
            ];

            Tiles = CreateTerrainGrid();
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
                .Concat(Coins)
                .Concat(Enemies);
        }

        private Dictionary<int, Dictionary<int, Tile>> CreateTerrainGrid()
        {
            var tiles = new Dictionary<int, Dictionary<int, Tile>>();

            for (int row = 0; row < GameSettings.GridHeight; row++)
            {
                tiles[row] = new Dictionary<int, Tile>();

                for (int col = 0; col < GameSettings.GridWidth; col++)
                {
                    tiles[row][col] = new Tile
                    {
                        X = col * GameSettings.TileSize,
                        Y = row * GameSettings.TileSize,
                        Size = GameSettings.TileSize,
                        Color = GetRandomGrassColor()
                    };
                }
            }

            return tiles;
        }

        private void GenerateMiddleCircle()
        {
            for (int row = 0; row < Tiles.Count; row++)
            {
                for (int col = 0; col < Tiles[row].Count; col++)
                {
                    if (IsInsideCenterCircle(row, col))
                    {
                        Tiles[row][col].Color = GetRandomColor(DirtColors, Random);
                    }
                }
            }
        }

        private Color GetRandomGrassColor()
        {
            int red = Random.Next(256);
            int green = Random.Next(256);
            int blue = Random.Next(256);

            return GrassColors[(red + green + blue) % GrassColors.Length];
        }

        private static Color GetRandomColor(Color[] colors, Random random) => colors[random.Next(colors.Length)];

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
