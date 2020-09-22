using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

/*
 * TO-DO:
 * 
 * nothing
 * 
 * 
 * */
namespace Snake
{
    public class Game
    {
        HighscoreHandler highscore = new HighscoreHandler();
        public List<Point> original_snake = new List<Point>();
        public Point HeadOfSnake;
        public int[,] GameBoard { get; set; }   //0... nichts; >5... points; -1... gegner
        public int Points { get; set; }
        public int Tick { get; set; }
        public int[] LinearEnemy { get; set; }
        /*
         INDEX:
        0 -> 0=not in use; 1=in use
        1 -> 0=horizontal; 1=vertikal
        2 -> X-Koordinate
        3 -> Y-Koordinate
        4 -> Aktiver Tick (1-17 is warning) (18-25 is attack)
         */
        public bool InGame { get; set; } //Ist der Spieler in einem Spiel?

        private EDirection currentDirection = EDirection.Right;
        private EDifficulty currentDifficulty = EDifficulty.Easy;

        public void StartMenu()
        {
            GameBoard = new int[34, 18]; //Größe des Spielfelds wird festgelegt [Breite, Höhe] !MIN_X_LENGTH=32!
            LinearEnemy = new int[5];
            LinearEnemy[0] = 0; //linear enemy is not in use
            Console.CursorVisible = false;
            Points = 0; //Startpunkte festlegen
            Tick = 0;

            while (true)
            {
                Console.Clear();
                DrawBorder();

                Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 5, (GameBoard.GetLength(1) / 2) - 5);
                Console.WriteLine("S N A K E");
                Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 7, (GameBoard.GetLength(1) / 2) -3);
                Console.WriteLine("(Highscore: " + highscore.GetHighScore()+")");
                Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 8, (GameBoard.GetLength(1) / 2));
                Console.WriteLine("by Marcel Fürpaß");
                Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 13, (GameBoard.GetLength(1) / 2) + 2);
                Console.WriteLine("PRESS [1] to start EASY-Mode");
                Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 13, (GameBoard.GetLength(1) / 2) + 3);
                Console.WriteLine("PRESS [2] to start MEDIUM-Mode");
                Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 13, (GameBoard.GetLength(1) / 2) + 4);
                Console.WriteLine("PRESS [3] to start EXTREME-Mode");
                Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 13, (GameBoard.GetLength(1) / 2) + 5);
                Console.WriteLine("PRESS [4] to start ORIGINAL-SNAKE");
               
                Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 13, (GameBoard.GetLength(1) / 2) + 6);
                Console.WriteLine("PRESS [x] to exit");


                ConsoleKey ck = Console.ReadKey(true).Key;

                switch (ck)
                {
                    case ConsoleKey.D1:
                        currentDifficulty = EDifficulty.Easy;
                        break;
                    case ConsoleKey.D2:
                        currentDifficulty = EDifficulty.Medium;
                        break;
                    case ConsoleKey.D3:
                        currentDifficulty = EDifficulty.Extreme;
                        break;
                    case ConsoleKey.D4:
                        currentDifficulty = EDifficulty.Original;
                        break;
                    case ConsoleKey.H:
                        highscore.SetHighScore(0);
                        continue;
                    case ConsoleKey.M:
                        PlaySong("mario");
                        break;
                    case ConsoleKey.X:
                        return;
                    default:
                        continue;
                }
                StartGame();
            }
        }
        public void StartGame()
        {
            Random rand = new Random();
            Console.Clear();
            InGame = true;
            ResetGameBoard();
            DrawBorder();
            currentDirection = EDirection.Right;
            UpdatePoints(0);

            HeadOfSnake.X = 3;
            HeadOfSnake.Y = 3;
            original_snake.Add(new Point(3, 3));

            SyncPlayer(HeadOfSnake.X, HeadOfSnake.Y);

            if (currentDifficulty == EDifficulty.Original)
            {
                GenerateRandomObject(1);      //startobejekt wird generiert
            }
            do
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey ck = Console.ReadKey(true).Key;

                    currentDirection = ChooseDirection(ck);
                }
                else
                {
                    MoveNext();

                    switch (currentDifficulty)
                    {
                        case EDifficulty.Easy:
                            if (rand.Next(13) == 0)
                                GenerateRandomObject(2); //standing enemy wird erstellt
                            break;
                        case EDifficulty.Medium:
                            if (rand.Next(10) == 0)
                                GenerateRandomObject(2);
                            break;
                        case EDifficulty.Extreme:
                            if (rand.Next(6) == 0)
                                GenerateRandomObject(2);
                            break;
                    }

                    if (rand.Next(12) == 0 && currentDifficulty != EDifficulty.Original)
                    {
                        GenerateRandomObject(1); //bonuspunkt wird erstellt
                    }



                }

                Tick++;

                if (currentDifficulty != EDifficulty.Original)
                {
                    if (Tick == 20 || Tick == 100 || Tick == 180 || Tick == 250 || Tick == 280 || Tick == 310 || Tick == 340 || Tick == 370 || Tick == 400 || (Tick > 400 && Tick % 40 == 0))
                    {
                        GenerateRandomObject(3); //generate linear enemy bei bestimmten ticks
                    }
                }

                if (LinearEnemy[0] == 1)
                {
                    HandleLinearEnemy();
                }

            } while (InGame);

        }
        private EDirection ChooseDirection(ConsoleKey ck)
        {
            if ((ck == ConsoleKey.W || ck == ConsoleKey.UpArrow) &&
                !(currentDifficulty == EDifficulty.Original && currentDirection == EDirection.Down))
            {
                return EDirection.Up;
            }
            else if ((ck == ConsoleKey.A || ck == ConsoleKey.LeftArrow) &&
                     !(currentDifficulty == EDifficulty.Original && currentDirection == EDirection.Right))
            {
                return EDirection.Left;
            }
            else if ((ck == ConsoleKey.S || ck == ConsoleKey.DownArrow) &&
                     !(currentDifficulty == EDifficulty.Original && currentDirection == EDirection.Up))
            {
                return EDirection.Down;
            }
            else if ((ck == ConsoleKey.D || ck == ConsoleKey.RightArrow) &&
                    !(currentDifficulty == EDifficulty.Original && currentDirection == EDirection.Left))
            {
                return EDirection.Right;
            }
            else
            {
                return currentDirection;
            }
        }
        private void PlaySong(string song)
        {
            if (song == "mario")
            {
                Console.Beep(659, 125); Console.Beep(659, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(167); Console.Beep(523, 125); Console.Beep(659, 125); Thread.Sleep(125); Console.Beep(784, 125); Thread.Sleep(375); Console.Beep(392, 125); Thread.Sleep(375); Console.Beep(523, 125); Thread.Sleep(250); Console.Beep(392, 125); Thread.Sleep(250); Console.Beep(330, 125); Thread.Sleep(250); Console.Beep(440, 125); Thread.Sleep(125); Console.Beep(494, 125); Thread.Sleep(125); Console.Beep(466, 125); Thread.Sleep(42); Console.Beep(440, 125); Thread.Sleep(125); Console.Beep(392, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(125); Console.Beep(784, 125); Thread.Sleep(125); Console.Beep(880, 125); Thread.Sleep(125); Console.Beep(698, 125); Console.Beep(784, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(125); Console.Beep(523, 125); Thread.Sleep(125); Console.Beep(587, 125); Console.Beep(494, 125); Thread.Sleep(125); Console.Beep(523, 125); Thread.Sleep(250); Console.Beep(392, 125); Thread.Sleep(250); Console.Beep(330, 125); Thread.Sleep(250); Console.Beep(440, 125); Thread.Sleep(125); Console.Beep(494, 125); Thread.Sleep(125); Console.Beep(466, 125); Thread.Sleep(42); Console.Beep(440, 125); Thread.Sleep(125); Console.Beep(392, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(125); Console.Beep(784, 125); Thread.Sleep(125); Console.Beep(880, 125); Thread.Sleep(125); Console.Beep(698, 125); Console.Beep(784, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(125); Console.Beep(523, 125); Thread.Sleep(125); Console.Beep(587, 125); Console.Beep(494, 125); Thread.Sleep(375); Console.Beep(784, 125); Console.Beep(740, 125); Console.Beep(698, 125); Thread.Sleep(42); Console.Beep(622, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(167); Console.Beep(415, 125); Console.Beep(440, 125); Console.Beep(523, 125); Thread.Sleep(125); Console.Beep(440, 125); Console.Beep(523, 125); Console.Beep(587, 125); Thread.Sleep(250); Console.Beep(784, 125); Console.Beep(740, 125); Console.Beep(698, 125); Thread.Sleep(42); Console.Beep(622, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(167); Console.Beep(698, 125); Thread.Sleep(125); Console.Beep(698, 125); Console.Beep(698, 125); Thread.Sleep(625); Console.Beep(784, 125); Console.Beep(740, 125); Console.Beep(698, 125); Thread.Sleep(42); Console.Beep(622, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(167); Console.Beep(415, 125); Console.Beep(440, 125); Console.Beep(523, 125); Thread.Sleep(125); Console.Beep(440, 125); Console.Beep(523, 125); Console.Beep(587, 125); Thread.Sleep(250); Console.Beep(622, 125); Thread.Sleep(250); Console.Beep(587, 125); Thread.Sleep(250); Console.Beep(523, 125); Thread.Sleep(1125); Console.Beep(784, 125); Console.Beep(740, 125); Console.Beep(698, 125); Thread.Sleep(42); Console.Beep(622, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(167); Console.Beep(415, 125); Console.Beep(440, 125); Console.Beep(523, 125); Thread.Sleep(125); Console.Beep(440, 125); Console.Beep(523, 125); Console.Beep(587, 125); Thread.Sleep(250); Console.Beep(784, 125); Console.Beep(740, 125); Console.Beep(698, 125); Thread.Sleep(42); Console.Beep(622, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(167); Console.Beep(698, 125); Thread.Sleep(125); Console.Beep(698, 125); Console.Beep(698, 125); Thread.Sleep(625); Console.Beep(784, 125); Console.Beep(740, 125); Console.Beep(698, 125); Thread.Sleep(42); Console.Beep(622, 125); Thread.Sleep(125); Console.Beep(659, 125); Thread.Sleep(167); Console.Beep(415, 125); Console.Beep(440, 125); Console.Beep(523, 125); Thread.Sleep(125); Console.Beep(440, 125); Console.Beep(523, 125); Console.Beep(587, 125); Thread.Sleep(250); Console.Beep(622, 125); Thread.Sleep(250); Console.Beep(587, 125); Thread.Sleep(250); Console.Beep(523, 125);
            }
        }
        private void MoveNext()
        {
            switch (currentDirection)
            {
                case EDirection.Up:
                    SyncPlayer(HeadOfSnake.X, HeadOfSnake.Y - 1);
                    break;
                case EDirection.Down:
                    SyncPlayer(HeadOfSnake.X, HeadOfSnake.Y + 1);
                    break;
                case EDirection.Left:
                    SyncPlayer(HeadOfSnake.X - 1, HeadOfSnake.Y);
                    break;
                case EDirection.Right:
                    SyncPlayer(HeadOfSnake.X + 1, HeadOfSnake.Y);
                    break;
            }
            Thread.Sleep(185);
        }
        private void DrawBorder()
        {
            //HORIZONTALE LINIEN
            Console.SetCursorPosition(0, 0);
            Console.Write("┌");
            for (int i = 1; i < GameBoard.GetLength(0); i++)
            {
                Console.SetCursorPosition(i, 0);
                Console.Write("─");
                Console.SetCursorPosition(i, GameBoard.GetLength(1));
                Console.Write("─");
            }
            Console.SetCursorPosition(GameBoard.GetLength(0), 0);
            Console.Write("┐");
            Console.SetCursorPosition(0, GameBoard.GetLength(1));
            Console.Write("└");

            //VERTIKALE LINIEN
            for (int i = 1; i < GameBoard.GetLength(1); i++)
            {
                Console.SetCursorPosition(GameBoard.GetLength(0), i);
                Console.Write("│");
                Console.SetCursorPosition(0, i);
                Console.Write("│");
            }
            Console.SetCursorPosition(GameBoard.GetLength(0), GameBoard.GetLength(1));
            Console.Write("┘");




        }
        private void ResetGameBoard()
        {
            for (int i = 0; i < GameBoard.GetLength(0); i++)
            {
                for (int j = 0; j < GameBoard.GetLength(1); j++)
                {
                    GameBoard[i, j] = 0;
                }
            }
        }

        private void SyncPlayer(int x, int y)
        {
            Console.SetCursorPosition(GameBoard.GetLength(0) + 2, 3);
            Console.WriteLine("TICK: " + Tick);

            if (x == GameBoard.GetLength(0) || (y == GameBoard.GetLength(1)) || x == 0 || y == 0)
            {
                //Außerhalb des Spielfeldes  
                EndGame();
            }
            else
            {
                if (GameBoard[x, y] >= 5) //wenn feld bonuspunkt ist
                {
                    UpdatePoints(GameBoard[x, y]);
                }
                else if (GameBoard[x, y] == -1) //wenn feld gegner ist
                {
                    EndGame();
                    return;
                }

                if (CheckSelfDeath(x, y)) return;

                DrawPlayer(x, y);
            }
        }

        private void DrawPlayer(int x, int y)
        {
            if (Points == 0 || currentDifficulty != EDifficulty.Original)
            {
                Point endOfSnake = original_snake[original_snake.Count - 1];

                Console.SetCursorPosition(endOfSnake.X, endOfSnake.Y);
                Console.WriteLine(" ");
                GameBoard[endOfSnake.X, endOfSnake.Y] = 0;
                original_snake.RemoveAt(original_snake.Count - 1);
            }
            else
            {
                UpdatePoints(Points * (-1));
                GenerateRandomObject(1);
            }

            original_snake.Insert(0, new Point(x, y));
            HeadOfSnake.X = x;
            HeadOfSnake.Y = y;
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("0");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void UpdatePoints(int points)
        {
            Points += points;

            Console.SetCursorPosition(GameBoard.GetLength(0) + 2, 1);
            if (currentDifficulty != EDifficulty.Original)
            {
                Console.WriteLine("SCORE: " + Points);
            }
            else
            {
                Console.WriteLine("SCORE: " + (original_snake.Count+1));
            }
           
        }
        private void GenerateRandomObject(int type)//1... point; 2...standing enemy; 3... linear enemy
        {
            Random rand = new Random();
            int xCoord;
            int yCoord;
            int pointValue;

            do
            {
                xCoord = rand.Next(1, GameBoard.GetLength(0));
                yCoord = rand.Next(1, GameBoard.GetLength(1));
            } while (xCoord != HeadOfSnake.X && yCoord != HeadOfSnake.Y && original_snake.Contains(new Point(xCoord, yCoord)));

            pointValue = rand.Next(5, 50);

            if (type == 1)
            {
                if (pointValue >= 5 && pointValue <= 10)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else if (pointValue >= 11 && pointValue <= 25)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else if (pointValue >= 26 && pointValue <= 35)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else if (pointValue >= 36 && pointValue <= 50)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }

                Console.SetCursorPosition(xCoord, yCoord);
                Console.Write("x");
                Console.ForegroundColor = ConsoleColor.White;

                if (GameBoard[xCoord, yCoord] != 1)
                {
                    GameBoard[xCoord, yCoord] = pointValue;
                }
            }
            else if (type == 2)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.SetCursorPosition(xCoord, yCoord);
                Console.Write("¤");
                Console.ForegroundColor = ConsoleColor.White;

                if (GameBoard[xCoord, yCoord] != 1)
                {
                    GameBoard[xCoord, yCoord] = -1;
                }
            }
            else if (type == 3)
            {
                LinearEnemy[0] = 1;

                if (rand.Next(1, 3) == 1)
                {
                    LinearEnemy[1] = 0;

                    LinearEnemy[2] = GameBoard.GetLength(0);
                    LinearEnemy[3] = yCoord;
                }
                else
                {
                    LinearEnemy[1] = 1;

                    LinearEnemy[2] = xCoord;
                    LinearEnemy[3] = GameBoard.GetLength(1);
                }

                LinearEnemy[4] = 0;

            }

        }
        private void HandleLinearEnemy()
        {
            Console.SetCursorPosition(LinearEnemy[2], LinearEnemy[3]);

            if (LinearEnemy[4] >= 0 && LinearEnemy[4] <= 17 && LinearEnemy[4] % 2 == 0)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[!]");
                LinearEnemy[4]++;
            }
            else if (LinearEnemy[4] >= 1 && LinearEnemy[4] <= 17 && LinearEnemy[4] % 2 == 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[!]");
                LinearEnemy[4]++;
            }
            else if (LinearEnemy[4] == 18)
            {
                if (LinearEnemy[1] == 0) //horizontal
                {
                    for (int i = LinearEnemy[2] - 1; i > 0; i--)
                    {
                        Console.SetCursorPosition(i, LinearEnemy[3]);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("=");

                        if (GameBoard[i, LinearEnemy[3]] == 1)
                        {
                            EndGame();
                            break;
                        }
                        GameBoard[i, LinearEnemy[3]] = -1; //feld wird auf gegnerfeld gesetzt
                    }
                }
                else if (LinearEnemy[1] == 1) //vertikal
                {
                    for (int i = LinearEnemy[3] - 1; i > 0; i--)
                    {
                        Console.SetCursorPosition(LinearEnemy[2] + 1, i);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("|");

                        if (GameBoard[LinearEnemy[2], i] == 1)
                        {
                            EndGame();
                            break;
                        }

                        GameBoard[LinearEnemy[2], i] = -1; //feld wird auf gegnerfeld gesetzt
                    }
                }

                LinearEnemy[4]++;
            }
            else if (LinearEnemy[4] >= 19 && LinearEnemy[4] <= 25)
            {
                LinearEnemy[4]++;
            }
            else if (LinearEnemy[4] == 26)
            {
                Console.SetCursorPosition(LinearEnemy[2], LinearEnemy[3]);
                Console.WriteLine("   ");

                if (LinearEnemy[1] == 0) //horizontal
                {

                    for (int i = LinearEnemy[2] - 1; i > 0; i--)
                    {
                        Console.SetCursorPosition(i, LinearEnemy[3]);
                        Console.Write(" ");

                        GameBoard[i, LinearEnemy[3]] = 0; //feld wird auf nichts gesetzt
                    }
                }
                else if (LinearEnemy[1] == 1) //vertikal
                {
                    for (int i = LinearEnemy[3] - 1; i > 0; i--)
                    {
                        Console.SetCursorPosition(LinearEnemy[2] + 1, i);
                        Console.Write(" ");

                        GameBoard[LinearEnemy[2], i] = 0; //feld wird auf nichts gesetzt
                    }
                }

                DrawBorder();
                LinearEnemy[0] = 0;
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
        private bool CheckSelfDeath(int x, int y)
        {
            if (original_snake.Count > 1 && original_snake.Contains(new Point(x, y)))
            {
                EndGame();
                return true;
            }
            return false;
        }
        private void EndGame()
        {
            Console.ForegroundColor = ConsoleColor.White;
            DrawBorder();

            original_snake.Clear();

            Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 8, (GameBoard.GetLength(1) / 2) - 2);
            Console.WriteLine("G A M E   O V E R");
            Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 3, (GameBoard.GetLength(1) / 2) + 1);
            Console.WriteLine("SCORE: " + Points);
            Console.SetCursorPosition((GameBoard.GetLength(0) / 2) - 5, (GameBoard.GetLength(1) / 2) + 4);
            Console.WriteLine("[PRESS KEY]");

            if (highscore.GetHighScore() < Points)
            {
                highscore.SetHighScore(Points);
            }

            InGame = false;
            Points = 0;
            Tick = 0;
            LinearEnemy[0] = 0;
            LinearEnemy[4] = 0;

            Console.ReadKey(true);

            Console.Clear();
        }
    }
}
