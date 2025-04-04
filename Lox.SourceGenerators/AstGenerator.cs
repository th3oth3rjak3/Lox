using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Lox.SourceGenerators;

[Generator]
public class AstGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddSource("Expression.cs", SourceText.From(GenerateAstFromClasses(), Encoding.UTF8));
        });
    }

    private readonly static string[] BaseClassNames = ["Expr", "Stmt"];

    private readonly static string[] Configuration =
    [
        "Expr : Binary : Expr? left, Token? token, Expr? right : A binary expression with two operands and an operator.",
        "Expr : Grouping : Expr? expression : An expression that is parenthesized.",
        "Expr : Literal : object? value : A literal like string, number, true, false, etc.",
        "Expr : Unary : Token? token, Expr? right : An expression that contains a single operator and a single operand.",
        "Expr : Variable : Token? name : A variable expression.",
        "Stmt : Expression : Expr? expr : An expression that is terminated as a statement.",
        "Stmt : Print : Expr? expression : A call to print the expression.",
        "Stmt : Var : Token? name, Expr? initializer : A variable declaration.",
    ];

    private readonly static List<ClassConfig> ClassesToGenerate = [.. Configuration.Select(ClassConfig.FromString)];

    private static string GenerateAstFromClasses()
    {

        var builder = new StringBuilder();

        builder.AppendLine("namespace Lox;");
        builder.AppendLine();

        var visitor = GenerateVisitorInterface(BaseClassNames.ToList());
        builder.Append(visitor);

        var baseClasses = GenerateAbstractBaseClasses(BaseClassNames);
        builder.Append(baseClasses);

        var classes = GenerateConcreteClasses();
        builder.Append(classes);

        return builder.ToString();
    }

    private static StringBuilder GenerateVisitorInterface(List<string> baseClasses)
    {
        var builder = new StringBuilder();

        foreach (var baseClass in baseClasses)
        {
            builder.AppendLine("/// <summary>");
            builder.AppendLine("/// A visitor pattern interface for generic types.");
            builder.AppendLine("/// </summary>");
            builder.AppendLine($"public interface I{baseClass}Visitor<T>");
            builder.AppendLine("{");
            var classes = ClassesToGenerate.Where(cls => cls.BaseName == baseClass).ToList();
            foreach (var cls in classes)
            {
                builder.AppendLine("    /// <summary>");
                builder.AppendLine($"    /// Visit the {cls.ClassName} {cls.BaseName} to perform an operation.");
                builder.AppendLine("    /// </summary>");
                builder.AppendLine($"    public T Visit{cls.ClassName}{cls.BaseName}({cls.ClassName} {cls.BaseName.ToLower()});");
                if (classes.IndexOf(cls) != classes.Count - 1)
                {
                    builder.AppendLine();
                }
            }
            builder.AppendLine("}");
            builder.AppendLine();
        }

        return builder;
    }

    private static StringBuilder GenerateAbstractBaseClasses(string[] baseClasses)
    {
        var builder = new StringBuilder();
        baseClasses
            .ToList()
            .ForEach(name =>
            {
                builder.AppendLine("/// <summary>");
                builder.AppendLine("/// An abstract implementation to be implemented by other classes.");
                builder.AppendLine("/// </summary>");
                builder.AppendLine($"public abstract class {name}");
                builder.AppendLine("{");
                builder.AppendLine("    /// <summary>");
                builder.AppendLine("    /// Accept the visitor to perform an operation.");
                builder.AppendLine("    /// </summary>");
                builder.AppendLine("""    /// <param name="visitor">The visitor that performs the operation.</param>""");
                builder.AppendLine("    /// <returns>The result of visiting the expression.</returns>");
                builder.AppendLine($"    public abstract R Accept<R>(I{name}Visitor<R> visitor);");
                builder.AppendLine("}");
                builder.AppendLine();
            });

        return builder;
    }

    private static StringBuilder GenerateConcreteClasses()
    {
        var builder = new StringBuilder();

        ClassesToGenerate
        .ForEach(cls =>
        {
            builder.AppendLine("/// <summary>");
            builder.AppendLine($"/// {cls.ClassComment}");
            builder.AppendLine("/// </summary>");
            builder.AppendLine($"public class {cls.ClassName}({FunctionParameter.ToString(cls.Parameters)}) : {cls.BaseName}");
            builder.AppendLine("{");

            cls.Parameters.ForEach(parameter =>
            {
                builder.AppendLine($"    public {parameter.TypeName} {parameter.Name.ToProperCase()} {{ get; set; }} = {parameter.Name};");
            });
            builder.AppendLine();
            builder.AppendLine("    /// <summary>");
            builder.AppendLine("    /// Accept the visitor to perform an operation.");
            builder.AppendLine("    /// </summary>");
            builder.AppendLine($"    public override R Accept<R>(I{cls.BaseName}Visitor<R> visitor)");
            builder.AppendLine("    {");
            builder.AppendLine($"        return visitor.Visit{cls.ClassName}{cls.BaseName}(this);");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            builder.AppendLine();
        });

        return builder;
    }
}