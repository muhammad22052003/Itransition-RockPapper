using System.Security.Cryptography;
using System.Text;
using SHA3.Net;
using ConsoleTables;
using System.Data;

namespace Rock_PaperGame
{
    /// <summary>
    /// Структура ход Компютера
    /// </summary>
    public class ComputerStep
    {
        public ComputerStep(string str)
        {
            ElementStr = str;

            Random generator = new Random();
            byte[] randomNumberByte = BitConverter.GetBytes(generator.Next(int.MaxValue));

            using (Sha3 sha3 = SHA3.Net.Sha3.Sha3256())
            {
                byte[] hash = sha3.ComputeHash(randomNumberByte);
                HMACKey = BitConverter.ToString(hash).Replace("-", string.Empty);
            }

            HMAC = CreateHMAC(HMACKey, ElementStr);
        }

        public static string CreateHMAC(string key, string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            string hmac = string.Empty;

            using (HMACSHA256 hm = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hm.ComputeHash(textBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");
                hmac = hash;
            }

            return hmac;
        }

        /// <summary>
        /// Элемент игры
        /// </summary>
        public string ElementStr { get; set; }

        /// <summary>
        /// HMAC значение
        /// </summary>
        public string HMAC { get; set; }

        /// <summary>
        /// HMAC ключ
        /// </summary>
        public string HMACKey { get; set; }
    }

    /// <summary>
    /// Класс для контролирование игры
    /// </summary>
    public class GameController
    {
        public GameController(IEnumerable<string> args)
        {
            GameValues = args.ToList();

            if (!CheckArgumentsForValid(GameValues))
            {
                GameValid = false;
            }
            else { GameValid = true; }
        }

        /// <summary>
        /// Возвращает рандомный ход компютера создавая hmac
        /// </summary>
        /// <returns></returns>
        public ComputerStep GetComputerStep()
        {
            Random random = new Random();

            ComputerStep step = new ComputerStep(GameValues[random.Next(GameValues.Count - 1)]);

            return step;
        }

