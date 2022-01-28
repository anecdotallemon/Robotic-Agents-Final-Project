using System;
using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    public class Pacman {

        public Point Location;
        public bool IsOurPlayer = false;

        // Pac types include rock, paper, scissors
        public PacType Type;
        public readonly int PacId;

        public static readonly int StartCoolDown = 10;
        
        public int SpeedTurnsLeft { get; set; } = 0;
        public int _abilityCooldown = 0; // unused in wood league

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
            _abilityCooldown = cooldown;
        }
        
        #region gamelogic
        // returns previous position
        public Point Move(GameAction a) {
            throw new NotImplementedException();
        }
        
        // returns -1 if loses, 0 if tie, 1 if wins
        // does NOT call Kill() -- that should only be called by parent state!
        public bool Combat(Pacman other) {
            throw new NotImplementedException();
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

        public static readonly Dictionary<int ,PacType> NumToPacType = new Dictionary<int, PacType>() {
            {0,Rock},
            {1,Paper},
            {2,Scissors}
           
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
public static class SwitchPac{
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
           int newPacNum = (currType.GetIntCode() + offset) % (PacType.FromString.Count);
           // return a PacType
           return PacType.NumToPacType[newPacNum];
       }  
    }
}