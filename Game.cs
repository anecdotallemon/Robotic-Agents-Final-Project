using System;

namespace Robotic_Agents_Final_Project {
    /**
 * Grab the pellets as fast as you can!
 **/
    class Game
    {
        static void Main(string[] args) {
            State state = new State(Parser.GetMapLayout());
            
            // game loop
            while (true) {
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                Console.WriteLine("MOVE 0 10 15"); // MOVE <pacId> <x> <y>
            }
        }
    }
}