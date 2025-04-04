using System.Collections.Generic;
using System.Linq;

namespace Lox.SourceGenerators;

public class ClassConfig
{
    public string ClassName { get; private set; } = "";
    public string ClassComment { get; private set; } = "";
    public List<FunctionParameter> Parameters { get; private set; } = [];
    
    public static ClassConfig FromString(string input)
    {
        var parts = input.Split(':');
        var className = parts[0].Trim();
        var inputParams = FunctionParameter.FromString(parts.Skip(1).First());
        var classComment = parts.Skip(2).First().Trim();

        return new ClassConfig
        {
            ClassName = className,
            ClassComment = classComment,
            Parameters = inputParams,
        };
    }
}