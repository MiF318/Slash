using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slash
{
    public abstract class Entity : IPhysics, IGameTick
    {
        public static List<Entity> All { get; private set; } = new List<Entity>();

        public static void TickForAll(Game game)
        {
            Entity[] oldAllEntities = new Entity[All.Count];

            All.CopyTo(oldAllEntities);
            foreach (Entity entity in oldAllEntities)
            {
                entity.Tick(game);
            }
        }

        public static void DisplayAll(Graphics graphics)
        {
            foreach(Entity entity in All)
            {
                entity.Display(graphics);
            }
        }

        public readonly Types Type;

        public Point Cords { get; set; } // Left-Top hitbox pixel
        public readonly int Width; // Pixels
        public readonly int Height; // Pixels

        public Hitbox Hitbox
        {
            get
            {
                if (Type == Types.Slasher)
                {
                    return new Hitbox(Cords, Width, Height, 15);
                }
                return new Hitbox(Cords, Width, Height);
            }
        }

        public Directions Direction { get; set; }

        public bool OnGround { get; private set; } = false;
        public void GroundCheck()
        {
            var level = Levels.Level;

            foreach (var botHitboxPoint in Hitbox.HitboxSides[Hitbox.Sides.Bottom].GetSegmentPoints(2))
            {
                if ((botHitboxPoint.Y + 1) / level.BlockSize > level.Height ||
                    level.Map[botHitboxPoint.X / level.BlockSize, (botHitboxPoint.Y + 1) / level.BlockSize].Solid)
                {
                    OnGround = true;
                    Physics.Speed.Y = 0;
                    return;
                }
            }

            OnGround = false;
        }

        public Physics Physics { get; private set; }

        private int SpriteNumber = 0;
        public Image Texture
        {
            get
            {
                if (Type == Types.Slasher)
                {
                    if (Direction == Directions.Right)
                    {
                        SpriteNumber = 1;
                    }
                    else if (Direction == Directions.Left)
                    {
                        SpriteNumber = 0;
                    }

                    return Textures.Entities.GetTexture(Type, SpriteNumber);
                }

                return Textures.Entities.GetTexture(Type);
            }
        }

        public Entity(Types type, Point cords, Vector maxSpeed, Vector jump, int width, int height, int g)
        {
            Type = type;
            Cords = new Point(cords.X, cords.Y);
            Width = width;
            Height = height;
            Physics = new Physics(this, g, maxSpeed, jump);

            All.Add(this);
        }

        public void Tick(Game game)
        {
            GroundCheck();

            if (Type == Types.Slasher)
            {
                ((Slasher)this).Tick(game);
            }
            else if (Type == Types.Square)
            {
                ((Square)this).Tick(game);
            }
            else if (Type == Types.Totem)
            {
                ((Totem)this).Tick(game);
            }

            Move();
        }

        public void Move()
        {
            Fall();

            if (Type == Types.Slasher)
            {
                ((Slasher)this).Move();
            }

            else if (Type == Types.Square)
            {
                ((Square)this).Move();
                Physics.Move(((Square)this).GetComparator());
                return;
            }

            Physics.Move();
        }
        public void Fall()
        {
            Physics.Fall();
        }

        public void Display(Graphics graphics)
        {
            graphics.DrawImage(Texture, Cords.X, Cords.Y, Width, Height);

            if (Type == Types.Totem)
            {
                ((Totem)this).DisplayProgressBar(graphics);
            }
        }

        public enum Types
        {
            Slasher,
            Square,
            Sphere,
            Totem
        }
    }

    public class Hitbox
    {
        public enum Sides
        {
            Top,
            Right,
            Bottom,
            Left
        }

        public Dictionary<Sides, Segment> HitboxSides;

        public Hitbox(Point startCords, int width, int height, int lateralDisplacement)
        {
            HitboxSides = GetSides(startCords, width, height, lateralDisplacement);
        }
        public Hitbox(Point startCords, int width, int height)
        {
            HitboxSides = GetSides(startCords, width, height, 0);
        }
        private Dictionary<Sides, Segment> GetSides(Point startCords, int width, int height, int lateralDisplacement)
        {
            return new Dictionary<Sides, Segment>()
            {
                {
                    Sides.Top, new Segment(
                        new Point(startCords.X + lateralDisplacement, startCords.Y),
                        new Point(startCords.X + width - lateralDisplacement, startCords.Y))
                },
                {
                    Sides.Right, new Segment(
                        new Point(startCords.X + width - lateralDisplacement, startCords.Y),
                        new Point(startCords.X + width - lateralDisplacement, startCords.Y + height))
                },
                {
                    Sides.Bottom, new Segment(
                        new Point(startCords.X + width - lateralDisplacement, startCords.Y + height),
                        new Point(startCords.X + lateralDisplacement, startCords.Y + height))
                },
                {
                    Sides.Left, new Segment(
                        new Point(startCords.X + lateralDisplacement, startCords.Y + height),
                        new Point(startCords.X + lateralDisplacement, startCords.Y))
                }
            };
        }

        public List<Point> GetAllHitboxPoints(int pointsPerBlock)
        {
            var hitboxPoints = new List<Point>();

            foreach (var side in HitboxSides.Values)
            {
                hitboxPoints.AddRange(side.GetSegmentPoints(pointsPerBlock));
            }

            return hitboxPoints;
        }
        public List<Point> GetHitboxPointsBySpeed(int pointsPerBlock, Vector speed)
        {
            var hitboxPoints = new List<Point>();

            if (speed.X > 0)
                hitboxPoints.AddRange(HitboxSides[Sides.Right].GetSegmentPoints(pointsPerBlock));
            if (speed.X < 0)
                hitboxPoints.AddRange(HitboxSides[Sides.Left].GetSegmentPoints(pointsPerBlock));

            if (speed.Y < 0)
                hitboxPoints.AddRange(HitboxSides[Sides.Top].GetSegmentPoints(pointsPerBlock));
            if (speed.Y > 0)
                hitboxPoints.AddRange(HitboxSides[Sides.Bottom].GetSegmentPoints(pointsPerBlock));

            return hitboxPoints;
        }
        public List<Point> GetHitboxPointsBySpeed(Vector speed)
        {
            return GetHitboxPointsBySpeed(5, speed);
        }
    }
}
