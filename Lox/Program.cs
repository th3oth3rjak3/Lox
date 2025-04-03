namespace Lox;

public class Program
{
    private static bool hadError = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: lox [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var source = System.Text.Encoding.UTF8.GetString(bytes);
        Run(source);

        if (hadError)
        {
            Environment.Exit(65);
        }
    }

    private static void RunPrompt()
    {
        using var reader = new StreamReader(Console.OpenStandardInput());
        while (true)
        {
            Console.Write("> ");
            var line = reader.ReadLine();
            if (line == null) break;
            Run(line);
            hadError = false;
        }
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        hadError = true;
    }

    private static void Run(string source)
    {
        // Scanner scanner = new Scanner(source);
        // List<Token> tokens = scanner.scanTokens();

        // foreach (var token in tokens)
        // {
        //      Console.WriteLine(token);
        // }
    }
}