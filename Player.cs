using System;

namespace Robotic_Agents_Final_Project {
    public class Player {

        public Point Location;
        public readonly bool IsOurPlayer = false;
        
        // Pac types include rock, paper, scissors
        private PacType _type;

        public bool Combat(Player other) {
            throw new NotImplementedException();
        }


    }

    enum PacType {
        Rock,
        Paper,
        Scissors
    }
}