using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameOfLife
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer Timer;
        const int FramesPerSecond = 10;

        private int CELL_SIZE = Properties.Settings.Default.CELL_SIZE;

        ICollection<Cell> OldGeneration;
        ICollection<Cell> NewGeneration;

        readonly Brush AliveCellBrush = Brushes.Black;
        readonly Brush DeadCellBrush = Brushes.White;

        public MainWindow()
        {
            InitializeComponent();

            InitializeTimer();

            InitializeBoard();
        }

        void InitializeTimer()
        {
            Timer = new DispatcherTimer();
            Timer.Tick += delegate { UpdateBoard(); };
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        }
        
        void InitializeBoard()
        {
            Board.Width = CELL_SIZE * Properties.Settings.Default.BOARD_ROWS_NUMBER;
            Board.Height = CELL_SIZE * Properties.Settings.Default.BOARD_COLUMNS_NUMBER;

            var totalCellsNumber = Properties.Settings.Default.BOARD_ROWS_NUMBER * Properties.Settings.Default.BOARD_COLUMNS_NUMBER;

            OldGeneration = new List<Cell>();
            NewGeneration = new List<Cell>();

            OldGenerationList();
            NewGenerationList();
        }

        private void OldGenerationList()
        {
            for (int currentRow = 0; currentRow < Properties.Settings.Default.BOARD_ROWS_NUMBER; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < Properties.Settings.Default.BOARD_COLUMNS_NUMBER; currentColumn++)
                {
                    int x = currentRow * CELL_SIZE,
                        y = currentColumn * CELL_SIZE;

                    var newCell = new Cell(x, y, false);

                    OldGeneration.Add(newCell);

                    DrawingCells(newCell);
                }
            }
        }

        private void NewGenerationList()
        {
            NewGeneration.Clear();

            foreach (var item in OldGeneration)
            { 
                int x = item.X,
                    y = item.Y;

                var newCell = new Cell(x, y, item.IsAlive);

                NewGeneration.Add(newCell);
            }
        }

        void UpdateBoard()
        {
            foreach (var cell in OldGeneration)
            {
                var neighbourCellsNumber = GetCellNeighboursNumber(cell);

                if (neighbourCellsNumber == 3 && cell.IsAlive == false)
                    NewGeneration.First(newCell => newCell.X == cell.X && newCell.Y == cell.Y).IsAlive = true;
                else if ((neighbourCellsNumber < 2 || neighbourCellsNumber > 3) && cell.IsAlive == true)
                    NewGeneration.First(newCell => newCell.X == cell.X && newCell.Y == cell.Y).IsAlive = false;

            }

            OldGeneration.Clear();
            Board.Children.Clear();

            foreach (var cell in NewGeneration)
            {
                OldGeneration.Add(cell);

                UpdateBoardCell(cell);
            }

            foreach (var item in OldGeneration)
                DrawingCells(item);

            NewGenerationList();
        }

        int GetCellNeighboursNumber(Cell cell)
            => OldGeneration.Count(x =>
            (cell.X + CELL_SIZE == x.X && cell.Y == x.Y && x.IsAlive == true) 
            || (cell.X == x.X && cell.Y - CELL_SIZE == x.Y && x.IsAlive == true) 
            || (cell.X - CELL_SIZE == x.X && cell.Y == x.Y && x.IsAlive == true)
            || (cell.X == x.X && cell.Y + CELL_SIZE == x.Y && x.IsAlive == true)
            || (cell.X + CELL_SIZE == x.X && cell.Y + CELL_SIZE == x.Y && x.IsAlive == true) 
            || (cell.X - CELL_SIZE == x.X && cell.Y - CELL_SIZE == x.Y && x.IsAlive == true)
            || (cell.X + CELL_SIZE == x.X && cell.Y - CELL_SIZE == x.Y && x.IsAlive == true) 
            || (cell.X - CELL_SIZE == x.X && cell.Y + CELL_SIZE == x.Y && x.IsAlive == true));  

        private void DrawingCells(Cell cell)
        {
            Board.Children.Add(cell.Rectangle);
            Canvas.SetLeft(cell.Rectangle, cell.X);
            Canvas.SetTop(cell.Rectangle, cell.Y);
        }

        void MarkBoardCellAsLive(int mouseX, int mouseY)
        {
            var cell = OldGeneration.FirstOrDefault(c =>
            c.X - CELL_SIZE <= mouseX
            && c.X + CELL_SIZE >= mouseX
            && c.Y - CELL_SIZE <= mouseY 
            && c.Y + CELL_SIZE >= mouseY);

            cell.IsAlive = true;

            UpdateBoardCell(cell);
            NewGenerationList();
        }

        void UpdateBoardCell(Cell cell)
            => cell.Rectangle.Fill = cell.IsAlive ? AliveCellBrush : DeadCellBrush;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int mouseX = Convert.ToInt32(e.GetPosition(Board).X),
                   mouseY = Convert.ToInt32(e.GetPosition(Board).Y);

            MarkBoardCellAsLive(mouseX, mouseY);
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
                Timer.Stop();

            if (e.Key == Key.Space)
                Timer.Start();
        }
    }
}
