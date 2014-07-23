using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AHKreaderWin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        

    }


    /*static void Main()
    {
        //Command entry
        bool exit = false;
        ConsoleKeyInfo keypress; // ready the variable used for accepting user input

        Console.WriteLine("Instructions");
        Console.WriteLine("This program will take .ahk files in the local directory and write FunctionsList.csv in the local directory.");
        Console.WriteLine("Press \"C\" to compile and write the AHK command list.");
        Console.WriteLine("");
        Console.WriteLine("You can press Esc to exit.");
        while (!exit) // This is the main loop and will end when the user presses the Esc key.
        {
            keypress = Console.ReadKey(true); //This is listed with true so the key press is not shown.
            if (keypress.Key == ConsoleKey.C)
            {
                ReadAHKFile();
            }
            if (keypress.Key == ConsoleKey.Escape) // This checks to see if the key pressed was Esc
            { exit = true; } // This variable change will end the main loop and close the program.
        }
    }*/

    


}
