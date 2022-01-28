using System;
using System.Collections.Generic;
using System.Text;

namespace Robotic_Agents_Final_Project {
class MiniMaxNode {

    private bool _maxing; // true if maximizing, false if minimizing
    private State _state;
    private int _height; // zero if leaf node, Params.FutureDepth if root

    private Dictionary<GameAction,MiniMaxNode> _children;
    private bool _utilityIsSet = false;
    private double _utility;

    private string _debugString;
    
    // construct a tree node with the given state. no moves are taken, but a copy of the state is made
    // for use in constructing roots
    public MiniMaxNode(State s, int height) {
        _state = s.Clone();
        _height = height;
        _maxing = _state.GetCurrentPlayer().IsOurPlayer;

        if (Params.DebugMode) {
            _debugString = "";
            Console.Error.WriteLine(_debugString);
        }

    }
    
    // construct a tree node with the given state, then immediately make the given move
    // for use in constructing leaves and branches
    public MiniMaxNode(State s, int height, GameAction move, string debugString="") {
        _state = s.Clone();
        _height = height;

        if (Params.DebugMode) {
            _debugString = debugString + move.ToString();
            Console.Error.WriteLine("Generating: " + _debugString);
        }

        _state.MakeMove(move);
        _maxing = _state.GetCurrentPlayer().IsOurPlayer;
        
    }
    
    public Dictionary<GameAction, MiniMaxNode> GetChildren() {

        if (_height == 0) {
            return null;
        }

        if (_children != null) {
            return _children;
        }
        
        MakeChildren();
        return _children;
    }
    
    // separated into separate function purely for profiling purposes
    private void MakeChildren() {
        // swictehd to List to handel when special moves cant be done 
        List<GameAction> availableMoves = _state.GetMoves();
      
       
        _children = new Dictionary<GameAction, MiniMaxNode>();

        foreach (var d in availableMoves) {
            _children[d] = new MiniMaxNode(_state, _height - 1, d, _debugString);
        }
    }
    
    // semi-optimized to work with more than two players -- it SEEMS to be doing okay? no real changes made to the
    // fundamental algorithm itself
    double AlphaBetaUtility(double parentMax, double parentMin) {

        if (_utilityIsSet) return _utility;
        
        if (_height == 0) {
            if (Params.DebugMode) Console.Error.WriteLine("Estimating utility: " + _debugString);
            _utility = _state.EstimateUtility();
            _utilityIsSet = true;
            return _utility;
        }

        double util;
        
        if (_maxing) {
            util = double.NegativeInfinity;
            var curMax = parentMax;

            foreach (KeyValuePair<GameAction, MiniMaxNode> kv in GetChildren()) {
                util = Math.Max(util, kv.Value.AlphaBetaUtility(curMax, parentMin));
                curMax = Math.Max(curMax, util);
                
                if (util >= parentMin) break; // if our parent is a minimizing node, it won't like large values, so skip them
            }
        }
        else {
            util = double.PositiveInfinity;
            var curMin = parentMin;

            foreach (KeyValuePair<GameAction, MiniMaxNode> kv in GetChildren()) {
                util = Math.Min(util, kv.Value.AlphaBetaUtility(parentMax, curMin));
                curMin = Math.Min(curMin, util);
                
                if (util <= parentMax) break; // if our parent is a maximizing node, it wont like small values, so skip them
            }
            
            // if the current player is an enemy with no moves but NOT the last enemy, it shouldnt provide infinity score
            if (GetChildren().Count == 0 && _state.Enemies.Count > 1) {
                util = Params.KillReward; // TODO replace with dynamic estimator?
                curMin = Math.Min(curMin, util);
                _state.KillCurrent();
            } 
        }

        _utility = util;
        _utilityIsSet = true;
        return _utility;
    }

    double GetUtility() {
        if (_utilityIsSet) return _utility;
        throw new InvalidOperationException("Must set utility before retrieval!");
    }

    public GameAction GetBestMove() {
        var children = GetChildren();

        GameAction bestMove = null;
        AlphaBetaUtility(double.NegativeInfinity, double.PositiveInfinity);
        
        foreach (KeyValuePair<GameAction, MiniMaxNode> kv in children) {
            Console.Error.WriteLine(kv.Key + ": " + kv.Value.GetUtility());
            if (bestMove == null || children[kv.Key].GetUtility() > children[bestMove].GetUtility()) {
                bestMove = kv.Key;
                // if we hit a wincon, skip the rest -- they're likely unevaluated anyway
                if (double.IsPositiveInfinity(children[kv.Key].GetUtility())) break;
            }
        }

        if (bestMove == null) {
            Console.Error.WriteLine(this);
            Console.Error.WriteLine(_state);
            throw new InvalidOperationException("No available moves left!");
        }
        return bestMove;
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.Append(GetUtility());

        foreach (KeyValuePair<GameAction, MiniMaxNode> kv in GetChildren()) {
            sb.AppendLine(kv.Key + ": " + kv.Value);
        }

        return sb.ToString();

    }

}

}