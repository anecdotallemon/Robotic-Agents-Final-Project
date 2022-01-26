using System;
using System.Collections.Generic;

namespace Robotic_Agents_Final_Project
{
    public class State
    {
        // Constructor with gameBoard initialization
        public State(int width, int height)
        {
            public Point[,] gameBoard = new[width, height];
        }

        // one list for our pacs and one list for enemies, since we may want to keep track of where we ares
        public List < Player > MyPacs { get; private set; }
        public List<Player> Enemies { get; private set; }

        public State Clone() {
            throw new NotImplementedException();
        }

        public Player GetCurrentPlayer() {
            throw new NotImplementedException();
        }

        public void MakeMove(GameAction move) {
            throw new NotImplementedException();
        }

        public GameAction[] GetMoves() {
            throw new NotImplementedException();
        }

        public int EstimateUtility() {
            // parallel flood fill measuring available pellets
            throw new NotImplementedException();
        }
        
        // kill current player
        public void KillCurrent() {
            throw new NotImplementedException();
        }

    }
}