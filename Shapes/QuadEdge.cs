using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLaunay.NET.Shapes
{
    public class QuadEdge
    {
        /// <summary>
        /// Private <see cref="QuadEdge"/> constructor
        /// </summary>
        /// <param name="origin"><see cref="Origin"/> of this instance</param>
        private QuadEdge(Point origin)
        {
            this.Origin = origin;
        }

        /// <summary>
        /// Creates a QuadEdge instance from <paramref name="origin"/> to <paramref name="destination"/>
        /// </summary>
        /// <param name="origin"><see cref="Origin"/> of the <see cref="QuadEdge"/> to create</param>
        /// <param name="destination"><see cref="Destination"/> of the <see cref="QuadEdge"/> to create</param>
        /// <returns></returns>
        public static QuadEdge MakeEdge(Point origin, Point destination)
        {
            //Create all quad edges
            var q0 = new QuadEdge(origin);
            var q1 = new QuadEdge(null);
            var q2 = new QuadEdge(destination);
            var q3 = new QuadEdge(null);

            //Define links between edges
            q0.OriginNext = q0;
            q1.OriginNext = q3;
            q2.OriginNext = q2;
            q3.OriginNext = q1;
            q0.DualEdge = q1;
            q1.DualEdge = q2;
            q2.DualEdge = q3;
            q3.DualEdge = q0;

            return q0;
        }

        /// <summary>
        /// Origin <see cref="Point"/> of this instance
        /// </summary>
        public Point Origin { get; private set; }

        /// <summary>
        /// The end point of this instance
        /// </summary>
        public Point Destination => this.Inverse.Origin;

        /// <summary>
        /// Next <see cref="QuadEdge"/> from the <see cref="Origin"/> of this instance
        /// </summary>
        public QuadEdge OriginNext { get; set; }

        /// <summary>
        /// Previous <see cref="QuadEdge"/> from the <see cref="Origin"/> of this instance
        /// </summary>
        public QuadEdge OriginPrevious => this.DualEdge.OriginNext.DualEdge;

        /// <summary>
        /// The dual of this instance
        /// </summary>
        /// <remarks>Defined as Rot in Guibas and Stolfi</remarks>
        public QuadEdge DualEdge { get; set; }

        /// <summary>
        /// The Inverse of the Dual of this instance
        /// </summary>
        /// <remarks>Defined as InvRot in Guibas and Stolfi</remarks>
        public QuadEdge InverseDual => this.DualEdge.Inverse;

        /// <summary>
        /// Inverse of this instance
        /// </summary>
        /// <remarks>Defined as Sym in Guibas and Stolfi</remarks>
        public QuadEdge Inverse => this.DualEdge.DualEdge;

        /// <summary>
        /// The next <see cref="QuadEdge"/> from the <see cref="Destination"/> of this instance
        /// </summary>
        public QuadEdge DestinationNext => this.Inverse.OriginNext.Inverse;

        /// <summary>
        /// The previous <see cref="QuadEdge"/> from this <see cref="Destination"/> of this instance
        /// </summary>
        public QuadEdge DestinationPrevious => this.InverseDual.OriginNext.InverseDual;

        /// <summary>
        /// The next <see cref="DualEdge"/> to the left of this instance
        /// </summary>
        public QuadEdge LeftNext => this.InverseDual.OriginNext.DualEdge;

        /// <summary>
        /// The previous <see cref="DualEdge"/> to the left of this instance
        /// </summary>
        public QuadEdge LeftPrevious => this.OriginNext.Inverse;

        /// <summary>
        /// The next <see cref="DualEdge"/> to the right of this instance
        /// </summary>
        public QuadEdge RightNext => this.DualEdge.OriginNext.InverseDual;

        /// <summary>
        /// The previous <see cref="DualEdge"/> to the right of this instance
        /// </summary>
        public QuadEdge RightPrevious => this.Inverse.OriginNext;

        /// <summary>
        /// The Magnitude of this instance
        /// </summary>
        public double Magnitude => this.Origin.DistanceTo(this.Destination);

        /// <summary>
        /// Splices two edges together
        /// </summary>
        /// <param name="a">First instance to splice</param>
        /// <param name="b">Second instance to splice</param>
        public static void Splice(QuadEdge a, QuadEdge b)
        {
            QuadEdge alpha = a.OriginNext.DualEdge;
            QuadEdge beta = b.OriginNext.DualEdge;
            QuadEdge temp = a.OriginNext;
            QuadEdge temp2 = beta.OriginNext;
            QuadEdge temp3 = alpha.OriginNext;

            a.OriginNext = b.OriginNext;
            b.OriginNext = temp;
            alpha.OriginNext = temp2;
            beta.OriginNext = temp3;
        }

        /// <summary>
        /// Connects two <see cref="QuadEdge"/> instances to the left of each other
        /// </summary>
        /// <param name="a">First instance to connect</param>
        /// <param name="b">Second instance to connect</param>
        /// <returns></returns>
        public static QuadEdge ConnectLeft(QuadEdge a, QuadEdge b)
        {
            QuadEdge result = MakeEdge(a.Destination, b.Origin);
            Splice(result, a.LeftNext);
            Splice(result.Inverse, b);
            return result;
        }

        /// <summary>
        /// Connects two <see cref="QuadEdge"/> instances to the right of each other
        /// </summary>
        /// <param name="a">First instance to connect</param>
        /// <param name="b">Second instance to connect</param>
        /// <returns></returns>
        public static QuadEdge ConnectRight(QuadEdge a, QuadEdge b)
        {
            QuadEdge result = MakeEdge(a.Destination, b.Origin);
            Splice(result, a.Inverse);
            Splice(result.Inverse, b.OriginPrevious);
            return result;
        }

        /// <summary>
        /// Remove this instance from the edges connected to it
        /// </summary>
        public void Delete()
        {
            Splice(this, this.OriginPrevious);
            Splice(this.Inverse, this.Inverse.OriginPrevious);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Origin}; {this.Destination}";
        }
    }
}
