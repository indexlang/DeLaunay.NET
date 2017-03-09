using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DeLaunay.NET.Shapes;

namespace DeLaunay.NET
{
    public class Point : IComparable
    {
        public readonly float X;
        public readonly float Y;
        private const double ComparisonTolerance = 0.0001;

        /// <summary>
        /// Intance Constructor
        /// </summary>
        /// <param name="x">Point X Value</param>
        /// <param name="y">Point Y Value</param>
        public Point(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var point = obj as Point;
            return !ReferenceEquals(point, null) && this.Equals(point);
        }

        /// <summary>
        /// Check equality between this instance and another <see cref="Point"/>
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Equality Value</returns>
        public bool Equals(Point point)
        {
            if (ReferenceEquals(this, point))
                return true;
            if (ReferenceEquals(point, null))
                return false;
            return Math.Abs(this.X - point.X) < ComparisonTolerance &&
                   Math.Abs(this.Y - point.Y) < ComparisonTolerance;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.X.GetHashCode() * 397) ^ this.Y.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.X}, {this.Y}";
        }

        /// <inheritdoc />
        public static bool operator ==(Point point1, Point point2)
        {
            if (ReferenceEquals(point1, null) ^ ReferenceEquals(point2, null))
                return false;
            return ReferenceEquals(point1, point2) || point1.Equals(point2);
        }

        /// <inheritdoc />
        public static bool operator !=(Point point1, Point point2)
        {
            return !(point1 == point2);
        }

        /// <inheritdoc />
        public static bool operator >(Point point1, Point point2)
        {
            return point1.CompareTo(point2) > 0;
        }

        /// <inheritdoc />
        public static bool operator <(Point point1, Point point2)
        {
            return point1.CompareTo(point2) < 0;
        }

        /// <inheritdoc />
        public static bool operator >=(Point point1, Point point2)
        {
            return point1.CompareTo(point2) >= 0;
        }

        /// <inheritdoc />
        public static bool operator <=(Point point1, Point point2)
        {
            return point1.CompareTo(point2) <= 0;
        }

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (!(obj is Point)) return -1;
            var other = (Point) obj;
            if (this.X > other.X)
                return 1;
            if (this.X < other.X)
                return -1;
            if (this.Y > other.Y)
                return 1;
            if (this.Y < other.Y)
                return -1;
            return 0;
        }

        /// <summary>
        /// Calculates the squared distance between this instance and <paramref name="other"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double DistanceToSquared(Point other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return (this.X - other.X) * (this.X - other.X) +
                   (this.Y - other.Y) * (this.Y - other.Y);
        }

        /// <summary>
        /// Calculates the distance between this instance and <paramref name="other"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double DistanceTo(Point other)
        {
            if (other == null) 
                throw new ArgumentNullException(nameof(other));
            return Math.Sqrt(DistanceToSquared(other));
        }

        /// <summary>
        /// Checks whether this instance is to the left of <paramref name="edge"/>
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool IsLeftOf(QuadEdge edge)
        {
            return IsCounterClockWise(this, edge.Origin, edge.Destination);
        }

        /// <summary>
        /// Checks whether this instance is to the right of <paramref name="edge"/>
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool IsRightOf(QuadEdge edge)
        {
            return IsCounterClockWise(this, edge.Destination, edge.Origin);
        }

        /// <summary>
        /// Determines if parameters <paramref name="a"/>, <paramref name="b"/>, and <paramref name="c"/> are in a counterclockwise order
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsCounterClockWise(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) > 0.0f;
        }

        /// <summary>
        /// Checks whether this instance is within a circle defined by <paramref name="a"/>, <paramref name="b"/>, and <paramref name="c"/>
        /// </summary>
        /// <remarks>Points on the circle are not considered to be inside.</remarks>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>Whether this instance is inside the Circle</returns>
        public bool InCircle(Point a, Point b, Point c)
        {
            if ((a == this) || (b == this) || (c == this))
                return false; 

            float aNorm = a.X * a.X + a.Y * a.Y;
            float bNorm = b.X * b.X + b.Y * b.Y;
            float cNorm = c.X * c.X + c.Y * c.Y;
            float dNorm = X * X + Y * Y;

            float adx = a.X - X;
            float ady = a.Y - Y;
            float bdx = b.X - X;
            float bdy = b.Y - Y;
            float cdx = c.X - X;
            float cdy = c.Y - Y;

            return 0.0f < ((aNorm - dNorm) * (bdx * cdy - bdy * cdx) +
                           (bNorm - dNorm) * (cdx * ady - cdy * adx) +
                           (cNorm - dNorm) * (adx * bdy - ady * bdx));
        }
    }
}
