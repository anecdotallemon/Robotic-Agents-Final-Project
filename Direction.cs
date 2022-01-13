using System;
using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    
    /// <summary>
    /// "Pseudo-enum" representing cardinal directions in 2D space
    /// </summary>
    class Direction {
        
        #region Static
        
        /// <summary>
        /// Constant field representing the direction up.
        /// </summary>
        public static readonly Direction Up = new Direction(new Point(0, -1));
        /// <summary>
        /// Constant field representing the direction right.
        /// </summary>
        public static readonly Direction Right = new Direction(new Point(1, 0));
        /// <summary>
        /// Constant field representing the direction down.
        /// </summary>
        public static readonly Direction Down = new Direction(new Point(0, 1));
        /// <summary>
        /// Constant field representing the direction left.
        /// </summary>
        public static readonly Direction Left = new Direction(new Point(-1, 0));
        
        /// <summary>
        /// A list of all cardinal <c>Direction</c>s, for iterating over
        /// </summary>
        public static readonly Direction[] Directions = {Up, Right, Down, Left};
        
        /// <summary>
        /// A dictionary organized by the <c>Point</c> offset of each direction, for converting offsets to <c>Direction</c>s.
        /// </summary>
        public static readonly Dictionary<Point, Direction> FromOffset = new Dictionary<Point, Direction>() {
            {Up.Offset, Up},
            {Right.Offset, Right},
            {Down.Offset, Down},
            {Left.Offset, Left}
        };
        
        /// <summary>
        /// Given two points, return the <c>Direction<c> that moves between them.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>A <c>Direction</c> representing the vector between the given <c>Point</c>s.</returns>
        /// <exception cref="ArgumentException">If the two points are not exactly one unit apart</exception>
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
        
        /// <summary>
        /// A <c>Point</c> representing one space of movement in the given direction.
        /// </summary>
        public readonly Point Offset;

        private Direction(Point p) {
            Offset = p;
        }
        
        /// <summary>
        /// Apply the <c>Direction</c> to the given <c>Point</c>, returning a copy moved in this direction one square.
        /// </summary>
        /// <param name="p">The <c>Point</c> to move</param>
        /// <returns>A copy of the given <c>Point</c>, moved one square in this <c>Direction</c></returns>
        public Point ApplyToPoint(Point p) {
            return Offset + p;
        }

        #endregion
    }
}