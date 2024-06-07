using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slash
{
    public class Vector
    {
        public int X {  get; set; }
        public int Y { get; set; }

        public Point Start { get; set; }

        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y);
            }
        }

        public Vector(int x, int y)
        {
            X = x;
            Y = y;
            Start = new Point(0, 0);
        }

        public Vector(Point start, int x, int y)
        {
            Start = start;
            X = x;
            Y = y;
        }

        public static Point BinarySearch(Vector vector, Func<Point, Vector, Comparisons> comparator)
        {
            if (vector.Length == 0)
                return vector.Start;

            var newVector = new Vector(vector.Start, vector.X, vector.Y);
            var halfVector = newVector / 2;
            var currentPoint = new Point(halfVector.X, halfVector.Y) + vector.Start;

            var comparison = comparator(currentPoint, vector);

            if (vector.Length == 1)
            {
                if (comparison == Comparisons.Equal)
                    return currentPoint;
                else if (comparison == Comparisons.Less)
                    return new Point(currentPoint.X + vector.X, currentPoint.Y + vector.Y);
                else
                    return currentPoint;
            }

            if (comparison == Comparisons.Equal)
                return currentPoint;

            else if (comparison == Comparisons.Less)
                halfVector.Start = currentPoint;

            return BinarySearch(halfVector, comparator);
        }

        public static Vector operator + (Vector vector1, Vector vector2)
        {
            return new Vector(vector1.X + vector2.X, vector1.Y + vector2.Y);
        }

        public static Vector operator * (Vector vector, int value)
        {
            vector.X *= value;
            vector.Y *= value;
            return vector;
        }
        public static Vector operator / (Vector vector, int value)
        {
            vector.X /= value;
            vector.Y /= value;
            return vector;
        }

        public static bool operator > (Vector vector1, Vector vector2)
        {
            
            //Коллинеарность
            if (new Fraction(vector1.X, vector2.X) != new Fraction(vector1.Y, vector2.Y))
                return false;
            

            if (vector1.Length > vector2.Length)
                return true;

            else 
                return false;
        }

        public static bool operator < (Vector vector1, Vector vector2)
        {
            //Коллинеарность
            if (false && new Fraction(vector1.X, vector2.X) != new Fraction(vector1.Y, vector2.Y))
                return false;

            else if (vector1.X < vector2.X && vector1.Y < vector2.Y)
                return true;

            else
                return false;
        }
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator + (Point point1, Point point2)
        {
            return new Point(point1.X + point2.X, point1.Y + point2.Y);
        }
        public static Point operator * (Point point, int value)
        {
            point.X *= value;
            point.Y *= value;
            return point;
        }

        public Point Copy()
        {
            return new Point(X, Y); 
        }

        public bool CheckLocationPointInHitbox(Hitbox hitbox)
        {
            return hitbox.HitboxSides[Hitbox.Sides.Top].Start.X < X && hitbox.HitboxSides[Hitbox.Sides.Top].Start.Y < Y &&
                hitbox.HitboxSides[Hitbox.Sides.Bottom].End.X > X && hitbox.HitboxSides[Hitbox.Sides.Bottom].End.Y > Y;
        }
    }

    public class Fraction
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; }

        public Fraction(int numerator, int denominator)
        {
            Numerator = numerator;

            if (denominator == 0)
                Denominator = 0;
            else
                Denominator = denominator;
        }

        public static Fraction operator * (Fraction fraction, int number)
        {
            var newFraction = new Fraction(fraction.Numerator * number, fraction.Denominator);
            newFraction.Numerator *= number;
            newFraction.FractionCut();
            return newFraction;
        }

        public static Fraction operator / (Fraction fraction, int number)
        {
            fraction.Denominator *= number;
            fraction.FractionCut();
            return fraction;
        }

        public static bool operator == (Fraction fraction1, Fraction fraction2)
        {
            var newFraction1 = new Fraction(fraction1.Numerator, fraction1.Denominator);
            var newFraction2 = new Fraction(fraction2.Numerator, fraction2.Denominator);

            newFraction1.FractionCut();
            newFraction2.FractionCut();

            return (newFraction1.Numerator == newFraction2.Numerator) && (newFraction1.Denominator == newFraction2.Denominator);
        }
        public static bool operator != (Fraction fraction1, Fraction fraction2)
        {
            var newFraction1 = new Fraction(fraction1.Numerator, fraction1.Denominator);
            var newFraction2 = new Fraction(fraction2.Numerator, fraction2.Denominator);

            newFraction1.FractionCut();
            newFraction2.FractionCut();

            return (newFraction1.Numerator != newFraction2.Numerator) || (newFraction1.Denominator != newFraction2.Denominator);
        }

        public void FractionCut()
        {
            for (int nod = Math.Min(Numerator, Denominator); nod > 0; nod--)
            {
                if (Numerator % nod == 0 && Denominator % nod == 0)
                {
                    Numerator /= nod;
                    Denominator /= nod;

                    break;
                }
            }
        }
    }

    public class Segment
    {
        public Point Start;
        public Point End;

        public bool Horizontal = false;

        public Segment(Point start, Point end)
        {
            if (start.X != end.X && start.Y != end.Y)
                throw new Exception("Отрезки могут быть только вертикальными и горизонтальными");

            if (start.Y == end.Y)
                Horizontal = true;
            if (start.X < end.X || start.Y < end.Y)
            {
                Start = start;
                End = end;
            }
            else
            {
                Start = end;
                End = start;
            }
        }

        public static bool CheckIntersectionSegments(Segment segment1, Segment segment2)
        {
            if (segment1.Horizontal &&  segment2.Horizontal || !segment1.Horizontal && !segment2.Horizontal)
            {
                return false;
            }

            if (segment1.Horizontal)
            {
                if (segment1.Start.X <= segment2.Start.X && segment1.End.X >= segment2.Start.X &&
                    segment1.Start.Y >= segment2.Start.Y && segment1.Start.Y <= segment2.End.Y)
                    return true;
            }
            else
            {
                if (segment1.Start.X >= segment2.Start.X && segment1.Start.X <= segment2.End.X &&
                    segment1.Start.Y <= segment2.Start.Y && segment1.End.Y >= segment2.Start.Y)
                    return true;
            }

            return false;
        }

        public List<Point> GetSegmentPoints (int pointsPerBlock)
        {
            if (pointsPerBlock == 1)
                return new List<Point>() { Start.Copy() };
            else if (pointsPerBlock < 1)
                throw new Exception("Отрицательное количество точек на блок");

            var segmentPoints = new List<Point>();

            int numberSegmentStart;
            int numberSegmentEnd;

            if (Horizontal)
            {
                numberSegmentStart = Start.X;
                numberSegmentEnd = End.X;
            }
            else
            {
                numberSegmentStart = Start.Y;
                numberSegmentEnd = End.Y;
            }

            for (var i = numberSegmentStart; i <= numberSegmentEnd;
                i += (Levels.Level.BlockSize) / (pointsPerBlock - 1))
            {
                if (Horizontal)
                    segmentPoints.Add(new Point(i, Start.Y));
                else
                    segmentPoints.Add(new Point(Start.X, i));
            }

            return segmentPoints;
        }
    }
}
