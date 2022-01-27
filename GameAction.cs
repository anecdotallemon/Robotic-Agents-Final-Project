using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    // class to represent actions -- cant just use Direction because this game lets you speed, change color, etc
    public class GameAction {
        public List<string> gameActions;
        public GameAction(string gameAction){
            gameActions = new List<string>();
            gameActions.Add(gameAction);
        }
    }
}