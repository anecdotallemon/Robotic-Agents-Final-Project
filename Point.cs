namespace DefaultNamespace {
    
    class Point {
        
            public readonly int x;
            public readonly int y;

            public Point(int x, int y) {
                this.x = x;
                this.y = y;
            }

            public Point(Point p) {
                this.x = p.x;
                this.y = p.y;
            }

            public static Point operator +(Point a) => a;
            public static Point operator -(Point b) => new Point(-b.x, -b.y);
            public static Point operator +(Point a, Point b) => new Point(a.x + b.x, a.y + b.y);
            public static Point operator -(Point a, Point b) => a + (-b);

            public int Mag2() {
                return x * x + y * y;
            }

            public override string ToString() {
                return "(" + x + ", " + y + ")";
            }

            public bool IsOutOfBounds() {
                return x >= Params.WIDTH || x < 0 || y >= Params.HEIGHT || y < 0;
            }

            public override bool Equals(Object obj) {
                if (obj == null || obj.GetType() != typeof(Point)) return false;
                Point p = (Point) obj;
                return (p.x == x && p.y == y);
            }

            public override int GetHashCode() {
                // this implementation assumes small values and so is technically not suitable for general purposes
                // but it should work just fine for the entirety of the robotics class
                return x << 16 + y;
            }
        }
    
}