using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DeLaunay.NET.Shapes;

namespace DeLaunay.NET
{
    public class DeLaunayTriangulator
    {
        private readonly Point[] _points;

        private DeLaunayTriangulator(Point[] points)
        {
            this._points = points;
        }

        private static List<Point> GetResult(Partition partition)
        {
            var result = new List<Point>();
            QuadEdge baseEdge = QuadEdge.ConnectLeft(partition.Left.Inverse, partition.Right);
            QuadEdge e = baseEdge.DestinationNext.Inverse;
            Point u = baseEdge.Destination;
            while (true)
            {
                while (e != baseEdge && !CanMoveForward(e.DestinationNext, baseEdge))
                {
                    u = e.Destination;
                    e = e.DestinationNext.Inverse;
                }

                if (e != baseEdge)
                {
                    result.Add(e.Origin);
                    result.Add(e.Destination);
                }
                
                while (!CanMoveForward(e.OriginNext, baseEdge))
                {
                    result.Add(u);
                    if (u == baseEdge.Destination)
                    {
                        baseEdge.Delete();
                        return result;
                    }
                    e = e.OriginNext.Inverse;
                    while (CanMoveForward(e.DestinationNext, baseEdge))
                        e = e.DestinationNext;
                    u = e.Origin;

                    result.Add(e.Origin);
                    result.Add(e.Destination);
                }
                e = e.OriginNext;
            }
        }

        private static bool CanMoveForward(QuadEdge e, QuadEdge baseEdge)
        {
            if (e == baseEdge) return true;
            if (e == baseEdge.Inverse) return false;
            return e.Origin.X > e.Destination.X;
        }

        public static List<Point> Triangulate(IEnumerable<Point> points)
        {
            var triangulator =
                new DeLaunayTriangulator(
                    points.Where(p => p != null).Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToArray());
            Task<Partition> result = triangulator.Subdivide(0, triangulator._points.Length - 1);
            return GetResult(result.Result);
        }

        private async Task<Partition> Subdivide(int lidx, int ridx)
        {
            Debug.Assert(lidx != ridx);
            if (ridx - lidx == 1) //2 points
            {
                QuadEdge edge = QuadEdge.MakeEdge(this._points[lidx], this._points[lidx + 1]);
                return new Partition
                {
                    Left = edge,
                    Right = edge.Inverse
                };
            }
            if (ridx - lidx == 2) //3 points
            {
                QuadEdge a = QuadEdge.MakeEdge(this._points[lidx], this._points[lidx + 1]);
                QuadEdge b = QuadEdge.MakeEdge(this._points[lidx + 1], this._points[lidx + 2]);
                QuadEdge.Splice(a.Inverse, b);

                if (Point.IsCounterClockWise(this._points[lidx], this._points[lidx + 1], this._points[lidx + 2]))
                {
                    QuadEdge c = QuadEdge.ConnectLeft(b, a);
                    return new Partition
                    {
                        Left = a,
                        Right = b.Inverse
                    };
                }
                if (Point.IsCounterClockWise(this._points[lidx], this._points[lidx + 2], this._points[lidx + 1]))
                {
                    QuadEdge c = QuadEdge.ConnectLeft(b, a);
                    return new Partition
                    {
                        Left = c.Inverse,
                        Right = c
                    };
                }
                return new Partition
                {
                    Left = a,
                    Right = b.Inverse
                };
            }
            int midx = (lidx + ridx) / 2;
            Task<Partition> leftTask = Subdivide(lidx, midx);
            Task<Partition> rightTask = Subdivide(midx + 1, ridx);
            return Merge(await leftTask, await rightTask);
        }

        private static Partition Merge(Partition leftPartition, Partition rightPartition)
        {
            QuadEdge left = leftPartition.Left; //ldo
            QuadEdge right = rightPartition.Right; //rdo
            LowestCommonTangent(leftPartition, rightPartition, out QuadEdge lowLeft, out QuadEdge lowRight); //ldi, rdi

            QuadEdge edgeBase = QuadEdge.ConnectLeft(lowRight.Inverse, lowLeft);
            QuadEdge lcand = edgeBase.RightPrevious;
            QuadEdge rcand = edgeBase.OriginPrevious;
            if (edgeBase.Origin == right.Origin)
                right = edgeBase;
            if (edgeBase.Destination == left.Origin)
                left = edgeBase.Inverse;

            while (true)
            {
                QuadEdge temp = lcand.OriginNext;
                if (Point.IsCounterClockWise(edgeBase.Origin, temp.Destination, edgeBase.Destination))
                    while (edgeBase.Origin.InCircle(lcand.Destination, temp.Destination, lcand.Origin))
                    {
                        lcand.Delete();
                        lcand = temp;
                        temp = lcand.OriginNext;
                    }

                temp = rcand.OriginPrevious;
                if (Point.IsCounterClockWise(edgeBase.Origin, temp.Destination, edgeBase.Destination))
                    while (edgeBase.Destination.InCircle(temp.Destination, rcand.Destination, rcand.Origin))
                    {
                        rcand.Delete();
                        rcand = temp;
                        temp = rcand.OriginPrevious;
                    }

                bool leftValid = Point.IsCounterClockWise(edgeBase.Origin, lcand.Destination, edgeBase.Destination);
                bool rightValid = Point.IsCounterClockWise(edgeBase.Origin, rcand.Destination, edgeBase.Destination);
                if (!leftValid && !rightValid)
                    break;

                if (!leftValid ||
                    rightValid && rcand.Destination.InCircle(lcand.Destination, lcand.Origin, rcand.Origin))
                {
                    edgeBase = QuadEdge.ConnectLeft(rcand, edgeBase.Inverse);
                    rcand = edgeBase.Inverse.LeftNext;
                }
                else
                {
                    edgeBase = QuadEdge.ConnectRight(lcand, edgeBase).Inverse;
                    lcand = edgeBase.RightPrevious;
                }
            }

            return new Partition
            {
                Left = left,
                Right = right
            };
        }

        private static void LowestCommonTangent(Partition left, Partition right, out QuadEdge leftLowest,
            out QuadEdge rightLowest)
        {
            leftLowest = left.Right;
            rightLowest = right.Left;
            while (true)
                if (rightLowest.Origin.IsLeftOf(leftLowest))
                    leftLowest = leftLowest.LeftNext;
                else if (leftLowest.Origin.IsRightOf(rightLowest))
                    rightLowest = rightLowest.RightPrevious;
                else break;
        }
    }

    internal class Partition
    {
        public QuadEdge Left;
        public QuadEdge Right;
    }
}