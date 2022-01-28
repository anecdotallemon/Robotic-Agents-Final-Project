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
            while (true) {
                var input = Parser.ParseInput();
                state.InitializeForTurn(input.myScore, input.opponentScore, input.pacmans, input.pellets);
                List<Pacman> playerPacs = state.MyPacs;

                foreach (Pacman p in playerPacs) {
                    var action = state.GetBestAction();
                    Parser.OutputMove(p, action);
                }
                
            }
        }
    }
}