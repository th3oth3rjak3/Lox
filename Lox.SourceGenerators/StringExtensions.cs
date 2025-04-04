namespace Lox.SourceGenerators;

public static class StringExtensions
{
    public static string ToProperCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        if (input.Length == 1) return input.ToUpper();
        return input[0].ToString().ToUpper() +  input.Substring(1);
    }
}