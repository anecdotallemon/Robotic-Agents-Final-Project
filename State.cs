using System;
using System.Collections.Generic;
using Robotic_Agents_Final_Project;

namespace Robotic_Agents_Final_Project
{
    public class State
    {

        // one list for our pacs and one list for enemies, since we may want to keep track of where we ares
        public List <Pacman> MyPacs { get; private set; }
        public List<Pacman> Enemies { get; private set; }
        public Point[,] GameBoard;

        private Queue<Pacman> _turnOrder = new Queue<Pacman>(); // all alive players, sorted by turn order
        private List<Pacman> _allPlayers = new List<Pacman>(); // list of all players, including dead ones, sorted by initial turn order

        private bool[,] _walls; // true at wall, false at open space
        private int[,] _scores; // 0 at blank, 1 at pellet, 10 at super pellet
        
        public readonly int Width;
        public readonly int Height;

        /// <summary>
        /// Constructs a new state with the given wall setup. Copies the wall array to a new spot in memory
        /// </summary>
        /// <param name="walls">array of bools with true at wall locations</param>
        public State(bool[,] walls) {
            Width = walls.GetLength(0);
            Height = walls.GetLength(1);
            
            _walls = new bool[Width, Height];
            _scores = new int[Width, Height];
            
            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    _walls[i, j] = walls[i,j];
                }
            }
            
        }
        
        public State Clone() {

            State stateCopy = new State(_walls);
        
            // clone all players
            foreach (Pacman p in _allPlayers) {

                Pacman pacmanCopy = p.Clone();
            
                // add to turn orders
                stateCopy._allPlayers.Add(pacmanCopy);
                if (p.Alive) {
                    stateCopy._turnOrder.Enqueue(pacmanCopy);
                }
            
                // add to lists
                if (p.IsOurPlayer) {
                    stateCopy.MyPacs.Add(pacmanCopy);
                }
                else {
                    stateCopy.Enemies.Add(pacmanCopy);
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

        public void InitializeForFirstTurn() { // this was necessary for my original tron game -- im not sure if it will be here. idk.
            throw new NotImplementedException();
        }

        public void UpdatePlayers() {
            throw new NotImplementedException();
        }

        public Pacman GetCurrentPlayer() {
            return _turnOrder.Peek();
        }
        
        /// <summary>
        /// given a move, make the current player take that move, update the state as necessary (including checking for kills, removing pellets, setting player types, etc), and put the current player back at the end of the queue
        /// if it's a new location, the current player will move towards that location
        /// if it's a type change or speed boost (and is valid), the player will move towards their existing target but have that parameter changed
        /// should ONLY be used for predictive stuff, not for actual game updates
        /// </summary>
        /// <param name="move"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void MakeMove(GameAction move) {
            Pacman turnPac = _turnOrder.Dequeue();
            turnPac.Location = move.gameActions;
            _turnOrder.Enqueue(turnPac);
            
            // modify player score here and remove pellet from score board if necesarry 

            throw new NotImplementedException();

        }


        public GameAction[] GetMoves() {
            Pacman turnPac = _turnOrder.Peek();
            List<Point> actions = new List<Point>();

            foreach (Direction d in Direction.Directions) {
                var newPoint = d.ApplyToPoint(turnPac.Location).Wrap(Width, Height);
                if (!IsWall(d.ApplyToPoint(turnPac.Location).Wrap(Width, Height))) {
                    actions.Add(newPoint);
                }
            }
            
            GameAction[] kids = new GameAction[actions.Count];
            for (int j = 0; j< actions.Count; j++){
                kids[j] = new GameAction(actions[j]);
            }

            return kids;

        }
        
        
        public int EstimateUtility() {
            // parallel flood fill measuring available pellets
            throw new NotImplementedException();
        }
        
        // how many more points our pacs can access vs enemy pacs
        private int FloodFill() {
            
            Queue<Point>[] cellsToUpdate = new Queue<Point>[_turnOrder.Count];
            HashSet<Point>[] cellsToUpdateSet = new HashSet<Point>[_turnOrder.Count]; // used to check for duplicates in queue
            bool[,] cellsUpdated = new bool[Width, Height];

            bool stillCellsToUpdate = true;
            
            Pacman[] turnOrderCopy = _turnOrder.ToArray();
            
            int netPoints = 0;
            
            for (int i = 0; i < _turnOrder.Count; i++) {
                Point start = turnOrderCopy[i].Location;
                cellsToUpdate[i].Enqueue(start);
                cellsToUpdateSet[i].Add(start);
            }

    
            // while there are still adjacent cells to update
            while (stillCellsToUpdate) {

                stillCellsToUpdate = false;

                for (int i = 0; i < turnOrderCopy.Length; i++) {
                    // remove the next point
                    Point p = cellsToUpdate[i].Dequeue();
                    cellsToUpdateSet[i].Remove(p);
                    
                    if (cellsUpdated[p.x, p.y]) break;

                    cellsUpdated[p.x, p.y] = true;
        
                    // add relevant neighbors to queue
                    foreach (Direction d in Direction.Directions) {
                        Point neighbor = d.ApplyToPoint(p).Wrap(Width, Height);
                            
                        if (!neighbor.IsOutOfBounds() && !IsWall(neighbor) && !cellsUpdated[neighbor.x, neighbor.y]) {

                            bool isPlannedToUpdate = false;

                            for (int j = 0; j < turnOrderCopy.Length; j++) {
                                isPlannedToUpdate |= cellsToUpdateSet[j].Contains(neighbor);
                            }

                            if (!isPlannedToUpdate) {
                                cellsToUpdate[i].Enqueue(neighbor);
                                cellsToUpdateSet[i].Add(neighbor);
                            }
                        }
                    }

                    netPoints += _scores[p.x, p.y] * (turnOrderCopy[i].IsOurPlayer ? 1 : -1);

                    stillCellsToUpdate |= cellsToUpdateSet.Length > 0;
                }

            }
            
            return netPoints;
        }

        public bool IsWall(Point p) {
            return _walls[p.x, p.y];
        }
        
        // kill current player
        public void KillCurrent() {
            Kill(_turnOrder.Peek());
        }
        
        void Kill(Pacman victim) {
            // remove from enemies/mypacs
            // remove from turn order
            // mark dead
        
            if (victim.Alive == false) return; // silently return if already dead
            if (!_allPlayers.Contains(victim)) throw new ArgumentException("Player does not exist");

            if (victim.IsOurPlayer) {
                MyPacs.Remove(victim);
            }
            else {
                Enemies.Remove(victim);
            }
        
            // remove from turn order -- a bit fucky since it's a queue
            Pacman currentPlayer = _turnOrder.Peek();

            if (Equals(currentPlayer, victim)) {
                _turnOrder.Dequeue();
            }
            else {
                // loop through queue until found
                Pacman temp = _turnOrder.Dequeue();
                while (!Equals(temp, victim)) {
                    _turnOrder.Enqueue(temp);
                    temp = _turnOrder.Dequeue();
                }
                // end the loop without requeueing temp to remove it
            }

            victim.Kill();

        }

    }
}