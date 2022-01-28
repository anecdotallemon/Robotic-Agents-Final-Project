using System;
using System.Collections.Generic;

/// <summary>
/// "Pseudo-enum" representing cardinal directions in 2D space
/// </summary>
class Direction {
    #region Static

    /// <summary>
    /// Constant field representing the direction up.
    /// </summary>
    public static readonly Direction Up = new Direction(new Point(0, -1));

    /// <summary>
    /// Constant field representing the direction right.
    /// </summary>
    public static readonly Direction Right = new Direction(new Point(1, 0));

    /// <summary>
    /// Constant field representing the direction down.
    /// </summary>
    public static readonly Direction Down = new Direction(new Point(0, 1));

    /// <summary>
    /// Constant field representing the direction left.
    /// </summary>
    public static readonly Direction Left = new Direction(new Point(-1, 0));

    /// <summary>
    /// A list of all cardinal <c>Direction</c>s, for iterating over
    /// </summary>
    public static readonly Direction[] Directions = {Up, Right, Down, Left};

    /// <summary>
    /// A dictionary organized by the <c>Point</c> offset of each direction, for converting offsets to <c>Direction</c>s.
    /// </summary>
    public static readonly Dictionary<Point, Direction> FromOffset = new Dictionary<Point, Direction>() {
        {Up.Offset, Up},
        {Right.Offset, Right},
        {Down.Offset, Down},
        {Left.Offset, Left}
    };

    /// <summary>
    /// Given two points, return the <c>Direction<c> that moves between them.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns>A <c>Direction</c> representing the vector between the given <c>Point</c>s.</returns>
    /// <exception cref="ArgumentException">If the two points are not exactly one unit apart</exception>
    public static Direction FromPoints(Point start, Point end) {
        try {
            return FromOffset[end - start];
        }
        catch (ArgumentException) {
            throw new ArgumentException("Points must be exactly one unit apart! Points were: " + start + " -> " +
                                        end);
        }
    }

    #endregion

    #region NonStatic

    /// <summary>
    /// A <c>Point</c> representing one space of movement in the given direction.
    /// </summary>
    public readonly Point Offset;

    private Direction(Point p) {
        Offset = p;
    }

    /// <summary>
    /// Apply the <c>Direction</c> to the given <c>Point</c>, returning a copy moved in this direction one square.
    /// </summary>
    /// <param name="p">The <c>Point</c> to move</param>
    /// <returns>A copy of the given <c>Point</c>, moved one square in this <c>Direction</c></returns>
    public Point ApplyToPoint(Point p) {
        return Offset + p;
    }

    #endregion
}

class Game
{
    static void Main(string[] args) {
        State state = new State(Parser.GetMapLayout());
            
        // game loop
        while (true) {
            var input = Parser.ParseInput();
            state.InitializeForTurn(input.myScore, input.opponentScore, input.pacmans, input.pellets);
                
            List<Pacman> playerPacs = state.MyPacs;
            List<GameAction> actions = new List<GameAction>();
                
            foreach (Pacman p in playerPacs) {
                var action = state.GetBestAction();
                state.MakeMove(action);
                actions.Add(action);
            }
                
            Parser.OutputMoves(playerPacs.ToArray(), actions.ToArray());
                
        }
    }
}

public class GameAction {
    public Point TargetPoint;
    public ActionType ActionType;
    public PacType PacSwitch;
    public GameAction(Point targetPoint, ActionType actionType, PacType pacSwitch){
        
        TargetPoint = targetPoint;
        ActionType = actionType;
        PacSwitch = pacSwitch;
    }

    public override string ToString() {
        return $"{ActionType} {TargetPoint} {PacSwitch}";
    }
}

public class ActionType {
    public static readonly ActionType Move = new ActionType("MOVE");
    public static readonly ActionType Speed = new ActionType("SPEED");
    public static readonly ActionType Switch = new ActionType("SWITCH");

    public static readonly Dictionary<string, ActionType> FromString = new Dictionary<string, ActionType>() {
        {Move.ToString(), Move},
        {Speed.ToString(), Speed},
        {Switch.ToString(), Switch}
    };

    private readonly string _string;

    private ActionType(string s) {
        _string = s;
    }

    public override string ToString() {
        return _string;
    }

