using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Slash
{
    public static class Levels
    {
        public static Level[] AllLevels { get; private set; }

        private static int LevelNumber = 0;

        public static Level Level { get; set; }

        public static void Generate(Level[] levels)
        {
            AllLevels = levels;

            Level = AllLevels[LevelNumber];
        }

        public static Level Next()
        {
            if (LevelNumber == AllLevels.Length - 1)
                LevelNumber = 0;
            else
                LevelNumber++;

            Level = AllLevels[LevelNumber];

            return Level;
        }
        public static Level Previous()
        {
            if (LevelNumber == 0)
                LevelNumber = AllLevels.Length - 1;
            else
                LevelNumber--;

            Level = AllLevels[LevelNumber];
            return Level;
        }

        public static Level Restart()
        {
            Level.Restart();
            return Level;
        }
    }

    public class Level : IGameTick
    {
        public Block[,] Map { get; private set; }
        private string TextMap;
        public Image BG { get; private set; }
        public int Width { get; private set; } // Blocks
        public int Height { get; private set; } // Blocks
        public int BlockSize { get; private set; } // Pixel
        public Point SpawnPoint { get; private set; }
        public Hole Hole{ get; private set; }
        public GodPortal GodPortal { get; private set; }

        public readonly int MaxEnemySquareToLoose;
        public readonly int TotemTicksToLoose;
        public readonly int CountTotemToWin;

        public readonly bool SpawnEnemies;

        public Level(Image BG, int width, int height, int blockSize, string map, int maxEnemySquareToLoose, int totemTicksToLoose, int countTotemToWin, bool spawn)
        {
            TextMap = map;
            this.BG = BG;
            Width = width;
            Height = height;
            BlockSize = blockSize;
            MaxEnemySquareToLoose = maxEnemySquareToLoose;
            TotemTicksToLoose = totemTicksToLoose;
            CountTotemToWin = countTotemToWin;
            SpawnEnemies = spawn;

            Map = CreateMap(TextMap);
        }

        public void Tick(Game game)
        {
            Hole.Tick(game);
            GodPortal.Tick(game);
        }
        public void Display(Graphics graphics)
        {
            graphics.DrawImage(BG, 0, 0, Width * BlockSize, Height * BlockSize);

            foreach (var block in Map)
            {
                if (block.Type.MainType != AllBlockTypes.Air)
                {
                    var texture = block.Texture;
                    var x = block.Cords.X;
                    var y = block.Cords.Y;
                    var size = Levels.Level.BlockSize;

                    graphics.DrawImage(texture, x * size, y * size, size, size);
                }
            }

            graphics.DrawImage(Hole.Texture, Hole.Cords.X, Hole.Cords.Y, Hole.Width, Hole.Height);
            graphics.DrawImage(GodPortal.Texture, GodPortal.Cords.X, GodPortal.Cords.Y, GodPortal.Width, GodPortal.Height);
        }

        public void Restart()
        {
            Map = CreateMap(TextMap);
        }

        private Block[,] CreateMap(string textMap)
        {
            var allPlatforms = new List<Block>();

            int x = 0;
            int y = 0;
            Map = new Block[Width, Height];

            foreach (char textBlock in textMap)
            {
                if (x == Width)
                {
                    x = 0;
                    y += 1;
                }

                Map[x, y] = DetectBlock(textBlock, new Point(x, y), Map);

                if (Map[x, y].Role == BlockRoles.Platform)
                    allPlatforms.Add(Map[x, y]);

                x++;
            }

            SpawnPoint = new Point(GodPortal.Cords.X + GodPortal.Width / 2, GodPortal.Cords.Y);

            foreach (var block in allPlatforms)
            {
                Map[block.Cords.X, block.Cords.Y] = Blocks.UpdatePlatform(Map, block.Cords, Blocks.TwigTypesTree);
            }

            return Map;
        }

        private Block DetectBlock(char textBlock, Point point, Block[,] map)
        {
            if (textBlock == 'a')
                return Blocks.CreateBlock(point, AllBlockTypes.Air, map);

            else if (textBlock == 'g')
                return Blocks.CreateBlock(point, AllBlockTypes.Grass, map);

            else if (textBlock == 'd')
                return Blocks.CreateBlock(point, AllBlockTypes.Dirt, map);

            else if (textBlock == 't')
                return Blocks.CreateBlock(point, AllBlockTypes.Twig, map);

            else if (textBlock == 'b')
                return Blocks.CreateBlock(point, AllBlockTypes.BlackStone, map);

            else if (textBlock == 'l')
                return Blocks.CreateBlock(point, AllBlockTypes.Lava, map);

            else if (textBlock == 'h')
            {
                var width = 3;
                var height = 5;
                Hole = new Hole(
                    new Point((point.X - (width - 1) / 2) * BlockSize, (point.Y - (height - 1)) * BlockSize),
                    width * BlockSize, height * BlockSize,
                    Textures.Portals.GetTexture(Portal.Types.Hole),
                    Hole.MaxTickCountForSpawn, SpawnEnemies, TotemTicksToLoose);
                return Blocks.CreateBlock(point, AllBlockTypes.Air, map);
            }

            else if (textBlock == 'p')
            {
                var width = 4;
                var height = 4;
                GodPortal = new GodPortal(new Point((point.X - (width - 1) / 2) * BlockSize, (point.Y - (height - 1)) * BlockSize), width * BlockSize, height * BlockSize, Textures.Portals.GetTexture(Portal.Types.GodPortal));
                return Blocks.CreateBlock(point, AllBlockTypes.Air, map);
            }

            return Blocks.CreateBlock(point, AllBlockTypes.Error, map);
        }
    }
}
