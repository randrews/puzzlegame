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
                mapView1.Controller = Controller;
                mapView1.Refresh();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (Controller != null)
            {
                if (keyData == Keys.Up)
                {
                    Controller.MoveCommand(Direction.Up);
                }
                else if (keyData == Keys.Down)
                {
                    Controller.MoveCommand(Direction.Down);
                }
                else if (keyData == Keys.Left)
                {
                    Controller.MoveCommand(Direction.Left);
                }
                else if (keyData == Keys.Right)
                {
                    Controller.MoveCommand(Direction.Right);
                }

                mapView1.Refresh();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
