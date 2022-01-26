﻿using System;

namespace Robotic_Agents_Final_Project {
    public class Player {

        public Point Location;
        public bool IsOurPlayer = false;
        
        // Pac types include rock, paper, scissors
        private PacType _type;
        public int PacId;

        public bool Alive { get; private set; } = true;

        public Player(int x, int y, int pacId) {
            Location = new Point(x, y);
            PacId = pacId;
        }

        public Player(Point p, int pacId) {
            Location = p;
            PacId = pacId;
        }
        
        #region gamelogic
        // returns previous position
        public Point Move(GameAction a) {
            throw new NotImplementedException();
        }
        
        // returns -1 if loses, 0 if tie, 1 if loses
        // does NOT call Kill() -- that should only be called by parent state!
        public bool Combat(Player other) {
            throw new NotImplementedException();
        }
        
        // should ONLY be called by parent state, as parent state will need to adjust its own internals
        public void Kill() {
            Alive = false;
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
            if (obj == null || obj.GetType() != typeof(Player)) return false;

            Player p = (Player) obj;
            
            return (p.Location.Equals(Location) && p.IsOurPlayer == IsOurPlayer && PacId == p.PacId);
        }

        public Player Clone() {
            Player p = new Player(Location, PacId);
            p.IsOurPlayer = IsOurPlayer;
            p.Alive = Alive;

            return p;
        }
        #endregion



    }

    enum PacType {
        Rock,
        Paper,
        Scissors
    }
}