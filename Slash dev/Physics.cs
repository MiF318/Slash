using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace Slash
{
    public class Physics
    {
        public static readonly int DefaultG = 2;
        public static readonly Vector DefaultMaxSpeed = new Vector((int)(0.33 * Levels.Level.BlockSize), (int)(0.8 * Levels.Level.BlockSize));
        public static Vector DefaultJump = new Vector(0, -30);

        public readonly Entity Entity;
        public readonly int G;

        private readonly Vector? MaxSpeedValue;

        private Vector speed = new Vector(0,0);
        //Pixel / GameTick
        public Vector Speed {
            get
            {
                return speed;
            }
            set
            {
                if (value.X == 0)
                    speed.X = 0;
                else
                    speed.X = Math.Abs(speed.X) > MaxSpeedValue.X ? (value.X / Math.Abs(value.X) * MaxSpeedValue.X) : value.X;

                if (value.Y == 0)
                    speed.Y = 0;
                else
                    speed.Y = Math.Abs(speed.Y) > MaxSpeedValue.Y ? (value.Y / Math.Abs(value.Y) * MaxSpeedValue.Y) : value.Y;
            }
        }

        public Vector? Jump;

        public Physics(Entity entity, int g, Vector maxSpeedValue, Vector jump)
        {
            Entity = entity;
            G = g;

            if (maxSpeedValue != null)
            {
                if (maxSpeedValue.X < 0 || maxSpeedValue.Y < 0)
                    throw new Exception("Значение максимальной скорости не может быть отрицательное");
                else if (maxSpeedValue.X > Levels.Level.BlockSize || maxSpeedValue.Y > Levels.Level.BlockSize)
                    throw new Exception("Значение максимальной скорости не может быть больше размера одного блока");

                MaxSpeedValue = new Vector(maxSpeedValue.X, maxSpeedValue.Y);
            }

            Speed = new Vector(0, 0);
            Jump = jump;
        }

        public void Move()
        {
            var level = Levels.Level;
            var blockSize = level.BlockSize;

            var defaultComparator = new Func<Point, Vector, Comparisons>((point, vector) =>
            {
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

                return blockComparison;
            });

            Move(defaultComparator);
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

        //Revork
        public void Move(Func<Point, Vector, Comparisons> comparator)
        {
            var level = Levels.Level;
            var blockSize = level.BlockSize;

            var hitboxPoints = Entity.Hitbox.GetHitboxPointsBySpeed(Speed);

            foreach (var hitboxPoint in hitboxPoints)
            {
                var speedFromHitboxPoint = new Vector(hitboxPoint, Speed.X, Speed.Y);

                var destination = new Point(hitboxPoint.X + Speed.X, hitboxPoint.Y + Speed.Y);
                var destinationX = new Point(destination.X, hitboxPoint.Y);
                var destinationY = new Point(hitboxPoint.X, destination.Y);

                var speedFromHitboxPointX = new Vector(hitboxPoint, Speed.X, 0);
                var speedFromHitboxPointY = new Vector(hitboxPoint, 0, Speed.Y);

                var compX = comparator(destinationX, speedFromHitboxPointX);
                if (compX == Comparisons.More)
                    destinationX = Vector.BinarySearch(speedFromHitboxPointX, comparator);

                var compY = comparator(destinationY, speedFromHitboxPointY);
                if (compY == Comparisons.More)
                    destinationY = Vector.BinarySearch(speedFromHitboxPointY, comparator);

                speedFromHitboxPoint.X = destinationX.X - hitboxPoint.X;
                speedFromHitboxPoint.Y = destinationY.Y - hitboxPoint.Y;

                if (Math.Abs(Speed.X) > Math.Abs(speedFromHitboxPoint.X))
                {
                    Speed.X = speedFromHitboxPoint.X;
                }

                if (Math.Abs(Speed.Y) > Math.Abs(speedFromHitboxPoint.Y))
                {
                    Speed.Y = speedFromHitboxPoint.Y;
                }
            }

            Entity.Cords.X += Speed.X;
            Entity.Cords.Y += Speed.Y;
        }

        public void Fall()
        {
            if (!Entity.OnGround)
            {
                Speed.Y += G;
            }
        }
    }

    public interface IPhysics
    {
        public Physics Physics { get; }
        public void Fall();
        public void Move();
        public void GroundCheck();
    }

    public enum Comparisons
    {
        More,
        Less,
        Equal
    }
}
