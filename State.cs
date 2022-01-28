using System;
using System.Collections.Generic;
using Robotic_Agents_Final_Project;

namespace Robotic_Agents_Final_Project
{
    public class State
    {

        // one list for our pacs and one list for enemies, since we may want to keep track of where we ares
        public List<Pacman> MyPacs { get; private set; }
        public List<Pacman> Enemies { get; private set; }
        public Point[,] GameBoard;

        private Queue<Pacman> _turnOrder = new Queue<Pacman>(); // all alive players, sorted by turn order
        private List<Pacman> _allPlayers = new List<Pacman>(); // list of all players, including dead ones, sorted by initial turn order

        private bool[,] _walls; // true at wall, false at open space
        private int[,] _scores; // 0 at blank, 1 at pellet, 10 at super pellet

        private int PlayerScore;
        private int OpponentScore;

        private int _combatScoreThisTurn = 0;
        
        public readonly int Width;
        public readonly int Height;

        /// <summary>
        /// Constructs a new state with the given wall setup. Copies the wall array to a new spot in memory.
        /// Also initializes score to 1 at any empty spot.
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
                    if (!walls[i, j]) {
                        _scores[i, j] = 1;
                    }
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
                    stateCopy._scores[i, j] = _scores[i, j];
                }
            }

            return stateCopy;
        }

        public void InitializeForTurn() { // to capture the new information

            _combatScoreThisTurn = 0; // probably not necessary as this is usually only read and set in fictional states, but good practice just in case
            
            var ParserReturn = Parser.ParseInput();
            PlayerScore = ParserReturn.myScore;
            OpponentScore = ParserReturn.opponentScore;

			MyPacs.Clear();
			Enemies.Clear();
			_turnOrder.Clear();

            Pacman[] tempMen = ParserReturn.pacmans;
			Pellet[] pellets = ParserReturn.pellets;
            
            HashSet<Point> visiblePoints = new HashSet<Point>();
            
            foreach (Pacman pac in tempMen) {
				_scores[pac.Location.x, pac.Location.y] = 0;
				
				if (pac.Type == PacType.Dead) {
					if (pac.IsOurPlayer) {
						MyPacs.Add(pac);
						_allPlayers.Add(pac);
                        
                        // add all points in a cardinal direction to our set of known visible points
                        foreach (Direction d in Direction.Directions) {
                            GetVisiblePoints(pac.Location, d, visiblePoints);
                        }
                    }
					else {
                        Enemies.Add(pac);
						_allPlayers.Add(pac);
					}
				}
			}
            
            // go through our list of pellets and update score values, removing them from the visible points set as we go
            foreach (var pellet in pellets) {
                _scores[pellet.Location.x, pellet.Location.y] = pellet.Score;
                visiblePoints.Remove(pellet.Location);
            }
            
            // any remaining points in visible points are those that have no pellets but are visible, meaning they should be set to zero
            foreach (var p in visiblePoints) {
                _scores[p.x, p.y] = 0;
            }

            // initializing turn order
			foreach (Pacman pac in MyPacs) {
				_turnOrder.Enqueue(pac);
			}
			foreach (Pacman pac in Enemies) {
				_turnOrder.Enqueue(pac);
			}
            
        }


		private void GetVisiblePoints(Point start, Direction d, HashSet<Point> visiblePoints) {

            Point newPoint = d.ApplyToPoint(start);

            while (!IsWall(newPoint)) {
                visiblePoints.Add(newPoint);
                newPoint = d.ApplyToPoint(newPoint);
            }

        }

        public Pacman GetCurrentPlayer() {
            return _turnOrder.Peek();
        }


        /// <summary>
        /// This method evaluates all possible actions and returns the action with the best utility. 
        /// </summary>
        public GameAction GetBestAction() {
            List<GameAction> actions = GetMoves();
            GameAction bestAction = null;
            double bestUtility = double.NegativeInfinity;

            foreach (GameAction action in actions) {
                State child = this.Clone();
                child.MakeMove(action);
                double utility = child.EstimateUtility();
                if (utility >= bestUtility) {
                    bestAction = action;
                    bestUtility = utility;
                }
            }

            return bestAction;
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
            _combatScoreThisTurn = 0;

            Pacman turnPac = _turnOrder.Dequeue();
            turnPac.Move(move);
            _turnOrder.Enqueue(turnPac);
            
            CheckCombat(turnPac);
        }
        
        private void CheckCombat(Pacman subject) {
            
            var opponents = subject.IsOurPlayer ? Enemies : MyPacs;

            List<Pacman> honoredDead = new List<Pacman>();

            foreach (var opponent in opponents) {
                if (opponent.Location == subject.Location) {
                    
                    var combatResult = subject.Combat(opponent);
                    var scoreDelta = combatResult * Params.KillReward * (subject.IsOurPlayer ? 1 : -1);
                    scoreDelta /= (subject.IsOurPlayer ? MyPacs.Count : Enemies.Count); // the more we kill, the more valuable later kills are. losing your last pac is a lot worse than losing your first
                    _combatScoreThisTurn += scoreDelta;

                    if (combatResult < 0) break;
                    
                    if (combatResult > 0) {
                        honoredDead.Add(opponent);
                    }
                }
            }

            foreach (var pac in honoredDead) {
                pac.Kill();
            }
            
        }

        //
        public List<GameAction> GetMoves() {
            Pacman turnPac = _turnOrder.Peek();
            List<Point> actions = new List<Point>();

            foreach (Direction d in Direction.Directions) {
                var newPoint = d.ApplyToPoint(turnPac.Location).Wrap(Width, Height);
                if (!IsWall(d.ApplyToPoint(turnPac.Location).Wrap(Width, Height))) {
                    actions.Add(newPoint);
                }
            }
            // if we have a speed boost we go through all the points in actions 
            // and do what we did before to get all points we can go to in 1 turn with speed boost 
            if(turnPac.SpeedTurnsLeft != 0){
                foreach (Point p in actions){
                    foreach (Direction d in Direction.Directions) {
                        var newPoint = d.ApplyToPoint(p).Wrap(Width, Height);
                        if (!IsWall(d.ApplyToPoint(p).Wrap(Width, Height))) {
                            actions.Add(newPoint);
                        }
                    }   
                }
            }

            List<GameAction> kids = new List<GameAction>();
            
            for (int j = 0; j< actions.Count; j++){
                kids.Add(new GameAction(actions[j], ActionType.Move, turnPac.Type));
            }
            if(turnPac.AbilityCooldown == 0){
                    kids.Add(new GameAction(turnPac.Location, ActionType.Speed, turnPac.Type));
                    kids.Add(new GameAction(turnPac.Location, ActionType.Switch, SwitchPac.SwitchOptions(turnPac.Type, "PREY")));
                    kids.Add(new GameAction(turnPac.Location, ActionType.Switch, SwitchPac.SwitchOptions(turnPac.Type, "PREDATOR")));
            }

            return kids;

        }
        
        
        public double EstimateUtility() {
			double est = double.NegativeInfinity;
			est += FloodFill();

            est += _combatScoreThisTurn;
            
            // TODO if enemy pac in sight is of the "weaker" type to our pac, ++
			// Get current player, then get enemy pacs in sight (maybe just if there's one close enough?), then compare

            // TODO if enemy pac in sight is same type, ==
			// Same hat as above
			
            // TODO if enemy pac in sight is stronger type, -- (run away!) possibly implement here if it's close enough to change?
			// Same hat as above
			
            // TODO if a friendly pac is in sight, -- (we want pacs to be further away) (this may be duplicated by flood fill)
			// Same hat as above but with friendly pacs and no comparison
			
            return est;
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