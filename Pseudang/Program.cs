namespace Pseudang
{
    internal class Program
    {
        static bool DEBUG = false;

        static void Main(string[] args)
        {
            if (!DEBUG)
            {
                if (args.Length >= 1)
                {
                    if (args[0].ToLower() == "--run")
                    {
                        string file = "";
                        try
                        {
                            file = args[1];
                        }
                        catch
                        {
                            Console.WriteLine("No pseudo file was provided!");
                        }

                        string fpath = Path.GetFullPath(file);

                        if (File.Exists(fpath))
                        {
                            if (fpath.EndsWith(".pseudo"))
                            {
                                var code = File.ReadAllText(fpath);
                                var scanner = new Scanner(code);
                                var tokens = scanner.Scan();
                                var interpreter = new Interpreter(tokens);
                                interpreter.Interpret();
                            }
                            else
                            {
                                Console.WriteLine("Provided file doesn't end with '.pseudo' and thus isn't a possible pseudang file!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Provided file was not found!");
                        }
                    }
                    else if (args[0].ToLower() == "-version")
                    {
                        Console.WriteLine("Pseudang v1.0.0");
                    }
                }
                else
                {
                    while (true)
                    {
                        Console.Write("pseudo> ");
                        var code = Console.ReadLine();
                        Console.WriteLine(Environment.NewLine);
                        var scanner = new Scanner(code);
                        var tokens = scanner.Scan();
                        var interpreter = new Interpreter(tokens);
                        interpreter.Interpret();
                    }
                }
            }
            else
            {
                var code = File.ReadAllText(@"C:\Users\PC USER\source\repos\Pseudang\Pseudang\code.pseudo");
                var scanner = new Scanner(code);
                var tokens = scanner.Scan();
                var interpreter = new Interpreter(tokens);
                interpreter.Interpret();
            }
        }
    }
}