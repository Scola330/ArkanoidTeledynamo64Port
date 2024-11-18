using System;
using System.Drawing;
using System.Windows.Forms;

namespace ArkanoidTeledynamo64Port
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer gameTimer = null!;
        private Paddle paddle = null!;
        private Ball ball = null!;
        private Brick[,] bricks;
        private bool isGameMode = true;
        private bool moveLeft = false;
        private bool moveRight = false;

        public Form1()
        {
            InitializeComponent();
            InitializeMenu();
            bricks = new Brick[5, 10]; // Initialize bricks array with default size
            InitializeGame();
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.KeyUp += new KeyEventHandler(Form1_KeyUp);
            this.Resize += new EventHandler(Form1_Resize);
        }

        private void InitializeMenu()
        {
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem modeMenuItem = new ToolStripMenuItem("Mode");
            ToolStripMenuItem gameMenuItem = new ToolStripMenuItem("Game");
            ToolStripMenuItem editorMenuItem = new ToolStripMenuItem("Level Editor");

            gameMenuItem.Click += (s, e) => SwitchToGameMode();
            editorMenuItem.Click += (s, e) => SwitchToEditorMode();

            modeMenuItem.DropDownItems.Add(gameMenuItem);
            modeMenuItem.DropDownItems.Add(editorMenuItem);
            menuStrip.Items.Add(modeMenuItem);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void SwitchToGameMode()
        {
            isGameMode = true;
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Tick -= GameTimer_Tick;
            }
            InitializeGame();
        }

        private void SwitchToEditorMode()
        {
            isGameMode = false;
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Tick -= GameTimer_Tick;
            }

            // Otwórz LevelEditorForm i przeka¿ aktualne klocki
            using (LevelEditorForm editorForm = new LevelEditorForm(bricks))
            {
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    // Zaktualizuj klocki po zamkniêciu edytora
                    bricks = editorForm.Bricks;
                    Invalidate();
                }
            }
        }

        private void InitializeGame()
        {
            this.DoubleBuffered = true;

            paddle = new Paddle(this.ClientSize.Width / 2, this.ClientSize.Height - 30);
            ball = new Ball(this.ClientSize.Width / 2, this.ClientSize.Height / 2);

            InitializeBricks();

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void InitializeBricks()
        {
            int rows = 5;
            int cols = 10;
            bricks = new Brick[rows, cols];
            int brickWidth = this.ClientSize.Width / cols;
            int brickHeight = this.ClientSize.Height / (rows * 3);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    bricks[i, j] = new Brick(j * brickWidth, i * brickHeight, brickWidth, brickHeight);
                }
            }
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (isGameMode)
            {
                ball.Move();
                ball.CheckCollision(paddle, bricks);

                if (moveLeft && paddle.X > 0)
                {
                    paddle.Move(paddle.X - 10);
                }
                if (moveRight && paddle.X < this.ClientSize.Width - paddle.Width)
                {
                    paddle.Move(paddle.X + 10);
                }

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            if (isGameMode)
            {
                paddle.Draw(g);
                ball.Draw(g);

                foreach (var brick in bricks)
                {
                    brick?.Draw(g);
                }
            }
            else
            {
                // Draw level editor UI here
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!isGameMode)
            {
                // Handle level editor mouse move here
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                moveLeft = true;
            }
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                moveRight = true;
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                moveLeft = false;
            }
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                moveRight = false;
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            InitializeGame();
        }
    }

    public class Paddle
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Paddle(int x, int y)
        {
            Width = 100;
            Height = 20;
            X = x - Width / 2;
            Y = y;
        }

        public void Move(int x)
        {
            X = x;
        }

        public void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, X, Y, Width, Height);
        }
    }

    public class Ball
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Radius { get; private set; }
        private int dx = 5;
        private int dy = 5;

        public Ball(int x, int y)
        {
            Radius = 10;
            X = x;
            Y = y;
        }

        public void Move()
        {
            X += dx;
            Y += dy;

            if (X < 0 || X > 800 - Radius) dx = -dx;
            if (Y < 0 || Y > 600 - Radius) dy = -dy;
        }

        public void CheckCollision(Paddle paddle, Brick[,] bricks)
        {
            // SprawdŸ kolizjê z wios³em
            if (X + Radius > paddle.X && X < paddle.X + paddle.Width && Y + Radius > paddle.Y && Y < paddle.Y + paddle.Height)
            {
                dy = -dy;
            }

            // SprawdŸ kolizjê z ceg³ami
            for (int i = 0; i < bricks.GetLength(0); i++)
            {
                for (int j = 0; j < bricks.GetLength(1); j++)
                {
                    var brick = bricks[i, j];
                    if (brick != null && !brick.IsDestroyed)
                    {
                        if (X + Radius > brick.X && X < brick.X + brick.Width && Y + Radius > brick.Y && Y < brick.Y + brick.Height)
                        {
                            brick.IsDestroyed = true;

                            // Zmieñ kierunek ruchu pi³ki w zale¿noœci od miejsca kolizji
                            if (X + Radius - dx <= brick.X || X - dx >= brick.X + brick.Width)
                            {
                                dx = -dx; // Kolizja z bokiem ceg³y
                            }
                            else
                            {
                                dy = -dy; // Kolizja z gór¹ lub do³em ceg³y
                            }
                            return;
                        }
                    }
                }
            }
        }

        public void Draw(Graphics g)
        {
            g.FillEllipse(Brushes.Red, X, Y, Radius, Radius);
        }
    }

    public class Brick
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsDestroyed { get; set; }
        public Color Color { get; set; } // Dodano pole koloru

        public Brick(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsDestroyed = false;
            Color = Color.Gray; // Domyœlny kolor
        }

        public void Draw(Graphics g)
        {
            if (!IsDestroyed)
            {
                using (Brush brush = new SolidBrush(Color))
                {
                    g.FillRectangle(brush, X, Y, Width, Height);
                }
            }
        }
    }
}
