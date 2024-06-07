using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading.Tasks.Sources;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;

namespace Slash
{
    public class Slasher : Entity
    {
        public Actions OneTimeActions { get; set; }

        public readonly int SlashRange; // Pixels
        
        public Tuple<Segment, Segment> Slashes { get; private set; }

        public Slasher(Point cords) : 
            base(Types.Slasher, cords, Physics.DefaultMaxSpeed, Physics.DefaultJump, 2 * Levels.Level.BlockSize, 2 * Levels.Level.BlockSize, Physics.DefaultG)
        {
            OneTimeActions = new Actions();
            SlashRange = Levels.Level.BlockSize * 7;
        }

        public void Tick(Game game)
        {
            Direction = OneTimeActions.Direction;

            var level = Levels.Level;
            foreach (Point botHitboxPoint in Hitbox.HitboxSides[Hitbox.Sides.Bottom].GetSegmentPoints(3))
            {
                if ((botHitboxPoint.Y + 1) / level.BlockSize > level.Height ||
                    level.Map[botHitboxPoint.X / level.BlockSize, (botHitboxPoint.Y + 1) / level.BlockSize].Dangerous)
                {
                    game.Status = Game.Statuses.Loose;
                    break;
                }
            }
        }

        public void Move()
        {
            Walk();

            Jump();

            Slash();
        }

        private void Walk()
        {
            var walkSpeed = new Vector(0, 0);

            if (OneTimeActions.Right && OneTimeActions.Left)
            {
                if (Direction == Directions.Right)
                    walkSpeed += WalkRight();
                else
                    walkSpeed += WalkLeft();
            }

            else
            {
                if (OneTimeActions.Right)
                {
                    walkSpeed += WalkRight();
                }

                if (OneTimeActions.Left)
                {
                    walkSpeed += WalkLeft();
                }

                if (!OneTimeActions.Right && !OneTimeActions.Left)
                {
                    walkSpeed = StayX();
                }
            }

            Physics.Speed += walkSpeed;
        }
        private Vector WalkRight()
        {
            return new Vector(1, 0);
        }
        private Vector WalkLeft()
        {
            return new Vector(-1, 0);
        }
        //REVORK
        private Vector StayX()
        {
            var speed = Physics.Speed;

            if (speed.X == 0)
                return new Vector(0, 0);

            else if (Math.Abs(speed.X) < 4)
            {
                return new Vector(-speed.X, 0);
            }

            return new Vector(-speed.X / 4, 0);
        }

        private void Jump()
        {
            if (OneTimeActions.Jump)
            {
                if (OnGround)
                {
                    Physics.Speed += Physics.Jump;
                }
            }
        }
        public void Slash()
        {
            Slashes = null;
            Segment topSlashSegment = null;
            Segment bottomSlashSegment = null;

            if (OneTimeActions.Slash == SlashOptions.Active)
            {
                int slash;

                if (Direction == Directions.Right)
                    slash = SlashRange;
                else
                    slash = - SlashRange;

                slash = FindValidLengthForSlash(Hitbox.GetAllHitboxPoints(5), slash);

                if (Direction == Directions.Right)
                {
                    var topPoint = Hitbox.HitboxSides[Hitbox.Sides.Right].End;
                    var botPoint = Hitbox.HitboxSides[Hitbox.Sides.Right].Start;
                    topSlashSegment = new Segment(topPoint, new Point(topPoint.X + slash, topPoint.Y));
                    bottomSlashSegment = new Segment(botPoint, new Point(botPoint.X + slash, botPoint.Y));
                }
                else
                {
                    var topPoint = Hitbox.HitboxSides[Hitbox.Sides.Left].End;
                    var botPoint = Hitbox.HitboxSides[Hitbox.Sides.Left].Start;
                    topSlashSegment = new Segment(topPoint, new Point(topPoint.X + slash, topPoint.Y));
                    bottomSlashSegment = new Segment(botPoint, new Point(botPoint.X + slash, botPoint.Y));
                }

                Cords.X += slash;
                if (Cords.X < 0)
                    Cords.X = 0;
                if (Cords.X > Levels.Level.Width * Levels.Level.BlockSize - 1)
                    Cords.X = Levels.Level.Width * Levels.Level.BlockSize - 1;

                OneTimeActions.UseSlash();

                Slashes = new Tuple<Segment, Segment>(topSlashSegment, bottomSlashSegment);

                Physics.Speed.Y = (int)(Physics.Jump.Y * 0.8);
                Physics.Speed.X = 0;

                return;
            }

            if (OnGround)
                OneTimeActions.ResetSlash();
        }
        private int FindValidLengthForSlash(List<Point> hitboxPoints, int slash)
        {
            var blockSize = Levels.Level.BlockSize;
            var map = Levels.Level.Map;

            for (int dx = slash; dx != 0; dx -= slash / Math.Abs(slash))
            {
                var valid = true;

                foreach (var hitboxPoint in hitboxPoints)
                {
                    
                    //Проверка на выход за пределы карты
                    if (hitboxPoint.X + dx < 0 || hitboxPoint.X + dx > Levels.Level.Width * blockSize - 1)
                    {
                        valid = false;
                        break;
                    }

                    //Проверка блока
                    else if (map[(hitboxPoint.X + dx) / blockSize, hitboxPoint.Y / blockSize].Solid)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    return dx;
            }

            return 0;
        }

        public void Damage(int damage)
        {
            // Damage
        }

        public void Hill(int hill)
        {
            // Hill
        }

        public static Slasher Spawn(Point spawnPoint)
        {
            return new Slasher(spawnPoint);
        }
    }

    public enum AllActions
    {
        Right,
        RightOff,

        Left,
        LeftOff,

        Jump,
        JumpOff,

        Slash
    }

    public class Actions
    {
        public bool Right {  get; private set; }
        public bool Left {  get; private set; }

        public Directions Direction { get; private set; }
        public bool Jump { get; private set; }
        public SlashOptions Slash { get; private set; }

        public Actions()
        {
            Right = false;
            Left = false;
            Direction = Directions.Right;
            Jump = false;
            Slash = SlashOptions.NotUsed;
        }

        public void ResetSlash()
        {
            Slash = SlashOptions.NotUsed;
        }
        public void UseSlash()
        {
            Slash = SlashOptions.Used;
        }

        public void ActionHandling(List<AllActions> actions)
        {
            foreach (var action in actions)
            {
                if (action == AllActions.Right)
                {
                    Right = true;
                    Direction = Directions.Right;
                }
                else if (action == AllActions.RightOff)
                    Right = false;

                else if (action == AllActions.Left)
                {
                    Left = true;
                    Direction = Directions.Left;
                }
                else if (action == AllActions.LeftOff)
                    Left = false;

                else if (action == AllActions.Slash && Slash == SlashOptions.NotUsed)
                    Slash = SlashOptions.Active;

                else if (action == AllActions.Jump)
                    Jump = true;
                else if (action == AllActions.JumpOff)
                    Jump = false;
            }
        }
    }

    public enum SlashOptions
    {
        Active,
        Used,
        NotUsed
    }

    public enum Directions
    {
        Right,
        Left
    }
}
