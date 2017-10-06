using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Main
{
    public class Methods
    {
        public static Random Rnd { get; set; }

        static Methods()
        {
            Rnd = new Random();
        }

        public static string ManyNames(List<string> list)
        {
            string manyNames = string.Join(", ", list.GetRange(0, list.Count - 1));
            return manyNames;
        }

        public static int AddKey<K, V>(Dictionary<K, V> dict)
        {
            return dict.Count + 1;
        }

        public static bool TileExists(int coordX, int coordY, List<List<Tile>> grid)
        {
            if (FindTileBool(coordX, coordY, grid)) { return true; } else { return false; }
        }

        public static bool FindTileBool(int coordX, int coordY, List<List<Tile>> grid)
        {
            if (coordX < 0 || coordY < 0) { return false; }
            try
            {
                Tile foundTile = grid[coordX - 1][coordY - 1];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Tile FindTile(int coordX, int coordY, List<List<Tile>> grid)
        {
            if (coordX < 0 || coordY < 0) { return null; }
            try
            {
                Tile foundTile = grid[coordX - 1][coordY - 1];
                return foundTile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public class Tile
        {
            public int CoordX { get; set; }
            public int CoordY { get; set; }
            public string Description { get; set; }
            public List<GameItems.Cover> Cover { get; set; }

            public Tile(int coordX, int coordY, string desc, List<GameItems.Cover> cover)
            {
                CoordX = coordX;
                CoordY = coordY;
                Description = desc;
                Cover = cover;
            }
        }

        public static List<List<Tile>> MakeGrid(Tuple<int, int> gridSize,
            Dictionary<int, string> desc, Dictionary<int, List<GameItems.Cover>> cover)
        {
            List<List<Tile>> Grid = new List<List<Tile>>();
            int TileNum = 1;

            for (int X = 1; X <= gridSize.Item1; X++)
            {
                List<Tile> Row = new List<Tile>();

                for (int Y = 1; Y <= gridSize.Item2; Y++)
                {
                    Tile newTile = new Tile(X, Y, desc[TileNum], cover[TileNum]);
                    Row.Add(newTile);
                    TileNum++;
                }

                Grid.Add(Row);
            }

            return Grid;
        }

        public static int GetPlayerChoice(int numberOfOptions)
        {
            while (true)
            {
                Console.Write(" choose a number: ");
                try
                {
                    int choice = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine();

                    if (choice > 0 && choice <= numberOfOptions)
                    {
                        return choice;
                    }
                    else
                    {
                        Console.WriteLine("(number needs to be one of selection)");
                        Console.WriteLine();
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine();
                    continue;
                }
            }
        }

        public static void PrintOptions(Dictionary<int, string> options)
        {
            foreach (KeyValuePair<int, string> entry in options)
            {
                Console.WriteLine(" " + entry.Key + " - " + entry.Value);
            }
            Console.WriteLine();
        }

        public static int RollStat(int stat, string skill = " ")
        {
            int roll = RollD(20);
            roll += StatMod(stat);

            if (!skill.Equals(" "))
            {
                Console.WriteLine(skill + ": " + roll);
                Pause();
                Pause();
                Console.WriteLine();
            }

            return roll;
        }

        public static int StatMod(int stat)
        {
            stat = (stat - 10) / 2;
            return stat;
        }

        public static int RollD(int dieType, int numOfDice = 1)
        {
            int roll = 0;

            for (int i = 0; i < numOfDice; i++)
            {
                roll += Rnd.Next(1, dieType + 1);
            }

            return roll;
        }

        public static void Enter()
        {
            Console.WriteLine("Press \"Enter\"");
            Console.ReadLine();
        }

        public static void Pause()
        {
            Thread.Sleep(200);
        }

        public static void Typewriter(string text, string colour = "none")
        {
            int typeSpeed = 5;          // 50
            int SentencePause = 5;    // 600
            int characterLimit = 80;
            int currentLineCount = 0;

            switch (colour.ToLower())
            {
                case "red":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "cyan":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case "grey":
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                default:
                    break;
            }

            foreach (char letter in text)
            {
                if (currentLineCount <= characterLimit)
                {
                    CharacterParse(letter);
                    currentLineCount++;
                }
                else
                {
                    if (letter.Equals(' '))
                    {
                        Console.WriteLine();
                        currentLineCount = 0;
                    }
                    else
                    {
                        CharacterParse(letter);
                    }
                }
            }

            void CharacterParse(char letter)
            {
                if (letter.Equals(','))
                {
                    Console.Write(letter);
                    Pause();
                }
                else if (letter.Equals('.') || letter.Equals(';') || letter.Equals('!') || letter.Equals('?'))
                {
                    Console.Write(letter);
                    Thread.Sleep(SentencePause);
                }
                else
                {
                    Console.Write(letter);
                    Thread.Sleep(typeSpeed);
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void Print(string text, int figure)
        {
            Console.WriteLine(text + ": " + figure);
            Console.WriteLine();
            Pause();
            Pause();
            Pause();
        }
    }
}
