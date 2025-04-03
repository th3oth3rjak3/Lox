namespace Lox;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: lox [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            runFile(args[0]);
        }
        else
        {
            runPrompt();
        }
    }

    private static void runFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var source = System.Text.Encoding.UTF8.GetString(bytes);
        run(source);
    }

    private static void runPrompt()
    {

    }

    private static void run(string source)
    {
        // Scanner scanner = new Scanner(source);
        // List<Token> tokens = scanner.scanTokens();

        // foreach (var token in tokens)
        // {
        //      Console.WriteLine(token);
        // }
    }
}