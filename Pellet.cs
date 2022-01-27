namespace Robotic_Agents_Final_Project {
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
}