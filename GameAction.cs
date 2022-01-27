using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    // class to represent actions -- cant just use Direction because this game lets you speed, change color, etc
    public class GameAction {
        public Point gameActions;
        public GameAction(Point gameAction){
        
            this.gameActions = gameAction;
        }
    }
}