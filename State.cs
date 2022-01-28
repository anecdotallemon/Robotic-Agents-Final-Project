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
            var ParserReturn = Parser.ParseInput();
            PlayerScore = ParserReturn.MyScore;
            OpponentScore = ParserReturn.OpponentScore;

			MyPacs.Clear();
			Enemies.Clear();
			_turnOrder.Clear();

            Pacman[] TempMen = ParserReturn.pacmans;
			Pellet[] TempPellets = ParserReturn.pellets;
            foreach (Pacman pac in TempMen) {
				_scores[pac.Location.x, pac.Location.y] = 0;
				if (!pac.Type.ToString() == "DEAD") {
					if (pac.IsOurPlayer) {
						MyPacs.Add(pac);
						_turnOrder.Add(pac);
						_allPlayers.Add(pac);
					}
					else {
						updateVisiblePellets(pac.Location.x, pac.Location.y, tempPellets);
						Enemies.Add(pac);
						_turnOrder.Add(pac);
						_allPlayers.Add(pac);
					}
				}
			}

			// initializing turn order
			foreach (Pacman pac in MyPacs) {
				_turnOrder.Add(pac);
			}
			foreach (Pacman pac in Enemies) {
				_turnOrder.Add(pac);
			}
            
			// initializing visible pellets, to try to catch any big pellets that escaped the updateVisiblePellets
            Pellet[] TempPellets = ParserReturn.pellets;
			foreach (Pellet pellet in TempPellets) {
				_scores[pellet.Location.x, pellet.Location.y] = pellet.Score;
			}
        }


		private void updateVisiblePellets(int tempX, int tempY, Pellet[] tempPellets) {
			// north
			if (tempY > 0) {
				if (!_walls[tempX, tempY - 1]) {
					// if the score isn't 0, see if it needs to be updated
					if (_score[tempX, tempY - 1] != 0) {
						foreach (Pellet p in tempPellets) {
							if (p.Location.x == tempX && p.Location.y == tempY - 1) {
								_score[tempX, tempY - 1] = p.Score;
							}
							else {
								_score[tempX, tempY - 1] = 0;
							}
						}
					}
					return updateDirectionally(tempX, tempY - 1);
				}
			}
			// south
			if (tempY < Height) {
				if (!_walls[tempX, tempY + 1]) {
					// if the score isn't 0, see if it needs to be updated
					if (_score[tempX, tempY + 1] != 0) {
						foreach (Pellet p in tempPellets) {
							if (p.Location.x == tempX && p.Location.y == tempY + 1) {
								_score[tempX, tempY + 1] = p.Score;
							}
							else {
								_score[tempX, tempY + 1] = 0;
							}
						}
					}
					return updateDirectionally(tempX, tempY + 1);
				}
			}
			// east
			if (tempX < Width) {
				if (!_walls[tempX + 1, tempY]) {
					// if the score isn't 0, see if it needs to be updated
					if (_score[tempX + 1, tempY] != 0) {
						foreach (Pellet p in tempPellets) {
							if (p.Location.x == tempX + 1 && p.Location.y == tempY) {
								_score[tempX + 1, tempY] = p.Score;
							}
							else {
								_score[tempX + 1, tempY] = 0;
							}
						}
					}
					return updateDirectionally(tempX + 1, tempY);
				}
			}
			// west
			if (tempX > 0) {
				if (!_walls[tempX - 1, tempY]) {
					// if the score isn't 0, see if it needs to be updated
					if (_score[tempX - 1, tempY] != 0) {
						foreach (Pellet p in tempPellets) {
							if (p.Location.x == tempX - 1 && p.Location.y == tempY) {
								_score[tempX - 1, tempY] = p.Score;
							}
							else {
								_score[tempX - 1, tempY] = 0;
							}
						}
					}
					return updateDirectionally(tempX - 1, tempY);
				}
			}
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
            
            if(move.actionType.Equals( ActionType.FromString["MOVE "]) ){
            turnPac.Location = move.gameActions;
            // decreses ability cool down after each move 
                if(turnPac._abilityCooldown !=0 ){
                    turnPac._abilityCooldown =  turnPac._abilityCooldown  -1;
                }
                if(turnPac._speedTurnsLeft != 0){
                    turnPac._speedTurnsLeft = turnPac._speedTurnsLeft -1; 
                }
            }
            else if(move.actionType.Equals(ActionType.FromString["SPEED "]) ){
                // startCoolDown is 10 for now
                turnPac._abilityCooldown = Pacman.startCoolDown ;
                turnPac._speedTurnsLeft =  Pacman.startCoolDown ;
            }
            else if( move.actionType.Equals(ActionType.FromString["SWITCH "])){
                turnPac.Type = move.pacSwitch;
                turnPac._abilityCooldown = Pacman.startCoolDown ;
            }
            _turnOrder.Enqueue(turnPac);
            
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
            if(turnPac._speedTurnsLeft != 0){
                foreach (Point p in actions){
                    foreach (Direction d in Direction.Directions) {
                        var newPoint = d.ApplyToPoint(p).Wrap(Width, Height);
                        if (!IsWall(d.ApplyToPoint(p).Wrap(Width, Height))) {
                            actions.Add(newPoint);
                        }
                    }   
                }
            }
            
            List<GameAction>kids; 
            for (int j = 0; j< actions.Count; j++){
                kids.Add(new GameAction(actions[j], "MOVE", turnPac.Type));
            }
            if(turnPac._abilityCooldown == 0){
                    kids.Add(new GameAction(turnPac.Location, "SPEED", turnPac.Type));
                    kids.Add(new GameAction(turnPac.Location, "SWITCH", SwicthPac.SwicthOptions(turnPac.Type, "PREY")));
                    kids.Add(new GameAction(turnPac.Location, "SWITCH", SwicthPac.SwicthOptions(turnPac.Type, "PREDATOR")));
            }

            return kids;

        }
        
        
        public int EstimateUtility() {
			int est = 0;
			est += FloodFill();
            // TODO if enemy pac in sight is of the "weaker" type to our pac, ++
			// Get current player, then get enemy pacs in sight (maybe just if there's one close enough?), then CompareTypeTo

            // TODO if enemy pac in sight is same type, ==
			// Same hat as above
			
            // TODO if enemy pac in sight is stronger type, -- (run away!) possibly implement here if it's close enough to change?
			// Same hat as above
			
            // TODO if a friendly pac is in sight, -- (we want pacs to be further away) (this may be duplicated by flood fill)
			// Same hat as above but with friendly pacs and no type comparison
			
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