using System.Collections.Generic;
using System.Linq;

namespace Lox.SourceGenerators;

public class ClassConfig
{
    public string BaseName { get; private set; } = "";
    public string ClassName { get; private set; } = "";
    public string ClassComment { get; private set; } = "";
    public List<FunctionParameter> Parameters { get; private set; } = [];
    
    public static ClassConfig FromString(string input)
    {
        var parts = input.Split(':');
        var baseClassName = parts[0].Trim();
        var className = parts[1].Trim();
        var inputParams = FunctionParameter.FromString(parts.Skip(2).First());
        var classComment = parts.Skip(3).First().Trim();

        return new ClassConfig
        {
            BaseName = baseClassName,
            ClassName = className,
            ClassComment = classComment,
            Parameters = inputParams,
        };
    }
}