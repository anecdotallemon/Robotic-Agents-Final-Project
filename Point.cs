﻿namespace DefaultNamespace {
    
    /// <summary>
    /// Representation of a 2D point in space with integer coefficients. Can be added and subtracted with other points. Immutable.
    /// </summary>
    class Point {
        
            public readonly int x;
            public readonly int y;
            
            /// <summary>
            /// Construct a new <c>Point</c> with the given values.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public Point(int x, int y) {
                this.x = x;
                this.y = y;
            }
            
            /// <summary>
            /// Construct a copy of the given <c>Point</c>.
            /// </summary>
            /// <param name="p">The <c>Point</c> to copy</param>
            public Point(Point p) {
                this.x = p.x;
                this.y = p.y;
            }

            public static Point operator +(Point a) => a;
            public static Point operator -(Point b) => new Point(-b.x, -b.y);
            public static Point operator +(Point a, Point b) => new Point(a.x + b.x, a.y + b.y);
            public static Point operator -(Point a, Point b) => a + (-b);
            
            /// <summary>
            /// Get the square magnitude of the current <c>Point</c>, that is, the square of its distance from the origin.
            /// </summary>
            /// <returns>The square magnitude as an integer</returns>
            public int Mag2() {
                return x * x + y * y;
            }
            
            public override string ToString() {
                return "(" + x + ", " + y + ")";
            }
            
            /// <summary>
            /// Whether this <c>Point</c> is within the boundaries of the game.
            /// </summary>
            /// <returns>Returns <code>true</code> if and only if the <c>Point</c> is within the game bounds, <code>false</code> otherwise.</returns>
            public bool IsOutOfBounds() {
                return x >= Params.WIDTH || x < 0 || y >= Params.HEIGHT || y < 0;
            }
            
            /// <summary>
            /// Whether this <c>Point</c> is considered equal to a given object.
            /// </summary>
            /// <param name="obj">The object to compare against</param>
            /// <returns><code>true</code> if and only if the other object is a <c>Point</c> with identical values, <code>false</code> otherwise.</returns>
            public override bool Equals(Object obj) {
                if (obj == null || obj.GetType() != typeof(Point)) return false;
                Point p = (Point) obj;
                return (p.x == x && p.y == y);
            }
            
            /// <summary>
            /// Returns a semi-unique identifier for the given <c>Point</c>.
            /// </summary>
            /// <returns>An integer representing the <c>Point</c></returns>
            public override int GetHashCode() {
                // this implementation assumes small values and so is technically not suitable for general purposes
                // but it should work just fine for the entirety of the robotics class
                return x << 16 + y;
            }
        }
    
}