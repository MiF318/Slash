using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Slash
{
    public abstract class Portal
    {
        public Point Cords { get; set; } // Left-Top hitbox pixel
        public readonly int Width; // Pixels
        public readonly int Height; // Pixels

        public readonly Image Texture;

        public Hitbox Hitbox
        {
            get
            {
                return new Hitbox(Cords, Width, Height);
            }
        }

        public Portal(Point cords, int width, int height, Image texture)
        {
            Cords = cords;
            Width = width;
            Height = height;
            Texture = texture;
        }

        public enum Types
        {
            Hole,
            GodPortal
        }

        public void Display(Graphics graphics)
        {
            graphics.DrawImage(Texture, Cords.X, Cords.Y, Width, Height);
        }
    }

    public class GodPortal : Portal, IGameTick
    {
        public bool Active = false;

        public GodPortal(Point cords, int width, int heigh, Image texture) : base(cords, width, heigh, texture)
        {

        }

        public void Tick(Game game)
        {
            
        }
    }

    public class Hole : Portal, IGameTick
    {
        public List<Square> AllSquares { get; set; } = new List<Square>();
        public List<Square> KilledSquare { get; set; } = new List<Square>();
        public List<Totem> AllTotems { get; set; } = new List<Totem>();
        public List<Totem> KilledTotems { get; set; } = new List<Totem>();

        public int TickCount { get; private set; }
        public int TickCountForSpawn { get; private set; }
        public static readonly int MaxTickCountForSpawn = 500;

        public bool Active;

        public readonly int TotemTicksToLoose;

        public Hole(Point cords, int width, int height, Image texture, int tickCountForSpawn, bool spawnEnemies, int totemTicksToLoose) : base(cords, width, height, texture)
        {
            TickCountForSpawn = tickCountForSpawn;
            TotemTicksToLoose = totemTicksToLoose;
            Active = spawnEnemies;
        }

        public void Tick(Game game)
        {
            TickForSpawn(game, game.Slasher);

            if (game.Slasher.Slashes == null)
                return;

            var topSlashSegment = game.Slasher.Slashes.Item1;
            var bottomSlashSegment = game.Slasher.Slashes.Item2;

            foreach (var hitboxSide in Hitbox.HitboxSides.Values)
            {
                if (Segment.CheckIntersectionSegments(topSlashSegment, hitboxSide) || Segment.CheckIntersectionSegments(bottomSlashSegment, hitboxSide))
                {
                    game.Slasher.OneTimeActions.ResetSlash();
                }
            }

            if (AllSquares.Count > Levels.Level.MaxEnemySquareToLoose)
            {
                game.Status = Game.Statuses.Loose;
                return;
            }

            else if (KilledTotems.Count >= Levels.Level.CountTotemToWin)
            {
                game.Status = Game.Statuses.Win;
                Active = false;
                Levels.Level.GodPortal.Active = true;
                return;
            }
        }

        public void TickForSpawn(Game game, Entity targetForSpawnEnemies)
        {
            if (!Active)
                return;

            if (TickCount == TickCountForSpawn)
            {
                var square = new Square(targetForSpawnEnemies, Cords);
                AllSquares.Add(square);

                var rnd = new Random();
                TickCountForSpawn = rnd.Next(50, MaxTickCountForSpawn);

                SpawnAdditionalEnemies(targetForSpawnEnemies);

                TickCount = 0;
            }

            TickCount++;

            if (KilledTotems.Count >= 5)
            {
                Active = false;
                Levels.Level.GodPortal.Active = true;
            }
        }
        private void SpawnAdditionalEnemies(Entity targetForSpawnEnemies)
        {
            var spawnAdditionalEnemies = 0.5;
            var totemChance = 0.5;

            var rnd1 = new Random();
            if (rnd1.NextDouble() < spawnAdditionalEnemies)
            {
                var totemCount = 0;
                for (int i = 0; i < rnd1.Next(1, 4); i++)
                {
                    var rnd2 = new Random();
                    var newCords = new Point(rnd2.Next(2 * Levels.Level.BlockSize, Levels.Level.Width * Levels.Level.BlockSize - 4 * Levels.Level.BlockSize), 0);
                    if (rnd2.NextDouble() < totemChance)
                    {
                        var square = new Square(targetForSpawnEnemies, newCords);
                        AllSquares.Add(square);
                    }
                    else if (totemCount == 0)
                    {
                        var totem = new Totem(targetForSpawnEnemies, newCords, TotemTicksToLoose);
                        AllTotems.Add(totem);
                        totemCount++;
                    }
                }
            }
        }
    }
}
