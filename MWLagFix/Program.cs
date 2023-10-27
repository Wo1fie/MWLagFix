/*
    This file is part of MWLagFix.

    MWLagFix is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MWLagFix is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MWLagFix.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace MWLagFix
{
    class Program
    {
        static CancellationTokenSource Cancellation { get; set; }
        static bool Exit = false;
        static void Main()
        {
            Console.Title = "MWLagFix";
            Cancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = Cancellation.Token;

            Commands cmd = new Commands
            {
                { ConsoleKey.D1, async action => { await StartFix(FixTypes.Default, cancellationToken); } },
                { ConsoleKey.D2, async action => { await StartFix(FixTypes.Low, cancellationToken); } },
                { ConsoleKey.D3, async action => { await StartFix(FixTypes.Lowest, cancellationToken); } },
                { ConsoleKey.D4, async action => { await StartFix(FixTypes.MWDefault, cancellationToken); } },

                { ConsoleKey.G, action => { Process.Start("https://github.com/Wo1fie/MWLagFix"); } },
                { ConsoleKey.H, action => { PrintHelp(); } },
                { ConsoleKey.X, action => { Exit = true; } },
                { ConsoleKey.C, action => { Console.Clear(); } }
            };
            Console.CancelKeyPress += Console_CancelKeyPress;
            PrintGreeting();
            //Application Input Loop
            while (!Exit)
            {
                PrintMenu();
                ConsoleKey key = Console.ReadKey(true).Key;
                if (!cmd.Process(key))
                {
                    LogC(ConsoleColor.Red, "\nInvalid Option.");
                    LogC(ConsoleColor.DarkCyan, "Press H to open the Help menu.\n");
                }
                else if(cancellationToken.IsCancellationRequested) //Operation cancelled
                {
                    Log("Operation Cancelled.\n");
                    Cancellation = new CancellationTokenSource();
                    cancellationToken = Cancellation.Token;
                }
            }
            Log("Goodbye!");
        }
        static void PrintMenu()
        {
            LogC(ConsoleColor.Cyan, "Select an option:");
            Log("1. Default Priority (Recommended)");
            Log("2. Low Priority");
            Log("3. Lowest Priority");
            Log("4. Modern Warfare Default Settings (High Priority)");
            Log("H. Help");
            Log("C. Clear Console");
            Log("G. Open Github in Browser");
            Log();
            Log("X. Exit Program");
        }
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Log("Cancelling Operation");
            Cancellation.Cancel();
            e.Cancel = true;
        }
        static async Task StartFix(FixTypes fixType, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Log("\nThe lag fix has started!");
                Log("Looking for Modern Warfare.  Press Ctrl+C to cancel.");
                Process mw = WaitForMW(cancellationToken);
                if (mw is null || cancellationToken.IsCancellationRequested)
                    break;
                Log("Found Modern Warfare.  Applying Fix.");
                mw.PriorityClass = (ProcessPriorityClass)fixType;
                if (fixType == FixTypes.Lowest)
                    mw.ProcessorAffinity = (mw.ProcessorAffinity - 1); //Remove CPU 0 affinity
                Log("Fix applied.  Waiting for Modern Warfare to exit.  This will not use CPU or other resources.");
                await Task.Run(mw.WaitForExit, cancellationToken);
                Log("Modern Warfare has closed.");
            }
        }
        static Process WaitForMW(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Process mw = Process.GetProcessesByName("ModernWarfare").FirstOrDefault();
                if (mw != null)
                    return mw;
                Thread.Sleep(1000);
            }
            return null;
        }
        static void PrintGreeting()
        {
            Log("Thank you for using MWLagFix!");
            Log("For more information about this program visit https://github.com/Wo1fie/MWLagFix");
            Log($"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}\n");
        }
        static void PrintHelp()
        {
            Console.Clear();
            PrintGreeting();
            LogC(ConsoleColor.Cyan, "Description:");
            Log("You may have noticed Modern Warfare using 100% CPU frequently. " +
                "The reason for this is it sets its own Priority to 'High'. " +
                "By default, programs only run at 'Normal' Priority.  " +
                "This causes system lag and instability.\n");
            LogC(ConsoleColor.Cyan, "Commands List:");
            LogC(ConsoleColor.Yellow, "1. Default Priority (Recommended)");
            Log("Restores MW to the default system Priority of 'Normal'");
            LogC(ConsoleColor.Yellow, "2. Low Priority");
            Log("Sets MW Priority to 'Below Normal'");
            LogC(ConsoleColor.Yellow, "3. Lowest Priority");
            Log("Sets MW Priority to 'Below Normal' Priority and disables processor core 0.");
            LogC(ConsoleColor.Red, "Do not use this option if you have a single core processor. (Can MW even run on a single core?)");
            LogC(ConsoleColor.Yellow, "4. Modern Warfare Default Settings (High Priority)");
            Log("Sets MW Priority to its default, 'High' Priority");
            LogC(ConsoleColor.Yellow, "C. Clear Console");
            Log("Clears the console output.");
            LogC(ConsoleColor.Yellow, "Ctrl+C. Cancel Operation");
            Log("Cancels the currently running operation if there is one.\n");
            LogC(ConsoleColor.DarkCyan, "Note: If you use Ctrl+C in the menu you will need to press a key for it to cancel since there is no running operation besides key input.\n");
            Log("Press any key to exit Help Menu");
            Console.ReadKey(true);
        }
        static void LogC(ConsoleColor foreColor, string text)
        {
            Console.ForegroundColor = foreColor;
            Log(text);
            Console.ResetColor();
        }
        static void Log(string text = "")
        {
            Console.WriteLine(text);
        }
    }
}
