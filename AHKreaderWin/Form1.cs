using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace AHKreaderWin
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void InstructionsLabel_Click(object sender, EventArgs e)
        {

        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            Status.Text = "Processing...";
            ReadAHKFile();
        }

        private void ReadAHKFile()
        {
            string filepath = Directory.GetCurrentDirectory(); // This gets the current working directory.
            DirectoryInfo d = new DirectoryInfo(filepath); // This gets directory information so we can later use foreach on all .ahk files in the working directory.
            int lineCount = 0;

            TextFieldParser parser = new TextFieldParser("AutoHotKey.ahk");
            string[] fields;
            int TotalLinestoWrite = 100;
            string[] filtered = new string[TotalLinestoWrite];
            int CurrentFilterRow = 0;
            bool KeepNext = false;
            string[] fields2;
            int row = 0;
            int column = 0;

            foreach (var file in d.GetFiles("*.ahk"))
            {
                //Console.WriteLine(file.Name);
                lineCount = File.ReadAllLines(file.Name).Length;
                //Console.WriteLine("Processing " + lineCount + " lines.");
                parser = new TextFieldParser(file.Name);
                parser.HasFieldsEnclosedInQuotes = false;
                parser.SetDelimiters("fdhfuihfeoijafkdaj");
                while (!parser.EndOfData)
                {
                    fields = parser.ReadFields();
                    foreach (string field in fields)
                    {
                        if (field.StartsWith(";") && field.Length >= 4)
                        {
                            if (field[1] == '!')
                            {
                                if (field[2] == '@')
                                {
                                    if (field[3] == '#')
                                    {
                                        TotalLinestoWrite = TotalLinestoWrite + 2;
                                    }
                                }
                            }
                        }
                    }
                    row = row + 1;
                }
                parser.Close();

                row = 0;
                column = 0;
                parser = new TextFieldParser(file.Name);
                parser.HasFieldsEnclosedInQuotes = false;
                parser.SetDelimiters("fdhfuihfeoijafkdaj");
                while (!parser.EndOfData)
                {
                    fields2 = parser.ReadFields();
                    foreach (string field in fields2)
                    {
                        if (KeepNext) // This will be triggered when the line above it starts with ";!@#"
                        {
                            filtered[CurrentFilterRow] = HotKeyExecution(field);
                            CurrentFilterRow++;
                            KeepNext = false; // We want to set this back to default so we don't just grab every line
                        }
                        if (field.StartsWith(";") && field.Length >= 4)
                        {
                            if (field[1] == '!')
                            {
                                if (field[2] == '@')
                                {
                                    if (field[3] == '#')
                                    {
                                        filtered[CurrentFilterRow] = HotKeyDescription(field);
                                        CurrentFilterRow++;
                                        KeepNext = true; // This will make it so the next string is caught by the first if statement in this foreach statement
                                    }
                                }
                            }
                        }
                    }
                    row = row + 1;
                }
                parser.Close();
            }

            // Reorganize data into csv format for writing
            row = 0;
            column = 1;
            TotalLinestoWrite = TotalLinestoWrite / 2;
            string[,] data = new string[TotalLinestoWrite, 2];
            foreach (string field in filtered)
            {
                data[row, column] = field;
                switch (column)
                {
                    case 0: column = 1; row++;
                        break;
                    case 1: column = 0;
                        break;
                }
            }

            // Writing to file
            System.IO.File.WriteAllText("import.csv", string.Empty); // Before writing to the file, this empties the file. This way if there were previous contents with more lines than we are writing now, we will not have any of the old contents.
            try
            {
                var fs = File.Open("import.csv", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var sw = new StreamWriter(fs);
                column = 0;
                foreach (string field in data)
                {
                    if (column < 1)
                    {
                        sw.Write(field + ",");
                        column = column + 1;
                        //break;
                    }
                    else
                    {
                        sw.WriteLine(field);
                        column = 0;
                        //break;
                    }
                }
                sw.Flush();
                fs.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message); // There needs to be a way for errors to be caught and displayed
            }
            Status.Text = "Done";
            //Console.WriteLine("Done."); // There needs to be an indicator that the process has finished
        }

        static string HotKeyDescription(string input)
        {
            string Description;
            Description = input.Remove(0, 4); // This removes the first four characters, which we already know are ";!@#"
            if (Description[0] == ' ') // This checks to see if the next character after the above process is a space
            {
                Description = Description.Remove(0, 1); // If the next character was a space, this removes it
            }
            return Description; // Return the cleaned up description
        }

        static string HotKeyExecution(string input)
        {
            string KeyCommands = "";
            // Determine what kind of AHK command it is
            // Where does the AHK command end? Is it a normal command?
            if (DoesStringExist(input, "::")) // Does the AHK command end normally with a "::"?
            {
                input = input.Substring(0, WhereDoesItEnd(input)); // Cut the AHK command so it only contains the command and nothing after that

                //Convert it into English
                bool PreviousInput = false;
                int firstCharacter;
                while (input[0] == '<' || input[0] == '>' || input[0] == '^' || input[0] == '!' || input[0] == '+' || input[0] == '#')
                {
                    switch (input[0])
                    {
                        case '<':
                            KeyCommands = KeyCommands + "Left ";
                            break;
                        case '>':
                            KeyCommands = KeyCommands + "Right ";
                            break;
                        case '^':
                            KeyCommands = KeyCommands + "Ctrl ";
                            break;
                        case '!':
                            KeyCommands = KeyCommands + "Alt ";
                            break;
                        case '+':
                            KeyCommands = KeyCommands + "Shift ";
                            break;
                        case '#':
                            KeyCommands = KeyCommands + "Windows Key ";
                            break;
                    }
                    input = input.Remove(0, 1);
                    PreviousInput = true;
                }
                if (PreviousInput)
                { KeyCommands = KeyCommands + "+ " + input; }
                else if (DoesStringExist(input, ":*c1:")) // This section is for the auto-replace hot keys
                {
                    firstCharacter = input.IndexOf(":*c1:");
                    input = input.Remove(firstCharacter, (firstCharacter + 5)); // This removes the ":*c1:" string
                    if (DoesStringExist(input, "``"))
                    {
                        firstCharacter = input.IndexOf("``");
                        input = input.Substring(0, firstCharacter + 1); // This removes the duplicate '`' (assuming it is at the end of the string) character that is common in our auto-replace hot keys
                    }
                    KeyCommands = KeyCommands + input; // Adds the cleaned up command to the variable we return
                }
            }
            else
            { KeyCommands = "Unable to parse [" + input + "]"; }
            return KeyCommands; // Return the cleaned up description
        }

        static bool DoesStringExist(string input, string compare)
        {
            string searchWithinThis = input;
            int firstCharacter = input.IndexOf(compare);
            if (firstCharacter == -1)
            { return false; }
            else
            { return true; }
        }

        static int WhereDoesItEnd(string input)
        {
            string searchWithinThis = input;
            int firstCharacter = input.IndexOf("::");
            return firstCharacter;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

    }
}
