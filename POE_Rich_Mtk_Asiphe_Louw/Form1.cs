using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml;

namespace POE_Rich_Mtk_Asiphe_Louw
{
    public partial class Form1 : Form
    {
        private GameEngine gameEngine;

        public Form1()
        {
            InitializeComponent();
            gameEngine = new GameEngine(10);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            lblDisplay.Text = gameEngine.ToString();
            lblHeroStats.Text = "Hero HP: " + gameEngine.HeroStats;
        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            try
            {
                gameEngine.SaveGame();
                MessageBox.Show(this, "Game saved.", "Save Game", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception error) when (IsSaveFileError(error))
            {
                MessageBox.Show(this, "Could not save the game: " + error.Message, "Save Game",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnLoadGame_Click(object sender, EventArgs e)
        {
            try
            {
                gameEngine.LoadGame();
                UpdateDisplay();
            }
            catch (Exception error) when (IsSaveFileError(error))
            {
                MessageBox.Show(this, "Could not load the game. Your current game is unchanged.\n" +
                                error.Message, "Load Game", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static bool IsSaveFileError(Exception error)
        {
            return error is IOException || error is UnauthorizedAccessException ||
                   error is SerializationException || error is XmlException ||
                   error is ArgumentException;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Buttons normally consume arrow keys for focus navigation.
            // Keep attacks working after clicking Save Game or Load Game.
            if (keyData == Keys.Up || keyData == Keys.Right ||
                keyData == Keys.Down || keyData == Keys.Left)
            {
                Form1_KeyDown(this, new KeyEventArgs(keyData));
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Direction movementDirection = Direction.None;
            Direction attackDirection = Direction.None;

            switch (e.KeyCode)
            {
                case Keys.W:
                    movementDirection = Direction.Up;
                    break;
                case Keys.D:
                    movementDirection = Direction.Right;
                    break;
                case Keys.S:
                    movementDirection = Direction.Down;
                    break;
                case Keys.A:
                    movementDirection = Direction.Left;
                    break;
                case Keys.Up:
                    attackDirection = Direction.Up;
                    break;
                case Keys.Right:
                    attackDirection = Direction.Right;
                    break;
                case Keys.Down:
                    attackDirection = Direction.Down;
                    break;
                case Keys.Left:
                    attackDirection = Direction.Left;
                    break;
            }

            if (movementDirection != Direction.None)
            {
                gameEngine.TriggerMovement(movementDirection);
                UpdateDisplay();
                e.Handled = true;
            }
            else if (attackDirection != Direction.None)
            {
                gameEngine.TriggerAttack(attackDirection);
                UpdateDisplay();
                e.Handled = true;
            }
        }
    }
}
