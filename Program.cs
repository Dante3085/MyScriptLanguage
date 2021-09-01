using System;


namespace MyScriptLanguage
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser("Script.txt");
            parser.Parse();
            parser.RunParsedCode();

            Console.ReadLine();
        }
    }
}
