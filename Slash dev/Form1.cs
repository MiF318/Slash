using System.Drawing;

namespace Slash
{
    public partial class Form1 : Form
    {
        Game Game;

        Graphics Graphics;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();

            if (e.KeyCode == Keys.D)
                Game.UserActions.Add(AllActions.Right);

            if (e.KeyCode == Keys.A)
                Game.UserActions.Add(AllActions.Left);

            if (e.KeyCode == Keys.Space)
                Game.UserActions.Add(AllActions.Jump);

            if (e.KeyCode == Keys.S)
                Game.UserActions.Add(AllActions.Slash);

            if (e.KeyCode == Keys.P)
            {
                if (Game.Status == Game.Statuses.Play)
                    Game.Status = Game.Statuses.Pause;
                else if (Game.Status == Game.Statuses.Pause)
                    Game.Status = Game.Statuses.Play;
            }
                

            if (e.KeyCode == Keys.W)
                Game.EndLevel = true;

            if (e.KeyCode == Keys.N)
                Game.NextLevel = true;
            if (e.KeyCode == Keys.B)
                Game.PrevLevel = true;
        }
        
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
                Game.UserActions.Add(AllActions.RightOff);

            if (e.KeyCode == Keys.A)
                Game.UserActions.Add(AllActions.LeftOff);

            if (e.KeyCode == Keys.Space)
                Game.UserActions.Add(AllActions.JumpOff);
        }

        protected override void OnFormClosing(FormClosingEventArgs eventArgs)
        {
            var result = MessageBox.Show("Действительно закрыть?", "",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                eventArgs.Cancel = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            Game = new Game(Graphics, Invalidate);
            Game.Start(timer1);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            Game.OnPaintUpdate(graphics);
        }
    }
}
