using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace WpfApplication1
{
    public class Pawn
    {
        private static int _counter;
        private readonly int _id;
        private string _name;

        public static int Counter { get; } = _counter;

        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Pawn()
        {
            _counter++;
            _id = _counter;
            _name = Convert.ToString(_counter);
        }

        public Pawn(string name) : this()
        {
            _name = name;
        }
    }

    public class PawnMove : Pawn
    {
        private double _x = 0;
        private double _y = 0;

        private double _w = 1;
        private double _h = 1;

        public Shape PawnSprite;

        private Canvas _parent;

        public List<Bullet> Bullets = new List<Bullet>();

        public List<Enemy> CurrentHorde;
        
        public double W
        {
            get { return _w; }
            set { _w = value; }
        }

        public double H
        {
            get { return _h; }
            set { _h = value; }
        }

        public double X => _x;

        public double Y => _y;


        public PawnMove()
        {
            _initSprite();
        }

        public PawnMove(double x, double y) : this()
        {
            _initCoords(x, y);
        }

        public PawnMove(double x, double y, double w, double h):this(x,y)
        {
            InitSize(w,h);
        }

        public PawnMove(string name) : base(name)
        {
            _initSprite();            
        }

        public PawnMove(string name, double x, double y) : this(name)
        {
            _initCoords(x, y);
        }
        public PawnMove(string name, double x, double y, double w, double h) : this(name, x, y)
        {
            InitSize(w, h);
            
        }
        public PawnMove(ref Canvas canvas, string name, double x, double y, double w, double h):this(name,x,y)
        {
            InitSize(w,h);
            _parent = canvas;
        }

        private void _initCoords(double x, double y)
        {
            _x = x;
            _y = y;
            if (PawnSprite!=null)
            {
                Relocate();
            }
        }

        private void _initSprite()
        {
            PawnSprite = new Rectangle(){Width = _w, Height = _h};
            PawnSprite.Stroke = Brushes.Black;
        }

        public void InitSize(double w, double h)
        {    // TODO: Find out better solution for resizing 
            _w = w;
            _h = h;

            PawnSprite.Width = _w;
            PawnSprite.Height = _h;
        }

        public void Relocate()
        {
            PawnSprite.SetValue(Canvas.LeftProperty, _x);
            PawnSprite.SetValue(Canvas.TopProperty, _y);
        }

        public void Move(double x=0, double y=0)
        {
            _x += x;
            if (_x < 0) _x = 0;
            if (_x > (double) PawnSprite.Parent?.GetValue(Canvas.ActualWidthProperty)-_w)
                _x = (double) PawnSprite.Parent?.GetValue(Canvas.ActualWidthProperty)-_w;
            _y += y;
            if (_y < 0) _y = 0;
            if (_y > (double) PawnSprite.Parent?.GetValue(Canvas.ActualHeightProperty)-_h)
                _y = (double) PawnSprite.Parent?.GetValue(Canvas.ActualHeightProperty)-_h;
            Relocate();
        }

        public void SMove(double x = 0, double y = 0)
        {
            Move(x,y);
            FindCollisions(CurrentHorde);
        }

        public void FireBullet(ref Canvas canvas)
        {
            Bullet b = new Bullet(this);
            Bullets.Add(b);
            canvas.Children.Add(b.PawnSprite);
            if (_parent==null) _parent = canvas;
        }

        public void DestroyBullet(Bullet bullet)
        {
            Bullets.Remove(bullet);
            _parent.Children.Remove(bullet.PawnSprite);
        }

        public void MoveBullets()
        {
           if (Bullets.Count > 0)
            {
                for (var i = 0; i <= Bullets.Count-1; i++)
                {
                    Bullets[i].BulletMove();
                }
            }
        }
        public virtual void FindCollisions(List<Enemy> horde)
        {    
            
            if (horde != null)
            {
                for (int i = 0; i <= horde.Count-1; i++)
                {
                    if ((horde[i].X <= this.X && this.X <= horde[i].X + horde[i].W) &&
                        (horde[i].Y <= this.Y && this.Y <= horde[i].Y + horde[i].H))
                    {
                        horde[i].SelfDestruct();
                        SelfDestruct();
                    }
                }
            }
        }
        public delegate void MethodContainer();

        public event MethodContainer onGameOver;

        public virtual void SelfDestruct()
        {
            CurrentHorde = null;
            //temporary blank TODO: finish game
            onGameOver();
            //UIHelper.GetAncestor<MainWindow>(_parent).GameOver();
        }


    }

    public class Bullet : PawnMove
    {
        private static int _c;
        private PawnMove _parent;
        public static int C { get; } = _c;

        public Bullet(PawnMove p):base( "Bullet"+1,p.X+p.W/2 - 2.5, p.Y - 3, 5, 5)
        {
            _parent = p;
            _c++;
            _initBulletSprite();
        }
        
        private void _initBulletSprite()
        {
            PawnSprite = new Ellipse() { Width = 5, Height = 5 };
            PawnSprite.Stroke = Brushes.Black;
            PawnSprite.Fill = Brushes.Red;
        }

        public void BulletMove()
        {
            Move(y: -5);
            FindCollisions(_parent.CurrentHorde);
            if (Y <= 0)
            {
                SelfDestruct();
            }
        }

        public override void SelfDestruct()
        {
            _parent.DestroyBullet(this);
        }
    }

    public class Enemy : PawnMove
    {
        public static int ECount;
        public static int CeCount;
        public static int DeCount;
        public static int Scores;

        private bool _mDirection;
        private MainWindow _mWindow;
        private Canvas _parent;
        private List<Enemy> _pHorde;
        public Enemy(ref Canvas canvas,ref List<Enemy> horde,double x, double y):base(x, y, 20, 20)
        {
            _parent = canvas;
            _parent.Children.Add(this.PawnSprite);
            _mWindow = UIHelper.GetAncestor<MainWindow>(_parent);
            OnLevelComplete += _mWindow.LevelComplete;
            _pHorde = horde;
            ECount++;
            CeCount++;
        }

        public void EMove()
        {
            if (X+W >= _parent.ActualWidth || X<=0)
            {
                _mDirection = !_mDirection;
                Move(y:20);
                if (Y + H >= _parent.ActualHeight)
                {
                    _mWindow.Pawn.SelfDestruct();
                }
            }
            
            if (_mDirection)
            {
                Move(-5 - 1*_mWindow.DifficultyLevel);
            }
            else
            {
                Move(5 + 1*_mWindow.DifficultyLevel);
            }
            IsCollision();
        }

        public void IsCollision()
        {
            var player = _mWindow.Pawn;
            if ((this.X <= player.X && player.X <= this.X + this.W) &&
                (this.Y <= player.Y && player.Y <= this.Y + this.H))
            {
                SelfDestruct();
                player.SelfDestruct();
            }
        }
        public delegate void LvlCompleteContainer();

        public event LvlCompleteContainer OnLevelComplete;

        public override void SelfDestruct()
        {
            _parent.Children.Remove(PawnSprite);
            _pHorde.Remove(this);
            Scores += 1 + 1 * _mWindow.DifficultyLevel;
            DeCount++;
            _mWindow.KillCount(DeCount, Scores);
            if (_pHorde.Count == 0) OnLevelComplete();//_mWindow.LevelComplete();
        }
    }

    public class HardEnemy: Enemy
    {
        private int _health;
        public HardEnemy(ref Canvas canvas, ref List<Enemy> horde, double x, double y) : base(ref canvas, ref horde,x, y)
        {
            _health = 2;
            _reShape(Brushes.Navy);
        }

        public override void SelfDestruct()
        {
            _health--;
            Scores++;
            if(_health==0) base.SelfDestruct();
            _reShape(Brushes.Red);
        }
        
        private void _reShape(Brush b)
        {
            this.PawnSprite.Fill = b;
        } 
    }

    public class EnemyHorde
    {
        private int _count;
        private Canvas _parent;
        public List<Enemy> Horde = new List<Enemy>();
        

        public EnemyHorde(ref Canvas canvas)
        {
            _parent = canvas;
            CreateHorde();
        }

        private void CreateHorde()
        {
            double w = _parent.ActualWidth;
            double h = _parent.ActualHeight / 2;

            int wCount = ((int)w - 50) / 40;
            int hCount = ((int)h - 10) / 60;

            for (int i = 0; i <= hCount; i++)
            {
                for (int j = 0; j <= wCount; j++)
                {
                    if((j+i)%2==0) Horde.Add(new Enemy(ref _parent,ref Horde,30 + 40*j,10 + 40*i));
                    else Horde.Add(new HardEnemy(ref _parent, ref Horde, 30 + 40 * j, 10 + 40 * i));
                }
            }
        }

        public void MoveHorde()
        {
            if (Horde.Count > 0)
            {
                for (var i = 0; i <= Horde.Count - 1; i++)
                {
                    Horde[i].EMove();
                }
            }
        }

        public void DestroyHorde()
        {
            for (var i = Horde.Count-1; i>= 0 ;i--)
            {
                Horde[i].SelfDestruct();
            }

            Horde = null;
        }
    }
}