        /// <summary>
        /// Проверка валидность входных аргументов
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool CheckArgumentsForValid(List<string> args)
        {
            List<string> arguments = args.ToList();
            int argsLength = args.Count;

            if (argsLength >= 3 && argsLength % 2 == 1)
            {
                foreach (string arg in args)
                {
                    //  Проверка на дубликат
                    if (arguments.FindAll((x) => x.Equals(arg)).Count > 1)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает победителя из двух строки
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public string? GetWinner(string first, string second)
        {
            List<string>? firstWinners = GetWinners(first).ToList();

            if (!GameValues.Contains(second) || firstWinners == null) { return null; }

            if (firstWinners.Contains(second)) { return second; }

            return first;
        }

        /// <summary>
        /// Возвращает все побеждающие строки указанного строка
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private IEnumerable<string>? GetWinners(string element)
        {
            int elementIndex = GameValues.IndexOf(element);


            if (elementIndex == -1) { return null; }

            List<string> winners = new List<string>();
            int halfCount = (GameValues.Count - 1) / 2;

            for (int i = 0, j = 0; i < GameValues.Count; i++)
            {
                if (elementIndex - j <= 0)
                {
                    if ((GameValues.Count - Math.Abs(elementIndex - halfCount)) <= i && i > elementIndex)
                    {
                        winners.Add(GameValues[i]);
                        j++;
                    }
                }
                else if (i < elementIndex && i >= elementIndex - halfCount)
                {
                    winners.Add(GameValues[i]);
                    j++;
                }
            }

            return winners;
        }

        public List<string> GameValues { get; set; }
        public bool GameValid { get; set; }
    }

    /// <summary>
    /// Enum отвечаюший за состояние меню
    /// </summary>
    public enum InterfaceState
    {
        MAIN_MENU,
        OVER,
        HELP,
        ERROR,
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            List<string> arguments = args.ToList();

            if(arguments.Count >= 1) { arguments.RemoveAt(0); }

            GameController game = new GameController(arguments);

            if (!game.GameValid)
            {
                CustomInterface.WriteColoredText("Invalid Arguments\n", ConsoleColor.Red);
                return;
            }

            CustomInterface customInterface = new CustomInterface(game);

            bool continueGame = true;
            InterfaceState interfaceState = InterfaceState.MAIN_MENU;
            while (continueGame)
            {
                Console.Clear();

                switch (interfaceState)
                {
                    case InterfaceState.MAIN_MENU:
                        interfaceState = customInterface.ShowMainMenu();
                        break;
                    case InterfaceState.OVER:
                        continueGame = false;
                        break;
                    case InterfaceState.HELP:
                        interfaceState = customInterface.ShowHelpMenu();
                        break;
                    case InterfaceState.ERROR:
                        interfaceState = customInterface.ShowErrorMenu();
                        break;
                }
            }
        }
    }

    public class CustomInterface
    {
        public CustomInterface(GameController game)
        {
            Game = game;
        }

        public static void WriteColoredText(object text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        public InterfaceState ShowMainMenu()
        {
            InterfaceState interfaceState = InterfaceState.MAIN_MENU;
            var step = Game.GetComputerStep();

            WriteColoredText("\tRock-paper-scissors-lizard-Spock Game\n\n", ConsoleColor.Green);

            WriteColoredText($"HMAC value : {step.HMAC}\n", ConsoleColor.Magenta);
            WriteColoredText("Available moves : \n", ConsoleColor.Yellow);

            for (int i = 0; i < Game.GameValues.Count; i++)
            {
                WriteColoredText($"{i + 1}. {Game.GameValues[i]}\n", ConsoleColor.Blue);
            }

            WriteColoredText("0. Exit\n", ConsoleColor.Red);
            WriteColoredText("?. Help\n", ConsoleColor.Yellow);

            WriteColoredText("Enter Your Move : ", ConsoleColor.Yellow);
            string userMove = Console.ReadLine();

            if(userMove != "?" && !uint.TryParse(userMove, out uint move))
            {
                return InterfaceState.ERROR;
            }
            if(userMove == "0" || userMove == "?")
            {
                if (userMove == "0") { return InterfaceState.OVER; }
                else { return InterfaceState.HELP; }
            }

            int moveValue = int.Parse(userMove) - 1;
            if (moveValue >= Game.GameValues.Count) { return InterfaceState.ERROR; }

            string userMoveElement = Game.GameValues[moveValue];
            WriteColoredText($"Your Move : {userMoveElement}\n", ConsoleColor.Green);

            WriteColoredText($"Computer Move : {step.ElementStr}\n", ConsoleColor.Cyan);

            string winner = Game.GetWinner(userMoveElement, step.ElementStr);

            if(winner == userMoveElement)
            {
                if(userMoveElement == step.ElementStr)
                {
                    WriteColoredText($"Draw !\n", ConsoleColor.Yellow);
                }
                else
                {
                    WriteColoredText($"You Lose !\n", ConsoleColor.Red);
                }
            }
            else
            {
                WriteColoredText($"You Win !\n", ConsoleColor.Green);
            }

            WriteColoredText($"HMAC key : {step.HMACKey}\n", ConsoleColor.Magenta);

            WriteColoredText($"Press any key to restart", ConsoleColor.White);
            Console.ReadKey();

            return interfaceState;
        }

        public InterfaceState ShowHelpMenu()
        {
            WriteColoredText("Possible cases : \n\n", ConsoleColor.Yellow);

            DataTable datatable = new DataTable();
            string header = $"\\/ PC/User >";

            datatable.Columns.Add(header);

            for (int i = 0; i < Game.GameValues.Count; i++)
            {
                datatable.Columns.Add($"{Game.GameValues[i]}");
            }

            List<string> row = new List<string>();

            for (int i = 0; i < Game.GameValues.Count; i++)
            {
                DataRow workRow = datatable.NewRow();
                workRow[header] = Game.GameValues[i];
                for (int j = 0; j < Game.GameValues.Count; j++)
                {
                    string winner = Game.GetWinner(Game.GameValues[j], Game.GameValues[i]);

                    if(winner == Game.GameValues[i] && winner != Game.GameValues[j])
                    {
                        workRow[Game.GameValues[j]] = "User Win!";
                    }
                    else if(winner == Game.GameValues[j] && winner != Game.GameValues[i])
                    {
                        workRow[Game.GameValues[j]] = "User Lose!";
                    }
                    else
                    {
                        workRow[Game.GameValues[j]] = "Draw!";
                    }
                }
                datatable.Rows.Add(workRow);
            }

            // Создаем таблицу
            var table = ConsoleTable.From(datatable);

            // Выводим таблицу на консоль
            WriteColoredText(table + "\n\n",ConsoleColor.Green);

            WriteColoredText($"Press any key to restart", ConsoleColor.White);
            Console.ReadKey();

            return InterfaceState.MAIN_MENU;
        }

        public InterfaceState ShowErrorMenu()
        {
            WriteColoredText($"You have entered an incorrect input value\n\n", ConsoleColor.Red);
            WriteColoredText($"Press any key to restart", ConsoleColor.White);
            Console.ReadKey();
            return InterfaceState.MAIN_MENU;
        }

        GameController Game {  get; set; }
    }
}
