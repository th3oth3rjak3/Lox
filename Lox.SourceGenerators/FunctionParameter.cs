using System.Collections.Generic;
using System.Linq;

namespace Lox.SourceGenerators;

public class FunctionParameter(string typeName, string name)
{
    public string TypeName => typeName;
    public string Name => name;

    public static List<FunctionParameter> FromString(string parameters) =>
        parameters
            .Trim()
            .Split(',')
            .Select(p => p.Trim())
            .Select(p =>
            {
                var parts = p.Split(' ');
                return new FunctionParameter(parts[0], parts[1]);
            })
            .ToList();
    
    public static string ToString(List<FunctionParameter> parameters) =>
        string.Join(", ", parameters.Select(p => $"{p.TypeName} {p.Name}"));
}