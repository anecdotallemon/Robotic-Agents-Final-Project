﻿using System;
using System.Collections.Generic;
using Robotic_Agents_Final_Project;

namespace Robotic_Agents_Final_Project
{
    public class State
    {

        // one list for our pacs and one list for enemies, since we may want to keep track of where we ares
        public List < Player > MyPacs { get; private set; }
        public List<Player> Enemies { get; private set; }
        public Point[,] GameBoard;
        
        // Constructor with gameBoard initialization
        public State(Point[,] GameBoard, List < Player > MyPacs,  List<Player> Enemies   ) {
            this.GameBoard = new Point[GmaeBoard.GetLength(0) , GameBoard.GetLength(1)] ;
            for (int i =0; i < GameBoard.GetLength(0), i++){
                for(int j = 0; j < GameBoard.GetLength(1), j++){

                    this.GameBoard[i,j] = GameBoard[i,j];
                }



            }
            for (int i - 0; i< MyPacs.Count, i++ ){
                this.MyPacs.add

            } 
            
        }
        
        public State Clone() {
            throw new NotImplementedException();
        }

        public Player GetCurrentPlayer() {
            throw new NotImplementedException();
        }

        public void MakeMove(GameAction move) {
            throw new NotImplementedException();
        }

        public GameAction[] GetMoves() {
            throw new NotImplementedException();
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