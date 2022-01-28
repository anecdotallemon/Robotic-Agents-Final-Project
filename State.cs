using System;
using System.Collections.Generic;
using System.Text;
using Robotic_Agents_Final_Project;

namespace Robotic_Agents_Final_Project
{
    public class State
    {

        // one list for our pacs and one list for enemies, since we may want to keep track of where we ares
        public List<Pacman> MyPacs { get; private set; } = new List<Pacman>();
        public List<Pacman> Enemies { get; private set; } = new List<Pacman>();

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
                    // assume each blank spot has a pellet
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

            stateCopy.PlayerScore = PlayerScore;
            stateCopy.OpponentScore = OpponentScore;

            return stateCopy;
        }

        public void InitializeForTurn(int myScore, int opponentScore, Pacman[] pacs, Pellet[] pellets) { // to capture the new information

            _combatScoreThisTurn = 0; // probably not necessary as this is usually only read and set in fictional states, but good practice just in case
            
            PlayerScore = myScore;
            OpponentScore = opponentScore;

			MyPacs.Clear();
			Enemies.Clear();
			_turnOrder.Clear();
            _allPlayers.Clear();
            
            
            HashSet<Point> visiblePoints = new HashSet<Point>();
            
            foreach (Pacman pac in pacs) {
				_scores[pac.Location.x, pac.Location.y] = 0;
				
				if (pac.Type != PacType.Dead) {
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

            Console.Error.WriteLine(BoardToString());
            
        }


		private void GetVisiblePoints(Point start, Direction d, HashSet<Point> visiblePoints) {

            Point newPoint = d.ApplyToPoint(start).Wrap(Width, Height);

            while (!IsWall(newPoint)) {
                visiblePoints.Add(newPoint);
                newPoint = d.ApplyToPoint(newPoint).Wrap(Width, Height);
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
                Console.Error.WriteLine($"Pac: {GetCurrentPlayer().PacId}\nAction: {action}\nScore:{utility}");
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
            
            // update scores, eat pellets
            var score = GetScore(turnPac.Location);
            if (score > 0) {
                if (turnPac.IsOurPlayer) {
                    PlayerScore += score;
                }
                else {
                    OpponentScore += score;
                }

                _scores[turnPac.Location.x, turnPac.Location.y] = 0;
            }
            
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

                    if (combatResult < 0) {
                        honoredDead.Add(subject);
                        break;
                    }
                    
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
                if (!IsWall(newPoint)) {
                    actions.Add(newPoint);
                }
            }
            // if we have a speed boost we go through all the points in actions 
            // and do what we did before to get all points we can go to in 1 turn with speed boost 
            if(turnPac.SpeedTurnsLeft != 0) {

                var newActions = new List<Point>();
                
                foreach (Point p in actions) {
                    foreach (Direction d in Direction.Directions) {
                        newActions.Add(p);
                        
                        var newPoint = d.ApplyToPoint(p).Wrap(Width, Height);
                        if (!IsWall(newPoint) && newPoint != turnPac.Location) {
                            newActions.Add(newPoint);
                        }
                    }   
                }

                actions = newActions;
            }

            List<GameAction> kids = new List<GameAction>();
            
            for (int j = 0; j< actions.Count; j++) {
                kids.Add(new GameAction(actions[j], ActionType.Move, turnPac.Type));
            }
            if (turnPac.AbilityCooldown == 0) {
                    kids.Add(new GameAction(turnPac.Location, ActionType.Speed, turnPac.Type));
                    kids.Add(new GameAction(turnPac.Location, ActionType.Switch, SwitchPac.SwitchOptions(turnPac.Type, "PREY")));
                    kids.Add(new GameAction(turnPac.Location, ActionType.Switch, SwitchPac.SwitchOptions(turnPac.Type, "PREDATOR")));
            }

            return kids;

        }
        
        public Pacman ClosestEnemyPac(Pacman player){
            List<double> Distances = new List<double>();
            Pacman closestPac = Enemies[0];
            foreach(Pacman ene in Enemies){
                Distances.Add(ene.Location.Manhattan(player.Location));
                if (ene.Location.Manhattan(player.Location) < closestPac.Location.Manhattan(player.Location)) {
                    closestPac = ene;
                }
            }
            return closestPac;
        }

        public Pacman ClosestFriendlyPac(Pacman player) {
            List<double> Distances = new List<double>();
            Pacman closestPac = MyPacs[0];
            foreach(Pacman pac in MyPacs){
                Distances.Add(pac.Location.Manhattan(player.Location));
                if (pac.Location.Manhattan(player.Location) < closestPac.Location.Manhattan(player.Location)) {
                    closestPac = pac;
                }
            }
            return closestPac;
        }


        public double DistanceFromClosestPac(Pacman closest, Pacman player){
            double min = closest.Location.Manhattan(player.Location);
            return min;
        }
            
        
        public double EstimateUtility() {
            Console.Error.WriteLine();
            double est = PlayerScore - OpponentScore;
            Console.Error.WriteLine($"Points from scoredif: {est}");

            var flood = FloodFill();
            Console.Error.WriteLine($"Points from flood fill: {flood}");
            est += flood;

            est += _combatScoreThisTurn;
            Console.Error.WriteLine($"Points from combat: {_combatScoreThisTurn}");
            
            // fuck it just give a direct bonus for speed since flood fill doesnt seem to be picking it up
            int speedCount = 0;

            foreach (Pacman pac in _allPlayers) {
                var speedBonus = pac.SpeedTurnsLeft > 0 ? 1 : 0;
                speedCount += speedBonus * (pac.IsOurPlayer ? 1 : -1);
            }

            est += speedCount;

            Console.Error.WriteLine($"Points fom speed: {speedCount}");
            
            // TODO if enemy pac in sight is of the "weaker" type to our pac, ++
            // TODO if enemy pac in sight is same type, ==
            // TODO if enemy pac in sight is stronger type, -- (run away!) possibly implement here if it's close enough to change?
			// Get current player, then get enemy pacs in sight (maybe just if there's one close enough?), then compare
            foreach (Pacman pac in MyPacs) {
                Pacman closest = ClosestEnemyPac(pac);
                double dist = DistanceFromClosestPac(closest, pac);
                int combatResult = pac.CompareTo(closest);
                if (combatResult > 0) {
                    est += 10; // arbitrary num
                }
                else if (combatResult < 0) {
                    est -= 10;
                }
            }

            // TODO if a friendly pac is in sight, -- (we want pacs to be further away) (this may be duplicated by flood fill)
			// Same hat as above but with friendly pacs and no comparison

            foreach (Pacman pac in MyPacs) {

            }
            
            
            
            return est;
        }
        
        // how many more points our pacs can access vs enemy pacs
        private double FloodFill() {

            Queue<Point>[] cellsToUpdate = new Queue<Point>[_turnOrder.Count];
            HashSet<Point>[] cellsToUpdateSet = new HashSet<Point>[_turnOrder.Count]; // used to check for duplicates in queue
            bool[,] cellsUpdated = new bool[Width, Height];

            bool stillCellsToUpdate = true;
            
            Pacman[] turnOrderCopy = _turnOrder.ToArray();
            
            double netPoints = 0;
            
            for (int i = 0; i < _turnOrder.Count; i++) {
                cellsToUpdate[i] = new Queue<Point>();
                cellsToUpdateSet[i] = new HashSet<Point>();
                Point start = turnOrderCopy[i].Location;
                cellsToUpdate[i].Enqueue(start);
                cellsToUpdateSet[i].Add(start);
            }

            int time = 1;
    
            // while there are still adjacent cells to update
            while (stillCellsToUpdate) {

                stillCellsToUpdate = false;
                
                // iterate over players
                for (int i = 0; i < turnOrderCopy.Length; i++) {
                    
                    // just because SOMEBODY still has cells to update doesnt mean it's this guy
                    if (cellsToUpdateSet[i].Count > 0) {
                        stillCellsToUpdate |= FloodFillHelper(turnOrderCopy, cellsToUpdate, cellsToUpdateSet, cellsUpdated,
                                                              i, ref netPoints, ref time);
                    }
                    
                    // fast ones get to go again, because they move faster
                    if (turnOrderCopy[i].SpeedTurnsLeft > 0 && cellsToUpdateSet[i].Count > 0) {
                        stillCellsToUpdate |= FloodFillHelper(turnOrderCopy, cellsToUpdate, cellsToUpdateSet, cellsUpdated,
                                                              i, ref netPoints, ref time);
                        // this WILL change the speed turns remaining of the original pac, but...
                        // this is a leaf state anyway so it shouldnt matter
                        // this is the last thing this state will be doing.
                        turnOrderCopy[i].DecreaseSpeed();
                    }
                }

            }
            
            return netPoints;
        }
        
        // runs a single pass of the flood fill algorithm
        private bool FloodFillHelper(Pacman[] turnOrderCopy, Queue<Point>[] cellsToUpdate, HashSet<Point>[] cellsToUpdateSet, 
                                     bool[,] cellsUpdated, int i, ref double netPoints, ref int time) {
            // remove the next point
            Point p = cellsToUpdate[i].Dequeue();
            cellsToUpdateSet[i].Remove(p);
            
            // if this cell was already claimed, skip it
            if (cellsUpdated[p.x, p.y]) return cellsToUpdateSet[i].Count > 0;

            cellsUpdated[p.x, p.y] = true;
        
            // add relevant neighbors to queue
            foreach (Direction d in Direction.Directions) {
                Point neighbor = d.ApplyToPoint(p).Wrap(Width, Height);
                            
                if (!IsWall(neighbor) && !cellsUpdated[neighbor.x, neighbor.y]) {

                    bool isPlannedToUpdate = false;
                    
                    // ignore elements that somebody else already got to first
                    for (int j = 0; j < turnOrderCopy.Length; j++) {
                        isPlannedToUpdate |= cellsToUpdateSet[j].Contains(neighbor);
                    }

                    if (!isPlannedToUpdate) {
                        cellsToUpdate[i].Enqueue(neighbor);
                        cellsToUpdateSet[i].Add(neighbor);
                    }
                }
            }

            netPoints += GetScore(p) * (turnOrderCopy[i].IsOurPlayer ? 1 : -1) / (double) time;
            time++;

            return cellsToUpdateSet[i].Count > 0;
        }

        public bool IsWall(Point p) {
            return _walls[p.x, p.y];
        }

        public int GetScore(Point p) {
            return _scores[p.x, p.y];
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

        public string BoardToString() {
            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    if (_walls[x, y]) {
                        sb.Append("##");
                    }
                    else if (_scores[x, y] == 1) {
                        sb.Append("o ");
                    }
                    else if (_scores[x, y] == 10) {
                        sb.Append("O ");
                    }
                    else if (_scores[x, y] == 0) {
                        sb.Append("  ");
                    }
                    else {
                        sb.Append(_scores[x, y]);
                    }
                }

                sb.Append("\n");
            }

            return sb.ToString();

        }

    }
}