    public override bool Equals(Object obj) {
        if (obj == null || obj.GetType() != typeof(ActionType)) return false;

        ActionType p = (ActionType) obj;

        return (p.ToString() == ToString());
    }
}

public class Pacman {

        public Point Location;
        public bool IsOurPlayer = false;

        // Pac types include rock, paper, scissors
        public PacType Type;
        public readonly int PacId;

        public static readonly int StartCoolDown = 10;
        public static readonly int StartSpeedTurn = 5;
        
        
        public int SpeedTurnsLeft { get; private set; } = 0;
        public int AbilityCooldown = 0; // unused in wood league

        public bool Alive { get; private set; } = true;


        public Pacman(int x, int y, int pacId) {
            Location = new Point(x, y);
            PacId = pacId;
        }

        public Pacman(Point p, int pacId) {
            Location = p;
            PacId = pacId;
        }

        public Pacman(int x, int y, int pacId, bool isOurs, string typeId, int speedLeft, int cooldown) {
            Location = new Point(x, y);
            PacId = pacId;
            IsOurPlayer = isOurs;
            Type = PacType.FromString[typeId];
            SpeedTurnsLeft = speedLeft;
            AbilityCooldown = cooldown;
        }
        
        #region gamelogic
        public void Move(GameAction move) {
            if (move.ActionType == ActionType.Move){
                Location = move.TargetPoint;
                // decreases ability cool down after each move 
                if(AbilityCooldown !=0 ){
                    AbilityCooldown = AbilityCooldown  -1;
                }
                if(SpeedTurnsLeft != 0){
                    SpeedTurnsLeft = SpeedTurnsLeft -1; 
                }
            }
            else if (move.ActionType == ActionType.Speed){
                // startCoolDown is 10 for now
                AbilityCooldown = Pacman.StartCoolDown ;
                SpeedTurnsLeft = Pacman.StartSpeedTurn;
            }
            else if (move.ActionType == ActionType.Switch){
                Type = move.PacSwitch;
                AbilityCooldown = Pacman.StartCoolDown ;
            }
        }
        
        // returns -1 if loses, 0 if tie, 1 if wins
        // does NOT call Kill() -- that should only be called by parent state!
        public int Combat(Pacman other) {
            return CompareTo(other);
        }
        
        // should ONLY be called by parent state, as parent state will need to adjust its own internals
        public void Kill() {
            Alive = false;
        }

        public void DecreaseSpeed() {
            SpeedTurnsLeft--;
            if (SpeedTurnsLeft < 0) SpeedTurnsLeft = 0;
        }
        
        #endregion
        
        #region bookkeeping
        public override String ToString() {
            if (IsOurPlayer) {
                return "Player: " + Location.ToString() + $" {PacId:X}";
            }
            else {
                return "Enemy: " + Location.ToString() + $" {PacId:X}";
            }
        }
    
        // note: purely value-wise! NOT always appropriate!!!
        // copies of players should be value-equal but NOT reference-equal
        public override bool Equals(Object obj) {
            if (obj == null || obj.GetType() != typeof(Pacman)) return false;

            Pacman p = (Pacman) obj;
            
            return (p.Location.Equals(Location) && p.IsOurPlayer == IsOurPlayer && PacId == p.PacId);
        }

        public Pacman Clone() {
            Pacman p = new Pacman(Location, PacId);
            p.IsOurPlayer = IsOurPlayer;
            p.Alive = Alive;
            p.Type = Type;
            p.SpeedTurnsLeft = SpeedTurnsLeft;
            p.AbilityCooldown = AbilityCooldown;

            return p;
        }
        #endregion


		public int CompareTo(Pacman pac) {
			if (pac.Type == Type) {
				return 0;
			}
            
            
			else if (pac.Type == PacType.Rock) {
				if (Type == PacType.Paper) {
					return 1;
				}
				else {
					return -1;
				}
			}
			else if (pac.Type == PacType.Paper) {
				if (Type == PacType.Scissors) {
					return 1;
				}
				else {
					return -1;
				}
			}
			else {
				if (Type == PacType.Rock) {
					return 1;
				}
				else {
					return -1;
				}
			}
		}


    }

