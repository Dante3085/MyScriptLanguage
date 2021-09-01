using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

/* TODO
 * 1. Schleife zum wiederholen von Anweisungen.
 * 2. Variablen expandieren, das heißt Namen mit Werten ersetzen bevor einzelne Befehle ausgewertet werden.
 *    (Vielleicht nicht so schlau für Zahlen??)
 * 
 */

/* DONE
 * 1. Beim entfernen von SingleLineComments NewLines nicht entfernen.
 * 2. Leerzeichen überall entfernen außer innerhalb String-Literals "Hallo Welt".
 * 3. Befehle in Warteschlange ordnen (FIFO)
 * 
 */


namespace MyScriptLanguage
{
    public class Parser
    {
        private String fileContents;
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private Queue<Action> executableActions = new Queue<Action>();

        public Parser(String pathToFile)
        {
            fileContents = File.ReadAllText(pathToFile);
        }

        public void Parse()
        {
            ParseFileContents(fileContents);
        }

        public void RunParsedCode()
        {
            foreach (Action action in executableActions)
                action.Invoke();
        }

        private void ParseFileContents(String fileContents)
        {
            fileContents = RemoveSingleLineComments(fileContents);
            fileContents = RemoveWhitespaceCharacters(fileContents);

            String[] commands = fileContents.Split(';');
            foreach (String command in commands)
            {
                ProcessCommand(command);
            }
        }

        private void ProcessCommand(String commandString)
        {
            if (commandString.Contains("print"))
                ProcessPrint(commandString);
            else if (commandString.Contains("=") || commandString.Contains(":"))
                ProcessVariable(commandString);
            else if (commandString.Contains("do"))
                ProcessLoop(commandString);
        }

        private void ProcessLoop(String loopString)
        {
            // do:10{print("Hallo");}
        }

        private void ProcessPrint(String printString)
        {
            String argument = GetStringInbetween(printString, "(", ")");

            if (!argument.Contains("\""))
            {
                CheckVariableExistence(argument);
                executableActions.Enqueue(() => Console.WriteLine(variables[argument]));
            }
            else
            {
                argument = argument.Replace("\"", String.Empty);
                executableActions.Enqueue(() => Console.WriteLine(argument));
            }
        }

        private void ProcessVariable(String variableString)
        {
            String[] typeNameAndValue = variableString.Split("=");
            String[] typeAndName = typeNameAndValue[0].Split(":");

            if (variables.ContainsKey(typeAndName[1]))
                throw new ArgumentException("Variable '" + typeAndName[1] + "' already exists.");

            Object newVariable = null;

            switch(typeAndName[0])
            {
                case "string":
                    {
                        newVariable = new String(GetStringInbetween(typeNameAndValue[1], "\"", "\""));
                        break;
                    }
                case "int":
                    {
                        newVariable = int.Parse(typeNameAndValue[1]);
                        break;
                    }
                case "bool":
                    {
                        newVariable = bool.Parse(typeNameAndValue[1]);
                        break;
                    }
                case "float":
                    {
                        newVariable = float.Parse(typeNameAndValue[1]);
                        break;
                    }
                case "double":
                    {
                        newVariable = double.Parse(typeNameAndValue[1]);
                        break;
                    }
            }

            variables.Add(typeAndName[1], newVariable);
        }

        private void CheckVariableExistence(String varName)
        {
            if (!variables.ContainsKey(varName))
                throw new ArgumentException("The variable '" + varName + "' does not exist.");
        }

        private String GetStringInbetween(String s, String left, String right)
        {
            int indexLeft = s.IndexOf(left);
            int indexRight = s.IndexOf(right, indexLeft + 1);
            return s.Substring(indexLeft + 1, indexRight - (indexLeft + 1));
        }

        private String RemoveWhitespaceCharacters(String s)
        {
            s = s.Replace("\n", String.Empty).Replace("\r", String.Empty);

            // Whitespace nur aueßerhalb von String-Literals entfernen.
            int indexNextOpenQuote = s.IndexOf("\"");
            int indexFrom = 0;
            while (indexNextOpenQuote != -1)
            {
                // Leerzeichen entfernen
                s = ReplaceInbetween(s, " ", String.Empty, indexFrom, indexNextOpenQuote);

                // Zum nächsten String-Literal
                indexFrom = s.IndexOf("\"", indexNextOpenQuote + 1) + 1;
                indexNextOpenQuote = s.IndexOf("\"", indexFrom);

                // Der Rest des Strings s hat kein String-Literal mehr.
                if (indexNextOpenQuote == -1)
                {
                    s = ReplaceInbetween(s, " ", String.Empty, indexFrom, s.Length);
                    break;
                }
            }

            return s;
        }

        /// <summary>
        /// Ersetzt in einem Substring von s, der durch die Indizes from und to angegeben wird, den String 
        /// oldString mit newString.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String ReplaceInbetween(String s, String oldString,String newString, int from, int to)
        {
            // Substring mit from und to bestimmen
            String substring = s.Substring(from, to - from);

            // Im Substring oldString mit newString ersetzen
            substring = substring.Replace(" ", String.Empty);

            // Substring wieder in s einfügen
            s = s.Remove(from, to - from);
            s = s.Insert(from, substring);

            return s;
        }

        private String RemoveSingleLineComments(String s)
        {
            // "//" erkannt => bis zu new line zeichen entfernen.
            int indexNextSingleLineComment = s.IndexOf("//");
            int indexNewLineAfterComment = s.IndexOf("\r\n");
            while (indexNextSingleLineComment != -1)
            {
                s = s.Remove(indexNextSingleLineComment, indexNewLineAfterComment - (indexNextSingleLineComment));
                
                indexNextSingleLineComment = s.IndexOf("//");
                if (indexNextSingleLineComment == -1)
                    break;
                indexNewLineAfterComment = s.IndexOf("\r\n", indexNextSingleLineComment);
            }

            return s;
        }
    }
}
