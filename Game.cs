using System;

namespace Robotic_Agents_Final_Project {
    /**
 * Grab the pellets as fast as you can!
 **/
    class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]); // size of the grid
            int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
            for (int i = 0; i < height; i++)
            {
                string row = Console.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
            }

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                int myScore = int.Parse(inputs[0]);
                int opponentScore = int.Parse(inputs[1]);
                int visiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight
                for (int i = 0; i < visiblePacCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                    bool mine = inputs[1] != "0"; // true if this pac is yours
                    int x = int.Parse(inputs[2]); // position in the grid
                    int y = int.Parse(inputs[3]); // position in the grid
                    string typeId = inputs[4]; // unused in wood leagues
                    int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
                    int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues
                }
                int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
                for (int i = 0; i < visiblePelletCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int x = int.Parse(inputs[0]);
                    int y = int.Parse(inputs[1]);
                    int value = int.Parse(inputs[2]); // amount of points this pellet is worth
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                Console.WriteLine("MOVE 0 10 15"); // MOVE <pacId> <x> <y>
            }
        }
    }
}