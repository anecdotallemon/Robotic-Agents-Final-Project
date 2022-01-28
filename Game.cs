using System;
using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    /**
 * Grab the pellets as fast as you can!
 **/
    class Game
    {
        static void Main(string[] args) {
            State state = new State(Parser.GetMapLayout());
            
            // game loop
            while (true)
            {
                state.InitializeForTurn();
                List<Pacman> playerPacs = state.MyPacs;

                foreach (Pacman p in playerPacs) {
                    // get best game move, add it to the list to be added to the string of all the moves
                }
                

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                Console.WriteLine("MOVE 0 10 15"); // MOVE <pacId> <x> <y>
            }
        }
    }
}