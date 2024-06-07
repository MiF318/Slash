using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Slash
{
    public abstract class Enemy : Entity
    {
        public Entity Target { get; private set; }

        public Enemy(Types type, Entity target, Point holeCords, Vector maxSpeed, Vector jump, int width, int height, int g) :
            base(type, new Point(holeCords.X, holeCords.Y), maxSpeed, jump, width, height, g)
        {
            Target = target;
        }

        public void Kill()
        {
            All.Remove(this);

            var hole = Levels.Level.Hole;

            if (Type == Types.Square)
            {
                hole.AllSquares.Remove((Square)this);
                hole.KilledSquare.Add((Square)this);
            }
            else if (Type == Types.Totem)
            {
                hole.AllTotems.Remove((Totem)this);
                hole.KilledTotems.Add((Totem)this);
            }
        }

        public void ChangeTarget(Entity target)
        {
            Target = target;
        }

        public void Tick (Game game, ref bool wasKill)
        {
            if (game.Slasher.Slashes == null)
                return;

            var topSlashSegment = game.Slasher.Slashes.Item1;
            var bottomSlashSegment = game.Slasher.Slashes.Item2;

            foreach (var hitboxSide in Hitbox.HitboxSides.Values)
            {
                if (Segment.CheckIntersectionSegments(topSlashSegment, hitboxSide) || Segment.CheckIntersectionSegments(bottomSlashSegment, hitboxSide))
                {
                    Kill();
                    wasKill = true;

                    game.Slasher.OneTimeActions.ResetSlash();

                    return;
                }
            }
        }
    }

    public class Square : Enemy
    {
        private static readonly Vector MaxSpeed = new Vector((int)(0.03 * Levels.Level.BlockSize), (int)(0.4 * Levels.Level.BlockSize));
        private static readonly Vector Jump = new Vector(0, -(int)(0.3 * Levels.Level.BlockSize));
        private static readonly int Width = 2 * Levels.Level.BlockSize;
        private static readonly int Height = 2 * Levels.Level.BlockSize;

        public Square(Entity target, Point spawnPoint) :
            base(Types.Square, target, new Point(spawnPoint.X, spawnPoint.Y), MaxSpeed, Jump, Width, Height, Physics.DefaultG)
        { }

        public Func<Point, Vector, Comparisons> GetComparator()
        {
            var level = Levels.Level;
            var blockSize = level.BlockSize;

            var comparator = new Func<Point, Vector, Comparisons>((point, vector) =>
            {
                Comparisons resultComparison = Comparisons.Less;

                Comparisons mapLimitsComparison = CheckMapLimits(point);
                if (mapLimitsComparison != Comparisons.Less)
                    return mapLimitsComparison;

                int deltaX = 0;
                int deltaY = 0;
                if (vector.X != 0)
                    deltaX = vector.X / Math.Abs(vector.X);
                if (vector.Y != 0)
                    deltaY = vector.Y / Math.Abs(vector.Y);

                Comparisons blockComparison = CheckBlocks(point, deltaX, deltaY);
                if (blockComparison == Comparisons.More)
                    return blockComparison;

                Comparisons entitiesComparison = CheckAllEnemies(point, deltaX, deltaY);

                if (blockComparison == Comparisons.Equal ||
                entitiesComparison == Comparisons.Equal)
                    resultComparison = Comparisons.Equal;

                else if (blockComparison == Comparisons.More ||
                entitiesComparison == Comparisons.More)
                    resultComparison = Comparisons.More;

                return resultComparison;
            });

            return comparator;
        }
        private Comparisons CheckMapLimits(Point point)
        {
            var level = Levels.Level;
            var blockSize = level.BlockSize;

            if (point.X == 0 || point.X == level.Width * blockSize - 1 ||
                point.Y == 0 || point.Y == level.Height * blockSize - 1)
                return Comparisons.Equal;

            else if (point.X < 0 || point.X > level.Width * blockSize - 1 ||
            point.Y < 0 || point.Y > level.Height * blockSize - 1)
                return Comparisons.More;

            return Comparisons.Less;
        }
        private Comparisons CheckBlocks(Point point, int deltaX, int deltaY)
        {
            var level = Levels.Level;
            var blockSize = level.BlockSize;

            if (!level.Map[point.X / blockSize, point.Y / blockSize].Solid
                && level.Map[(point.X + deltaX) / blockSize, (point.Y + deltaY) / blockSize].Solid)
                return Comparisons.Equal;
            else if (level.Map[point.X / blockSize, point.Y / blockSize].Solid)
                return Comparisons.More;

            return Comparisons.Less;
        }
        private Comparisons CheckAllEnemies(Point point, int deltaX, int deltaY)
        {
            foreach (var enemy in Levels.Level.Hole.AllSquares)
            {
                if (enemy.Type != Types.Square || enemy == this)
                    continue;

                if (!new Point(point.X, point.Y).CheckLocationPointInHitbox(enemy.Hitbox) &&
                    new Point(point.X + deltaX, point.Y + deltaY).CheckLocationPointInHitbox(enemy.Hitbox))
                    return Comparisons.Equal;

                else if (new Point(point.X + deltaX, point.Y + deltaY).CheckLocationPointInHitbox(enemy.Hitbox))
                    return Comparisons.More;
            }

            return Comparisons.Less;
        }

        public void Tick(Game game)
        {
            var wasKill = false;
            ((Enemy)(this)).Tick(game, ref wasKill);

            if (wasKill)
                return;

            foreach (var slasherHitboxSegment in game.Slasher.Hitbox.HitboxSides.Values)
            {
                foreach (var enemyHitboxSegment in Hitbox.HitboxSides.Values)
                {
                    if (Segment.CheckIntersectionSegments(slasherHitboxSegment, enemyHitboxSegment))
                    {
                        game.Status = Game.Statuses.Loose;
                        break;
                    }
                }
                if (game.Status == Game.Statuses.Loose)
                    break;
            }
        }

        public void Move()
        {
                var targetDistanceX = Target.Cords.X - Cords.X;

                if (Math.Abs(targetDistanceX) > Levels.Level.BlockSize / 10)
                {
                    Physics.Speed = new Vector(Physics.Speed.X + targetDistanceX / Math.Abs(targetDistanceX), Physics.Speed.Y);

                    if (OnGround)
                    {
                        List<Point> sidePoints = new List<Point>();
                        if (Physics.Speed.X > 0)
                            sidePoints = Hitbox.HitboxSides[Hitbox.Sides.Right].GetSegmentPoints(2);
                        else if (Physics.Speed.X < 0)
                            sidePoints = Hitbox.HitboxSides[Hitbox.Sides.Left].GetSegmentPoints(2);

                        var level = Levels.Level;
                        foreach (var sidePoint in sidePoints)
                        {
                            if (OnGround && sidePoint.X > 0 && sidePoint.X < level.Width * level.BlockSize - 1 &&
                                level.Map[(sidePoint.X + Physics.Speed.X) / level.BlockSize, sidePoint.Y / level.BlockSize].Solid)
                            {
                                Physics.Speed += Physics.Jump;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Physics.Speed.X /= 4;
                }

            Physics.Move(GetComparator());
        }
    }

    public class Totem : Enemy
    {
        private static readonly Vector MaxSpeed = new Vector(0, (int)(0.7 * Levels.Level.BlockSize));
        private static readonly Vector Jump = null;

        public int TicksToLoose {  get; private set; } 

        public Totem(Entity target, Point spawnPoint, int timeToLoose) : base(Types.Totem, target, spawnPoint, MaxSpeed, Jump, 2 * Levels.Level.BlockSize, 4 * Levels.Level.BlockSize, Physics.DefaultG)
        {
            TicksToLoose = timeToLoose;
        }

        public void Tick(Game game)
        {
            if (TicksToLoose == 0)
            {
                game.Status = Game.Statuses.Loose;
            }
            else
            {
                TicksToLoose--;
            }

            var wasKill = false;
            ((Enemy)(this)).Tick(game, ref wasKill);
        }

        public void DisplayProgressBar(Graphics graphics)
        {
            var totemProgressBarTexture = Textures.Other.TotemProgressBar;
            var totemProgressBarWidth = TicksToLoose * Width / Levels.Level.TotemTicksToLoose;

            graphics.DrawImage(totemProgressBarTexture, Cords.X, Cords.Y + 20, totemProgressBarWidth, 20);
        }
    }
}
