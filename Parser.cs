﻿using System;
using System.Collections.Generic;

namespace Robotic_Agents_Final_Project {
    public static class Parser {

        /// <summary>
        /// Get the map layout -- only valid on the very first read of the very first turn
        /// </summary>
        /// <returns>Array of walls</returns>
        public static bool[,] GetMapLayout() {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            int width = int.Parse(inputs[0]); // size of the grid
            int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)

            bool[,] walls = new bool[width, height];

            for (int y = 0; y < height; y++) {
                string row = Console.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
                for (int x = 0; x < width; x++) {
                    walls[x, y] = row[x] == '#';
                }
            }

            return walls;

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns>tuple of: our score, opponent score, visible pacmen, visible pellets</returns>
        public static (int myScore, int opponentScore, Pacman[] pacmans, Pellet[] pellets) ParseInput() {

            string[] inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(inputs[0]);
            int opponentScore = int.Parse(inputs[1]);
            int visiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight

            List<Pacman> pacs = new List<Pacman>();
            
            for (int i = 0; i < visiblePacCount; i++) {
                inputs = Console.ReadLine().Split(' ');
                int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                bool mine = inputs[1] != "0"; // true if this pac is yours
                int x = int.Parse(inputs[2]); // position in the grid
                int y = int.Parse(inputs[3]); // position in the grid
                string typeId = inputs[4]; // unused in wood leagues
                int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
                int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues
                
                pacs.Add(new Pacman(x, y, pacId, mine, typeId, speedTurnsLeft, abilityCooldown));
            }

            List<Pellet> pellets = new List<Pellet>();

            int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
            for (int i = 0; i < visiblePelletCount; i++) {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int value = int.Parse(inputs[2]); // amount of points this pellet is worth
                
                pellets.Add(new Pellet(new Point(x, y), value));
            }

            return (myScore, opponentScore, pacs.ToArray(), pellets.ToArray());
        }

        private static string SingleMoveToString(Pacman pac, GameAction action) {
            // valid outputs:
            // MOVE pacId x y
            // SPEED pacId
            // SWITCH pacId pactype

            if (action.ActionType == ActionType.Move) {
                return $"{ActionType.Move} {pac.PacId} {pac.Location.x} {pac.Location.y}";
            }
            
            if (action.ActionType == ActionType.Speed) {
                return $"{ActionType.Speed} {pac.PacId}";
            }
            
            return $"{ActionType.Switch} {pac.PacId} {action.PacSwitch}";
            
        }

        public static void OutputMoves(Pacman[] pacs, GameAction[] actions) {

            if (pacs.Length != actions.Length) throw new ArgumentException();

            for (int i = 0; i < pacs.Length; i++) {
                Console.Write(SingleMoveToString(pacs[i], actions[i]));
                Console.Write("|");
            }

            Console.WriteLine();
        }
    }
}