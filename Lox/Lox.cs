namespace Lox;

public static class Lox
{
    private readonly static Interpreter Interpreter = new();
    private static bool _hadError = false;
    private static bool _hadRuntimeError = false;

    public static void Main(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                Console.WriteLine("Usage: lox [script]");
                Environment.Exit(64);
                break;
            case 1:
                RunFile(args[0]);
                break;
            default:
                RunPrompt();
                break;
        }
    }

    private static void RunFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var source = System.Text.Encoding.UTF8.GetString(bytes);
        Run(source);

        if (_hadError) Environment.Exit(65);
        if (_hadRuntimeError) Environment.Exit(70);
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
            _hadError = false;
        }
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse();

        if (_hadError) return;
        Interpreter.Interpret(statements);
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    internal static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
        {
            Report(token.Line, "at end", message);
        }
        else
        {
            Report(token.Line, "at '" + token.Lexeme + "'", message);
        }
    }

    internal static void RuntimeError(RuntimeError error)
    {
        Console.Error.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
        _hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        _hadError = true;
    }
}