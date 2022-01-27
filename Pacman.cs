﻿using System;
using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    public class Pacman {

        public Point Location;
        public bool IsOurPlayer = false;

        // Pac types include rock, paper, scissors
        public PacType Type;
        public int PacId;

        private int _speedTurnsLeft = 0; // unused in wood league
        private int _abilityCooldown = 0; // unused in wood league

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
            _speedTurnsLeft = speedLeft;
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



    }

    public class PacType {

        public static PacType Rock = new PacType("ROCK");
        public static PacType Scissors = new PacType("SCISSORS");
        public static PacType Paper = new PacType("PAPER");

        public static readonly Dictionary<String, PacType> FromString = new Dictionary<string, PacType>() {
            {Rock.ToString(), Rock},
            {Scissors.ToString(), Scissors},
            {Paper.ToString(), Paper}
        };

        private readonly string _string;

        private PacType(String s) {
            _string = s;
        }

        public override string ToString() {
            return _string;
        }
    }
}