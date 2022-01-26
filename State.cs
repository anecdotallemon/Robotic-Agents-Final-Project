using System;
using System.Collections.Generic;
using Robotic_Agents_Final_Project;

namespace Robotic_Agents_Final_Project
{
    public class State
    {

        // one list for our pacs and one list for enemies, since we may want to keep track of where we ares
        public List < Player > MyPacs { get; private set; }
        public List<Player> Enemies { get; private set; }
        public Point[,] GameBoard;

        private Queue<Player> _turnOrder = new Queue<Player>(); // all alive players, sorted by turn order
        private List<Player> _allPlayers = new List<Player>(); // list of all players, including dead ones, sorted by initial turn order

        private bool[,] _walls; // true at wall, false at open space
        private int[,] _scores; // 0 at blank, 1 at pellet, 10 at super pellet
        
        public readonly int Width;
        public readonly int Height;

        // Constructor with gameBoard initialization
        public State(int width, int height) {
            Width = width;
            Height = height;

            _walls = new bool[width, height];
            _scores = new int[width, height];
            
            // TODO: set scores to some average estimate???

        }
        
        public State Clone() {

            State stateCopy = new State(Width, Height);
        
            // clone all players
            foreach (Player p in _allPlayers) {

                Player playerCopy = p.Clone();
            
                // add to turn orders
                stateCopy._allPlayers.Add(playerCopy);
                if (p.Alive) {
                    stateCopy._turnOrder.Enqueue(playerCopy);
                }
            
                // add to lists
                if (p.IsOurPlayer) {
                    stateCopy.MyPacs.Add(playerCopy);
                }
                else {
                    stateCopy.Enemies.Add(playerCopy);
                }
            }
        
            // shuffle the copy's turn order queue to match ours
            while (!stateCopy._turnOrder.Peek().Equals(_turnOrder.Peek())) {
                stateCopy._turnOrder.Enqueue(stateCopy._turnOrder.Dequeue());
            }
        
        
            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    stateCopy._walls[i, j] = _walls[i,j];
                }
            }

            return stateCopy;
        }

        public void InitializeForFirstTurn() {
            throw new NotImplementedException();
        }

        public void UpdatePlayers() {
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