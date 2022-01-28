﻿using System;
using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    // class to represent actions -- cant just use Direction because this game lets you speed, change color, etc
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
    public class ActionType{
        
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
}