public class PacType {
    public static readonly PacType Rock = new PacType("ROCK", 0);
    public static readonly PacType Scissors = new PacType("SCISSORS", 2);
    public static readonly PacType Paper = new PacType("PAPER", 1);
    public static readonly PacType Dead = new PacType("DEAD", -1000);

    public static readonly Dictionary<String, PacType> FromString = new Dictionary<string, PacType>() {
        {Rock.ToString(), Rock},
        {Scissors.ToString(), Scissors},
        {Paper.ToString(), Paper},
        {Dead.ToString(), Dead}
    };

    public static readonly Dictionary<int, PacType> NumToPacType = new Dictionary<int, PacType>() {
        {0, Rock},
        {1, Paper},
        {2, Scissors}
    };

    private readonly string _string;
    private readonly int _int;

    private PacType(String s, int n) {
        _string = s;
        _int = n;
    }

    public int GetIntCode() {
        return _int;
    }

    public override string ToString() {
        return _string;
    }
}

public static class SwitchPac {
    // pick the pacType that is prey or praditor to the given pac
    public static readonly Dictionary<String, int> FromString = new Dictionary<String, int>() {
        {"PREDATOR", 1}, // represents the pacType that eats the given type we bas in SwitchOptions
        {"PREY", -1}, // represents the pacType that gets eaton the given type we bas in SwitchOptions
    };

    public static PacType SwitchOptions(PacType currType, string foodChain) {
        if (currType == PacType.Dead) throw new ArgumentException("Dead pacman given to switchoptions");

        // we get our offste of either 1 or -1 on if we want the predator type or prey type to the fiven pacType
        int offset = FromString[foodChain];
        // switched pacType to be based around mod math, the prey type is always 1 below and the preditor is 1 above
        // semi flexiable if we add more pacTypes
        
        // subtract one because one pactype is for dead ones, which we dont want to include
        int newPacNum = (currType.GetIntCode() + offset + (PacType.FromString.Count - 1)) % (PacType.FromString.Count - 1);
        // return a PacType
        return PacType.NumToPacType[newPacNum];
    }
}

static class Params {
    public static bool DebugMode = false;

    public static int KillReward = 100;
}

public static class Parser {

        /// <summary>
        /// Get the map layout -- only valid on the very first read of the very first turn
        /// </summary>
        /// <returns>Array of walls</returns>
        public static bool[,] GetMapLayout() {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]); // size of the grid
            int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)

            bool[,] walls = new bool[width, height];

            for (int y = 0; y < height; y++) {
                string row = Console.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
                for (int x = 0; x < width; x++) {
                    walls[x, y] = row[x] == '#';
                }
            }

            return walls;

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns>tuple of: our score, opponent score, visible pacmen, visible pellets</returns>
        public static (int myScore, int opponentScore, Pacman[] pacmans, Pellet[] pellets) ParseInput() {

            string[] inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(inputs[0]);
            int opponentScore = int.Parse(inputs[1]);
            int visiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight

            List<Pacman> pacs = new List<Pacman>();
            
            for (int i = 0; i < visiblePacCount; i++) {
                inputs = Console.ReadLine().Split(' ');
                int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                bool mine = inputs[1] != "0"; // true if this pac is yours
                int x = int.Parse(inputs[2]); // position in the grid
                int y = int.Parse(inputs[3]); // position in the grid
                string typeId = inputs[4]; // unused in wood leagues
                int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
                int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues
                
                pacs.Add(new Pacman(x, y, pacId, mine, typeId, speedTurnsLeft, abilityCooldown));
            }

            List<Pellet> pellets = new List<Pellet>();

            int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
            for (int i = 0; i < visiblePelletCount; i++) {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int value = int.Parse(inputs[2]); // amount of points this pellet is worth
                
                pellets.Add(new Pellet(new Point(x, y), value));
            }

            return (myScore, opponentScore, pacs.ToArray(), pellets.ToArray());
        }

        private static string SingleMoveToString(Pacman pac, GameAction action) {
            // valid outputs:
            // MOVE pacId x y
            // SPEED pacId
            // SWITCH pacId pactype

            if (action.ActionType == ActionType.Move) {
                return $"{ActionType.Move} {pac.PacId} {action.TargetPoint.x} {action.TargetPoint.y}";
            }
            
            if (action.ActionType == ActionType.Speed) {
                return $"{ActionType.Speed} {pac.PacId}";
            }
            
            return $"{ActionType.Switch} {pac.PacId} {action.PacSwitch}";
            
        }

        public static void OutputMoves(Pacman[] pacs, GameAction[] actions) {

            if (pacs.Length != actions.Length) throw new ArgumentException();

            for (int i = 0; i < pacs.Length; i++) {
                Console.Write(SingleMoveToString(pacs[i], actions[i]));
                Console.Write("|");
            }

            Console.WriteLine();
        }
    }


