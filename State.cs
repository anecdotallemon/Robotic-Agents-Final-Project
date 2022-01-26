using System;

namespace Robotic_Agents_Final_Project {
    public class State {

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

    }
}