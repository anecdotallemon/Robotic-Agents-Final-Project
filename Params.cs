﻿namespace Robotic_Agents_Final_Project {
    static class Params {
        // to do: better way of doing this?
        // encoding in class, maybe?
        // wrapping in gets/sets that only the initial functions can access?
        public static int WIDTH;
        public static int HEIGHT;

        public static bool DebugMode = false;

        public static int KillReward = 100;
    }
}