using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuzzleGame
{
    public partial class MainWindow : Form
    {
        public GameController Controller { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.openFileDialog.InitialDirectory = Application.StartupPath + "\\maps";
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            this.openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                var map = new TiledSharp.TmxMap(openFileDialog.FileName);
                Controller = new GameController(this, map);
                mapView1.Refresh();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Controller == null) return;

            if (e.KeyCode == Keys.Up)
            {
                Controller.MoveCommand(Direction.Up);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                Controller.MoveCommand(Direction.Down);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                Controller.MoveCommand(Direction.Left);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Right)
            {
                Controller.MoveCommand(Direction.Right);
                e.SuppressKeyPress = true;
            }
        }
    }
}
