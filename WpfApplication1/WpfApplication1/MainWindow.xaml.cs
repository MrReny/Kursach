using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Configuration;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Path = System.IO.Path;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static bool _isUp;
        private static bool _isDown;
        private static bool _isLeft;
        private static bool _isRight;
        private static bool _isSpaceUp = true;
        private static bool _isStarted;
        private static bool _isPaused;
        private static bool _isGameOver;
        private static bool _isLevelFinished;

        private static int _bulletCount;

        public int DifficultyLevel;
        public int LevelsCompleted;
        
        public List<String[]> ScoresList= new List<String[]>();
        
        public PawnMove Pawn;

        public EnemyHorde Horde;

        public MainWindow()
        {
            InitializeComponent();
            Load();
            StartMenuLayout.Focus();
            StartMenuLayout.FocusVisualStyle = null;
            MenuLayout.FocusVisualStyle = null;
            Canvas1.FocusVisualStyle = null;
            this.Closing += Save;
        }

        private void PMove(double x = 0, double y = 0)
        {
            if(Pawn!=null)
            {
                Pawn.SMove(x,y);
            }
        }

        private void LayoutMain_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                _isUp = true;
            }
            else if(e.Key == Key.Down)
            {
                _isDown = true;
            }
            else if(e.Key == Key.Right)
            {
                _isRight = true;
            }
            else if(e.Key == Key.Left)
            {
                _isLeft = true;
            }
            else if (e.Key == Key.Space && _isSpaceUp)
            {
                if (Pawn!=null)
                {
                    Pawn.FireBullet(ref Canvas1);
                    _bulletCount++;
                    TextBox1.Content = _bulletCount;
                    _isSpaceUp = false;
                }
            }
            else if (e.Key == Key.Escape)
            {
                ChangeScreens();
            }
        }

        private void LayoutMain_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                _isUp = false;
            }
            else if(e.Key == Key.Down)
            {
                _isDown = false;
            }
            else if(e.Key == Key.Right)
            {
                _isRight = false;
            }
            else if(e.Key == Key.Left)
            {
                _isLeft = false;
            }
            else if (e.Key == Key.Space)
            {
                _isSpaceUp = true;
            }

        }

        private async Task TimerMoveAsync()
        {
            while (true)
            {
                await Task.Delay(10);
                if (_isUp)
                {
                    PMove(y: -10);
                }
                else if (_isDown)
                {
                    PMove(y: 10);
                }

                if (_isRight)
                {
                    PMove(x: 10);
                }
                else if (_isLeft)
                {
                    PMove(x: -10);
                }
                
                if(_isPaused) return;
            }
        }

        private async Task BulletMoveAsync()
        {
            //TODO: Find out why async dont work, or find better solution
            while (true)
            {
                await Task.Delay(10);
                if (Pawn != null)
                {
                    Pawn.MoveBullets();
                }
                
                if(_isPaused) return;
            }
        }

        private async Task HordeMoveAsync()
        {
            while (true)
            {
                await Task.Delay(16);
                if (Horde != null)
                {
                    Horde.MoveHorde();
                }
                
                if(_isPaused) return;
            }
        }
        private void MenuLayout_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ChangeScreens();
            }
        }

        private void ChangeScreens()
        {
            GameLayout.IsEnabled = !GameLayout.IsEnabled;
            MenuLayout.IsEnabled = !MenuLayout.IsEnabled;
           
            if (StartMenuLayout.IsEnabled)
            {
                StartMenuLayout.IsEnabled = false;
                StartMenuLayout.Visibility = Visibility.Collapsed;
            }
            else if (_isGameOver)
            {
                _isGameOver = !_isGameOver;
                GameLayout.Visibility = Visibility.Collapsed;
                MenuLayout.IsEnabled = false;
                _isPaused = true;
            }
            else if(_isLevelFinished)
            {
                _isLevelFinished = !_isLevelFinished;
                GameLayout.Visibility = Visibility.Collapsed;
                MenuLayout.IsEnabled = false;
                _isPaused = true;
            }
            
            if (GameLayout.Visibility == Visibility.Visible)
            {
                GameLayout.Visibility = Visibility.Collapsed;
                MenuLayout.Visibility = Visibility.Visible;
                MenuLayout.Focus();
                _isPaused = true;
            }
            else
            {
                GameLayout.IsEnabled = true;
                GameLayout.Visibility= Visibility.Visible;
                MenuLayout.Visibility = Visibility.Collapsed;
                Canvas1.Focus();
                _isPaused = false;
                TimerMoveAsync();
                BulletMoveAsync();
                HordeMoveAsync();
            }
            

        }

        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isStarted)
            {
                Canvas1.Focus();
                ChangeScreens();
            }
            else
            {    
                if (_isGameOver||_isLevelFinished) Canvas1.Children.Clear();
                Pawn = new PawnMove(ref Canvas1,"MainCube",LayoutMain.ActualWidth/2, LayoutMain.ActualHeight/2,10,10);
                Canvas1.Children.Add(Pawn.PawnSprite);
                Canvas1.Focus();
                ChangeScreens();
                Countdown();
                _isStarted = true;
            }
        }

        private void Countdown()
        {
            Label l1 = new Label();
            l1.FontSize = 78;
            l1.Content = 3;
            Canvas1.Children.Add(l1);
            l1.SetValue(Canvas.LeftProperty, LayoutMain.ActualWidth/2 - l1.FontSize/3);
            
            Int32Animation countdownAnim = new Int32Animation();
            countdownAnim.From = 3;
            countdownAnim.To = 0;
            countdownAnim.Duration = TimeSpan.FromSeconds(4);
            countdownAnim.Completed += CountdownAnimCompleted;
            l1.BeginAnimation(Label.ContentProperty, countdownAnim);
            
            void CountdownAnimCompleted(object sender, EventArgs e)
            {
                l1.Visibility = Visibility.Hidden;
                Horde = new EnemyHorde(ref Canvas1);
                Pawn.CurrentHorde = Horde.Horde;
            }
        }

        public void GameOver()
        {
            _isGameOver = true;
            _isStarted = false;
            _isPaused = true;
            Pawn = null;
            Horde = null;
            GameOverFrame();
        }

        private void GameOverFrame()
        {
            Grid gameOverMenuGrid = new Grid(){ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition()},
                                                                    Style = (Style)this.Resources["BlackAndWhite"],
                                                                    MaxHeight = this.Height - 200};
            
            StackPanel gameOverMenu = new StackPanel(){Style = (Style)this.Resources["BlackAndWhite"]};
            
            Label gameOverLabel = new Label(){Style = (Style)this.Resources["BlackAndWhite"]};
            gameOverLabel.FontSize = 78;
            gameOverLabel.Content = "Game Over";
            gameOverLabel.FontFamily = new FontFamily("Verdana");
            
            Button restartButton = new Button(){Style = (Style)this.Resources["BlackAndWhite"]};
            restartButton.Content = "Restart";
            restartButton.FontSize = 22;
            restartButton.Click += ButtonStart_OnClick;
            
            Button exitButton = new Button(){Style = (Style)this.Resources["BlackAndWhite"]};
            exitButton.Content = "Exit";
            exitButton.FontSize = 22;
            exitButton.Click += ExitClick;
            
            gameOverMenu.Children.Add(gameOverLabel);
            gameOverMenu.Children.Add(restartButton);
            gameOverMenu.Children.Add(exitButton);
            gameOverMenu.Width = 460;
            gameOverMenu.SetValue(Grid.ColumnProperty,0);
            
            gameOverMenuGrid.Children.Add(gameOverMenu);
            ScoreBoard(ref gameOverMenuGrid);

            Canvas1.Children.Add(gameOverMenuGrid);
        }

        public void LevelComplete()
        {
            _isLevelFinished = true;
            _isStarted = false;
            _isPaused = true;
            Horde = null;
            LevelsCompleted++;
            TextBox3.Content = LevelsCompleted;
            LevelCompleteFrame();
        }

        public void LevelCompleteFrame()
        {   
            Grid levelCompleteMenuGrid = new Grid(){ColumnDefinitions = {new ColumnDefinition(), new ColumnDefinition()},
                                                                        Style = (Style)this.Resources["BlackAndWhite"],
                                                                        MaxHeight = this.Height - 200};
            
            StackPanel levelCompleteMenu = new StackPanel(){Style = (Style)this.Resources["BlackAndWhite"]};
            
            Label levelCompleteLabel = new Label(){Style = (Style)this.Resources["BlackAndWhite"]};
            levelCompleteLabel.FontSize = 48;
            levelCompleteLabel.Content = "Level Complete";
            levelCompleteLabel.FontFamily = new FontFamily("Verdana");
            
            Button nextLevelButton = new Button(){Style = (Style)this.Resources["BlackAndWhite"]};
            nextLevelButton.Content = "Next level";
            nextLevelButton.FontSize = 22;
            nextLevelButton.Click += NextLevel_OnClick;
            
            Button exitButton = new Button(){Style = (Style)this.Resources["BlackAndWhite"]};
            exitButton.Content = "Exit";
            exitButton.FontSize = nextLevelButton.FontSize;
            exitButton.Click += ExitClick;
            
            levelCompleteMenu.Children.Add(levelCompleteLabel);
            levelCompleteMenu.Children.Add(nextLevelButton);
            levelCompleteMenu.Children.Add(exitButton);
            levelCompleteMenu.Width = 380;
            levelCompleteMenu.SetValue(Grid.ColumnProperty,0);

            levelCompleteMenuGrid.Children.Add(levelCompleteMenu);
            ScoreBoard(ref levelCompleteMenuGrid);

            Canvas1.Children.Add(levelCompleteMenuGrid);
        }
        
        public void NextLevel_OnClick(object sender, RoutedEventArgs e)
        {
            DifficultyLevel++;
            ButtonStart_OnClick(sender, e);
        }
        public void KillCount(int count, int scores)
        {
            TextBox2.Content = count;
            TextBox4.Content = scores;
        }

        public void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void ScoreBoard(ref Grid parent)
        {
            Grid scoreBoardGrid = new Grid(){RowDefinitions = { new RowDefinition(){Height = GridLength.Auto},
                                                                new RowDefinition()
                                                              }, Style = (Style)this.Resources["BlackAndWhite"]};
            scoreBoardGrid.SetValue(Grid.ColumnProperty, 1);
            
            StackPanel titlePanel = new StackPanel(){Orientation = Orientation.Horizontal, Style = (Style)this.Resources["BlackAndWhite"]};
            titlePanel.SetValue(Grid.RowProperty, 0);
            
            Label enterNameLabel = new Label(){Content = "Enter your name:", FontSize = 22,
                                                Style = (Style)this.Resources["BlackAndWhite"]};
            TextBox nameBox = new TextBox(){FontSize = 22, Width = 100,MaxLength = 10, 
                                                Style = (Style)this.Resources["BlackAndWhite"]};
            Label scoreLabel = new Label(){Content = "Score: "+TextBox4.Content, FontSize = 22, 
                                                Style = (Style)this.Resources["BlackAndWhite"]};
            Button saveButton = new Button(){Content = "Save", FontSize = 22, 
                                                Style = (Style)this.Resources["BlackAndWhite"], Focusable = false};
            saveButton.Click += AddRecord;
            saveButton.Click += ClickB;

            titlePanel.Children.Add(enterNameLabel);
            titlePanel.Children.Add(nameBox);
            titlePanel.Children.Add(scoreLabel);
            titlePanel.Children.Add(saveButton);
            
            scoreBoardGrid.Children.Add(titlePanel);
            
            
            Grid boardGrid = MakeBoard();
            
            ScrollViewer scroll = new ScrollViewer(){Style = (Style)this.Resources["BlackAndWhite"]};
            scroll.SetValue(Grid.RowProperty, 1);
            scroll.Content = boardGrid;
            
            scoreBoardGrid.Children.Add(scroll);
            
            parent.Children.Add(scoreBoardGrid);
            void ClickB(object sender, RoutedEventArgs e)
            {
                scroll = new ScrollViewer(){Style = (Style)this.Resources["BlackAndWhite"]};
                boardGrid = MakeBoard();
                
                scroll.SetValue(Grid.RowProperty, 1);
                scroll.Content = boardGrid;
                scoreBoardGrid.Children.Add(scroll);
                titlePanel.Visibility = Visibility.Collapsed;
            }

        }

        public Grid MakeBoard()
        {
            Label[,] labels = new Label[ScoresList.Count, 3];
            Grid boardGrid = new Grid(){ColumnDefinitions = { new ColumnDefinition(){Width = GridLength.Auto},
                                                              new ColumnDefinition(){Width = GridLength.Auto},
                                                              new ColumnDefinition(){Width = GridLength.Auto}},
                                        Style = (Style)this.Resources["BlackAndWhite"]
                                        };
            
            boardGrid.RowDefinitions.Add(new RowDefinition(){Height = GridLength.Auto});
            
            Label posLabel = new Label(){Content = "#.", Style = (Style)this.Resources["BlackAndWhite"]};
            posLabel.SetValue(Grid.RowProperty,0);
            posLabel.SetValue(Grid.ColumnProperty,0);
            boardGrid.Children.Add(posLabel);
            
            Label nameLabel = new Label(){Content = "Name", Style = (Style)this.Resources["BlackAndWhite"]};
            nameLabel.SetValue(Grid.RowProperty,0);
            nameLabel.SetValue(Grid.ColumnProperty,1);
            boardGrid.Children.Add(nameLabel);
            
            Label pointLabel = new Label(){Content = "Score", Style = (Style)this.Resources["BlackAndWhite"]};
            pointLabel.SetValue(Grid.RowProperty,0);
            pointLabel.SetValue(Grid.ColumnProperty,2);
            boardGrid.Children.Add(pointLabel);
            
            
            for (int i = 1; i <= ScoresList.Count; i++)
            {
                boardGrid.RowDefinitions.Add(new RowDefinition(){Height = GridLength.Auto});
                for (int j = 0; j < 3; j++)
                {
                    labels[i-1,j] = new Label(){Content = ScoresList[i-1][j], Style = (Style)this.Resources["BlackAndWhite"]};
                    labels[i-1,j].SetValue(Grid.RowProperty, i);
                    labels[i-1,j].SetValue(Grid.ColumnProperty,j);
                    boardGrid.Children.Add(labels[i-1, j]);

                }
            }

            return boardGrid;
        }

        public void Load()
        {
            string path =Environment.CurrentDirectory + @"\stats.dat"; 
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    while (reader.PeekChar() > -1)
                    {
                        ScoresList.Add(new string[]{reader.ReadString(),reader.ReadString(),reader.ReadString()});
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        
        public void Save(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string path = Environment.CurrentDirectory + @"\stats.dat";
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
                {
                    foreach (string[] str in ScoresList)
                    {
                        writer.Write(str[0]);
                        writer.Write(str[1]);
                        writer.Write(str[2]);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ScoreSorting()
        {
            for (int i = 1; i < ScoresList.Count; i++)
            {
                for (int j = i; j > 0 && Int32.Parse(ScoresList[j - 1][2]) < Int32.Parse(ScoresList[j][2]); j--)
                {
                    
                    String temp1 = ScoresList[j - 1][1];
                    String temp2 = ScoresList[j - 1][2];

                    ScoresList[j - 1][1] = ScoresList[j][1];
                    ScoresList[j - 1][2] = ScoresList[j][2];
                    ScoresList[j][1] = temp1;
                    ScoresList[j][2] = temp2;

                }
            }
        }
        public void AddRecord(object sender, RoutedEventArgs e)
        {
            String st;
            Button s = (Button)e.Source;
            StackPanel p = (StackPanel)UIHelper.GetParent(s);
            TextBox n = (TextBox)UIHelper.GetChildOfType<TextBox>(p);
            if (n.Text == "")
                st = "noname";
            else
                st = n.Text;
            ScoresList.Add(new string[]{ScoresList.Count + 1 + ".",st,TextBox4.Content.ToString()});
            ScoreSorting();
        }
    }
}
