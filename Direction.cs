namespace DefaultNamespace {
    class Direction {
        
        #region Static

        public static readonly Direction Up = new Direction(new Point(0, -1));
        public static readonly Direction Right = new Direction(new Point(1, 0));
        public static readonly Direction Down = new Direction(new Point(0, 1));
        public static readonly Direction Left = new Direction(new Point(-1, 0));

        public static readonly Direction[] Directions = {Up, Right, Down, Left};

        public static readonly Dictionary<Point, Direction> FromOffset = new Dictionary<Point, Direction>() {
            {Up.Offset, Up},
            {Right.Offset, Right},
            {Down.Offset, Down},
            {Left.Offset, Left}
        };

        public static Direction FromPoints(Point start, Point end) {
            try {
                return FromOffset[end - start];
            }
            catch (ArgumentException) {
                throw new ArgumentException("Points must be exactly one unit apart! Points were: " + start + " -> " +
                                            end);
            }
        }

        #endregion

        #region NonStatic

        public readonly Point Offset;

        private Direction(Point p) {
            Offset = p;
        }

        public Point ApplyToPoint(Point p) {
            return Offset + p;
        }

        #endregion
    }
}