﻿using System;
using System.Collections.Generic;
using Robotic_Agents_Final_Project;

namespace Robotic_Agents_Final_Project
{
    public class State
    {

        // one list for our pacs and one list for enemies, since we may want to keep track of where we ares
        public List <Pacman> MyPacs { get; private set; }
        public List<Pacman> Enemies { get; private set; }
        public Point[,] GameBoard;

        private Queue<Pacman> _turnOrder = new Queue<Pacman>(); // all alive players, sorted by turn order
        private List<Pacman> _allPlayers = new List<Pacman>(); // list of all players, including dead ones, sorted by initial turn order

        private bool[,] _walls; // true at wall, false at open space
        private int[,] _scores; // 0 at blank, 1 at pellet, 10 at super pellet
        
        public readonly int Width;
        public readonly int Height;

        /// <summary>
        /// Constructs a new state with the given wall setup. Copies the wall array to a new spot in memory
        /// </summary>
        /// <param name="walls">array of bools with true at wall locations</param>
        public State(bool[,] walls) {
            Width = walls.GetLength(0);
            Height = walls.GetLength(1);
            
            _walls = new bool[Width, Height];
            _scores = new int[Width, Height];
            
            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    _walls[i, j] = walls[i,j];
                }
            }
            
        }
        
        public State Clone() {

            State stateCopy = new State(_walls);
        
            // clone all players
            foreach (Pacman p in _allPlayers) {

                Pacman pacmanCopy = p.Clone();
            
                // add to turn orders
                stateCopy._allPlayers.Add(pacmanCopy);
                if (p.Alive) {
                    stateCopy._turnOrder.Enqueue(pacmanCopy);
                }
            
                // add to lists
                if (p.IsOurPlayer) {
                    stateCopy.MyPacs.Add(pacmanCopy);
                }
                else {
                    stateCopy.Enemies.Add(pacmanCopy);
                }
            }
        
            // shuffle the copy's turn order queue to match ours
            while (!stateCopy._turnOrder.Peek().Equals(_turnOrder.Peek())) {
                stateCopy._turnOrder.Enqueue(stateCopy._turnOrder.Dequeue());
            }
        
        
            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    stateCopy._walls[i, j] = _walls[i,j];
                }
            }

            return stateCopy;
        }

        public void InitializeForFirstTurn() { // this was necessary for my original tron game -- im not sure if it will be here. idk.
            throw new NotImplementedException();
        }

        public void UpdatePlayers() {
            throw new NotImplementedException();
        }

        public Pacman GetCurrentPlayer() {
            return _turnOrder.Peek();
        }

        public void MakeMove(GameAction move) {
            throw new NotImplementedException();
        }


        public GameAction[] GetMoves(int h, List<Pacman> curplayer) {
             // sends over to recursive helper function
            getMovesHelper(h,  curplayer, "");
            // copies all values getKids which has all the actions a player can make 
            GameAction[] kids = new GameAction[getKids.Count];
            for(int i =0; i < getKids.Count; i++){
                kids[i] = new GameAction(getKids[i]);
            }
            return kids;
           
        }
        public Boolean getMovesHelper(int h,  List<Pacman> curplayer, string actions){
            // when h is 1 we are at the last pac on list
             if(h ==1 ){

                 string temp = "";
                // loops through directions which contains the 4 cartinal directions 
                for (int i =0; i < directions.Length; i++){
                    // action is the other pacmen on the player team have already done
                    temp = actions; 
                    // adds the cardinal direction to current players pac postion, needs to change to handle walls
                    temp = temp + (directions[i] + curplayer[curplayer.Count -1].Location ). ToString();

                    getKids.Add(temp);
                    
                }

                temp = actions;
                // speed move
                temp = temp + "SPEED";
                
                // switch is the 3 types we can go to we cycle through the options
                for (int i =0; i < switches; i++){
                    temp = actions;
                    // since we cant switch to out own type we ignore that case
                    if( curplayer[curplayer.Count -1].GetType.ToString() != switches[i] ){
                        temp = temp + "SWITCH " + switches[i];
                        getKids.Add(temp);
                    }
                }
                // return doent matter it just sends us back to previous pac to do its next action option
                return true;

            }

        string temp = "";
        for (int i =0; i < directions.Length; i++){
            temp = actions; 
            temp = temp + (directions[i] + curplayer[curplayer.Count - h ].Location ). ToString();
            //makes a recurisve call to next pacman to loop through their options  
            return getMovesHelper(h-1,  curplayer, temp);
            
        }
        temp = actions;
        temp = temp + "SPEED";
        //makes a recurisve call to next pacman to loop through their options  
        return getMovesHelper(h-1,  curplayer, temp);
        
        for (int i =0; i < switches; i++){
            temp = actions;
            if( curplayer[curplayer.Count -h].GetType.ToString() != switches[i] ){
                temp = temp + "SWITCH " + switches[i];
                //makes a recurisve call to next pacman to loop through their options  
                return getMovesHelper(h-1,  curplayer, temp);
            }
        }

        }
        public int EstimateUtility() {
            // parallel flood fill measuring available pellets
            throw new NotImplementedException();
        }
        
        // kill current player
        public void KillCurrent() {
            throw new NotImplementedException();
        }

    }
}