using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArkanoidTeledynamo64Port
{
    public partial class LevelEditorForm : Form
    {
        private Button saveButton;
        private Button loadButton;
        private Button colorButton; // Dodano deklarację pola colorButton
        private Brick?[,] bricks;

        public LevelEditorForm(Brick?[,] bricks)
        {
            InitializeComponent();
            this.bricks = bricks;
            this.colorButton = new Button(); // Initialize colorButton here
            this.saveButton = new Button(); // Initialize saveButton here
            this.loadButton = new Button(); // Initialize loadButton here
            InitializeEditor();
        }

        private void InitializeEditor()
        {
            this.Width = 800;
            this.Height = 600;
            this.DoubleBuffered = true;

            this.MouseClick += LevelEditorForm_MouseClick;
            this.KeyDown += LevelEditorForm_KeyDown;

            // Configure the colorButton
            colorButton.Text = "Wybierz kolor";
            colorButton.Location = new Point(10, 10);
            colorButton.Click += ColorButton_Click;
            this.Controls.Add(colorButton);

            // Configure the saveButton
            saveButton.Text = "Zapisz poziom";
            saveButton.Location = new Point(10, 50);
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            // Configure the loadButton
            loadButton.Text = "Wczytaj poziom";
            loadButton.Location = new Point(10, 90);
            loadButton.Click += LoadButton_Click;
            this.Controls.Add(loadButton);
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Stage files (*.stag)|*.stag";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SaveLevelToFile(saveFileDialog.FileName);
                }
            }
        }

        private void LoadButton_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Stage files (*.stag)|*.stag";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadLevelFromFile(openFileDialog.FileName);
                    Invalidate();
                }
            }
        }

        private void SaveLevelToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                int rows = bricks.GetLength(0);
                int cols = bricks.GetLength(1);
                writer.WriteLine($"{rows} {cols}");
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        var brick = bricks[row, col];
                        if (brick != null)
                        {
                            writer.WriteLine($"{row} {col} {brick.Color.ToArgb()}");
                        }
                    }
                }
            }
        }

        private void LoadLevelFromFile(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string[] dimensions = reader.ReadLine().Split(' ');
                int rows = int.Parse(dimensions[0]);
                int cols = int.Parse(dimensions[1]);
                bricks = new Brick?[rows, cols];

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(' ');
                    int row = int.Parse(parts[0]);
                    int col = int.Parse(parts[1]);
                    Color color = Color.FromArgb(int.Parse(parts[2]));
                    bricks[row, col] = new Brick(col * (this.ClientSize.Width / cols), row * (this.ClientSize.Height / (rows * 3)), this.ClientSize.Width / cols, this.ClientSize.Height / (rows * 3)) { Color = color };
                }
            }
        }

        private void ColorButton_Click(object? sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    // Handle color selection
                }
            }
        }

        private void LevelEditorForm_MouseClick(object? sender, MouseEventArgs e)
        {
            // Implementacja obsługi zdarzenia MouseClick
        }

        private void LevelEditorForm_KeyDown(object? sender, KeyEventArgs e)
        {
            // Implementacja obsługi zdarzenia KeyDown
        }
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
        Color = Color.White; // Default color
    }

    public void Draw(Graphics g)
    {
        using (Brush brush = new SolidBrush(Color))
        {
            g.FillRectangle(brush, X, Y, Width, Height);
        }
    }
}
