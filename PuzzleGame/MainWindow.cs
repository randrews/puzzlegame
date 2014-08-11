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

        private string _currentFilename;
        private string CurrentFilename
        {
            get { return _currentFilename; }
            set
            {
                _currentFilename = value;
                restartButton.Enabled = (value != null);
            }
        }

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
            OpenMap(openFileDialog.FileName);
        }

        public void OpenMap(string filename)
        {
            try
            {
                var map = new TiledSharp.TmxMap(filename);
                Controller = new GameController(this, map);
                mapView1.Controller = Controller;
                mapView1.Refresh();
                CurrentFilename = filename;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                MessageBox.Show(exc.Message, "Error loading map",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (Controller == null) return base.ProcessCmdKey(ref msg, keyData);

            if (mapView1.ShowingMessage)
            {
                mapView1.Message = null;
            }
            else
            {
                switch (keyData)
                {
                    case Keys.Up:
                        Controller.MoveCommand(Direction.Up);
                        break;
                    case Keys.Down:
                        Controller.MoveCommand(Direction.Down);
                        break;
                    case Keys.Left:
                        Controller.MoveCommand(Direction.Left);
                        break;
                    case Keys.Right:
                        Controller.MoveCommand(Direction.Right);
                        break;
                }                
            }

            mapView1.Refresh();
            return true;
        }

        internal void UpdateStatusLabel(string p)
        {
            StatusLabel.Text = p;
        }

        public void ShowMessage(string message)
        {
            mapView1.Message = message;
        }

        private void restartButton_Click(object sender, EventArgs e)
        {
            if (CurrentFilename != null)
            {
                OpenMap(CurrentFilename);
            }
        }
    }
}