// simple pellet class
// not actually used in game logic, just for transferring information from parser -> state
public class Pellet {
    public Point Location;
    public int Score;

    public Pellet(Point location, int score) {
        Location = location;
        Score = score;
    }
}

public readonly struct Point {
    public readonly int x;
    public readonly int y;

    /// <summary>
    /// Construct a new <c>Point</c> with the given values.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public Point(int x, int y) {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// Construct a copy of the given <c>Point</c>.
    /// </summary>
    /// <param name="p">The <c>Point</c> to copy</param>
    public Point(Point p) {
        this.x = p.x;
        this.y = p.y;
    }

    public static Point operator +(Point a) => a;
    public static Point operator -(Point b) => new Point(-b.x, -b.y);
    public static Point operator +(Point a, Point b) => new Point(a.x + b.x, a.y + b.y);
    public static Point operator -(Point a, Point b) => a + (-b);

    public static bool operator ==(Point a, Point b) => a.Equals(b);

    public static bool operator !=(Point a, Point b) => !a.Equals(b);

    /// <summary>
    /// Get the square magnitude of the current <c>Point</c>, that is, the square of its distance from the origin.
    /// </summary>
    /// <returns>The square magnitude as an integer</returns>
    public int Mag2() {
        return x * x + y * y;
    }

    public Point Wrap(int width, int height) {
        var x1 = x;
        while (x1 < 0) x1 += width;
        var y1 = y;
        while (y1 < 0) y1 += height;

        x1 %= width;
        y1 %= height;

        return new Point(x1, y1);
    }

    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }

    /// <summary>
    /// Whether this <c>Point</c> is within the boundaries of the game.
    /// </summary>
    /// <returns>Returns <code>true</code> if and only if the <c>Point</c> is within the game bounds, <code>false</code> otherwise.</returns>
    public bool IsOutOfBounds(int width, int height) {
        return x >= width || x < 0 || y >= height || y < 0;
    }

    /// <summary>
    /// Whether this <c>Point</c> is considered equal to a given object.
    /// </summary>
    /// <param name="obj">The object to compare against</param>
    /// <returns><code>true</code> if and only if the other object is a <c>Point</c> with identical values, <code>false</code> otherwise.</returns>
    public override bool Equals(Object obj) {
        if (obj == null || obj.GetType() != typeof(Point)) return false;
        Point p = (Point) obj;
        return (p.x == x && p.y == y);
    }

    /// <summary>
    /// Returns a semi-unique identifier for the given <c>Point</c>.
    /// </summary>
    /// <returns>An integer representing the <c>Point</c></returns>
    public override int GetHashCode() {
        // this implementation assumes small values and so is technically not suitable for general purposes
        // but it should work just fine for the entirety of the robotics class
        return x << 16 + y;
    }
}

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

            return stateCopy;
        }

        public void InitializeForTurn(int myScore, int opponentScore, Pacman[] pacs, Pellet[] pellets) { // to capture the new information

            _combatScoreThisTurn = 0; // probably not necessary as this is usually only read and set in fictional states, but good practice just in case
            
            PlayerScore = myScore;
            OpponentScore = opponentScore;

			MyPacs.Clear();
			Enemies.Clear();
			_turnOrder.Clear();
            
            
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
        
        
        public double EstimateUtility() {
            double est = PlayerScore - OpponentScore;
			est += FloodFill();

            est += _combatScoreThisTurn;
            
            // fuck it just give a direct bonus for speed since flood fill doesnt seem to be picking it up
            int speedCount = 0;

            foreach (Pacman pac in _allPlayers) {
                speedCount += pac.SpeedTurnsLeft > 0 ? (pac.IsOurPlayer ? 1 : -1) : 0;
            }

            est += speedCount;
            
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

    }
