﻿using System;
using System.Windows.Forms;

namespace PuzzleGame
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    };

    public enum Color
    {
        Red,
        Green,
        Yellow,
        Blue
    };

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
