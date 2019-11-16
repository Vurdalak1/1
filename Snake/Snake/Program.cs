using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snake
{
    class Program
    {
        private static Timer timer;
        private static Game game;

        static void Main(string[] args)
        {
            Console.SetWindowSize(80 + 1, 30 + 1);
            Console.SetBufferSize(80 + 1, 30 + 1);
            Console.CursorVisible = false;

            game = new Game(80, 30);
            game.Snake.IsAlive = true;
            game.FoodFactory.Create();

            timer = new Timer(game.Loop, null, 0, 125);

            while (game.Snake.IsAlive)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                game.Snake.Rotate(key.Key);
            }

            game.GameOver();
        }
    }

    public enum Directions
    {
        Left,
        Right,
        Up,
        Down
    }

    public class Point
    {
        public Point(int x, int y, char symbol)
        {
            this.X = x;
            this.Y = y;
            this.Symbol = symbol;
        }

        public int X { get; }
        public int Y { get; }
        public char Symbol { get; set; }

        public void Draw()
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(Symbol);
        }

        public void Clear()
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(" ");
        }

        public bool IsEqual(Point p)
        {
            if (this.X == p.X && this.Y == p.Y)
                return true;
            else
                return false;
        }
    }

    public class Walls
    {
        private List<Point> walls = new List<Point>();

        public List<Point> GetWalls => walls;

        public Walls(int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                Point topPoint = new Point(i, 0, '#');
                Point botPoint = new Point(i, height, '#');

                topPoint.Draw();
                botPoint.Draw();

                walls.AddRange(new List<Point>() { topPoint, botPoint });
            }

            for (int i = 0; i < height; i++)
            {
                Point leftPoint = new Point(0, i, '#');
                Point rightPoint = new Point(width, i, '#');

                leftPoint.Draw();
                rightPoint.Draw();

                walls.AddRange(new List<Point>() { leftPoint, rightPoint });
            }
        }  
    }

    public class FoodFactory
    {
        private int width;
        private int height;
        private Point currentFood;

        public Point CurrentFood => currentFood;

        public FoodFactory(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public void Create()
        {
            Random rnd = new Random();

            int x = rnd.Next(1, width - 1);
            int y = rnd.Next(1, height - 1);

            Point food = new Point(x, y, '@');

            food.Draw();

            currentFood = food;
        }
    }

    public class Snake
    {
        private List<Point> body = new List<Point>();
        private Directions direction = Directions.Right;
        private bool canRotate = true;

        public Snake(int length, int width, int height)
        {
            for (int i = 0; i < length; i++)
            {
                Point bodyPart = new Point((width / 2) + i, height / 2, 'x');
                body.Add(bodyPart);
                bodyPart.Draw();
            }
        }

        public Point Head => body.Last();
        public Point Tail => body.First();
        public List<Point> Body => body;
        public bool IsAlive { get; set; }
        public Directions Direction => direction;

        public void Move(bool clearTail = true)
        {
            Point newHead;

            switch (direction)
            {
                case Directions.Right:
                    newHead = new Point(Head.X + 1, Head.Y, 'x');
                    break;
                case Directions.Left:
                    newHead = new Point(Head.X - 1, Head.Y, 'x');
                    break;
                case Directions.Up:
                    newHead = new Point(Head.X, Head.Y - 1, 'x');
                    break;
                case Directions.Down:
                    newHead = new Point(Head.X, Head.Y + 1, 'x');
                    break;
                default:
                    goto case Directions.Right;
            }

            body.Add(newHead);
            Head.Draw();

            if (clearTail)
            {
                Tail.Clear();
                body.Remove(Tail);
            }

            canRotate = true;
        }

        public void Rotate(ConsoleKey key)
        {
            if (canRotate)
            {
                switch (direction)
                {
                    case Directions.Down:
                    case Directions.Up:
                        if (key == ConsoleKey.LeftArrow)
                        {
                            direction = Directions.Left;
                        }
                        else if (key == ConsoleKey.RightArrow)
                        {
                            direction = Directions.Right;
                        }
                        break;
                    case Directions.Right:
                    case Directions.Left:
                        if (key == ConsoleKey.UpArrow)
                        {
                            direction = Directions.Up;
                        }
                        else if (key == ConsoleKey.DownArrow)
                        {
                            direction = Directions.Down;
                        }
                        break;
                }

                canRotate = false;
            }
        }

        public void Grow(Point p)
        {
            p.Symbol = 'x';
            body.Add(p);
        }
    }

    public class Game
    {
        private Walls walls;
        private Snake snake;
        private FoodFactory foodFactory;

        public Game(int width, int height)
        {
            walls = new Walls(width, height);
            snake = new Snake(3, width, height);
            foodFactory = new FoodFactory(width, height);
        }

        public Walls Walls => walls;
        public Snake Snake => snake;
        public FoodFactory FoodFactory => foodFactory;
        public int Width { get; } 
        public int Height { get; }

        public void Loop(object obj)
        {
            if (walls.GetWalls.Any(x => x.IsEqual(snake.Head)))
            {
                snake.IsAlive = false;
                GameOver();
                return;
            }

            if (snake.Body.Take(snake.Body.Count - 1).Any(x => x.Equals(snake.Head)))
            {
                snake.IsAlive = false;
                GameOver();
                return;
            }

            int nextX = snake.Head.X;
            int nextY = snake.Head.Y;

            switch (snake.Direction)
            {
                case Directions.Right:
                    nextX++;
                    break;
                case Directions.Left:
                    nextX--;
                    break;
                case Directions.Down:
                    nextY++;
                    break;
                case Directions.Up:
                    nextY--;
                    break;
            }
            if (foodFactory.CurrentFood.X == nextX && foodFactory.CurrentFood.Y == nextY)
            {
                snake.Grow(foodFactory.CurrentFood);
                foodFactory.Create();
                return;
            }

            snake.Move();
        }

        public void GameOver()
        {
            int x = 0;
            int y = 0;

            int textW = 14;
            int textH = 5;

            x = (Width / 2) - (textW / 2);
            x = (Height / 2) - (textH / 2);

            for (var i = x; i < x + textW; i++)
            {
                Console.SetCursorPosition(i, y);
                var p1 = new Point(i, y, '#');
                var p2 = new Point(i, y + textH - 1, '#');

                Console.SetCursorPosition(p1.X, p1.Y);
                Console.Write(p1.Symbol);
                Console.SetCursorPosition(p2.X, p2.Y);
                Console.Write(p2.Symbol);
            }

            for (var i = y; i < y + textH; i++)
            {
                Console.SetCursorPosition(i, y);
                var p1 = new Point(x, i, '#');
                var p2 = new Point(x + textH - 1, i, '#');

                Console.SetCursorPosition(p1.X, p1.Y);
                Console.Write(p1.Symbol);
                Console.SetCursorPosition(p2.X, p2.Y);
                Console.Write(p2.Symbol);
            }

            for (var i = x + 1; i < x + textW - 1; i++)
            {
                Console.SetCursorPosition(i, y);
                var p1 = new Point(i, y, ' ');
                var p2 = new Point(i, y + textH - 1, ' ');

                Console.SetCursorPosition(p1.X, p1.Y);
                Console.Write(p1.Symbol);
                Console.SetCursorPosition(p2.X, p2.Y);
                Console.Write(p2.Symbol);
            }

            for (var i = y + 1; i < y + textH - 1; i++)
            {
                Console.SetCursorPosition(i, y);
                var p1 = new Point(x, i, ' ');
                var p2 = new Point(x + textH - 1, i, ' ');

                Console.SetCursorPosition(p1.X, p1.Y);
                Console.Write(p1.Symbol);
                Console.SetCursorPosition(p2.X, p2.Y);
                Console.Write(p2.Symbol);
            }

            Console.SetCursorPosition(x + 2, y + 2);
            Console.Write("GAME OVER!!!");
        }
    }
}
