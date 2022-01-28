using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    // class to represent actions -- cant just use Direction because this game lets you speed, change color, etc
    public class GameAction {
        public Point gameActions;
        public ActionType actionType;
        public PacType pacSwitch;
        public GameAction(Point gameAction, string actionType, PacType pacSwitch){
        
            this.gameActions = gameAction;
            this.actionType = ActionType.FromString[actionType];
            this.pacSwitch = pacSwitch;
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



    }
}