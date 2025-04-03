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

    private class ClassConfig
    {
        public string ClassName { get; set; } = "";
        public string ClassComment { get; set; } = "";
        public string Parameters { get; set; } = "";
    }

    private static ClassConfig GenerateConfigFromString(string input)
    {
        var parts = input.Split(':');
        var className = parts[0].Trim();
        var inputParams = parts.Skip(1).First().Trim();
        var classComment = parts.Skip(2).First().Trim();

        return new ClassConfig
        {
            ClassName = className,
            ClassComment = classComment,
            Parameters = inputParams,
        };
    }

    private static readonly string[] ClassConfiguration =
    [
        "Binary : Expression left, Token token, Expression right : A binary expression with two operands and an operator.",
        "Grouping : Expression expression : An expression that is parenthesized.",
        "Literal : Object? value : A literal like string, number, true, false, etc.",
        "Unary : Token token, Expression right : An expression that contains a single operator and a single operand."
    ];

    private static readonly List<ClassConfig> Classes = [.. ClassConfiguration.Select(GenerateConfigFromString)];

    private static string GenerateAstFromClasses()
    {
        var builder = new StringBuilder();

        builder.AppendLine("namespace Lox;");
        builder.AppendLine();

        var visitor = GenerateVisitorInterface();
        builder.Append(visitor);
        builder.AppendLine();

        var expressionBase = GenerateAbstractExpressionClass();
        builder.Append(expressionBase);
        builder.AppendLine();

        var concreteClasses = GenerateExpressionClasses();
        builder.Append(concreteClasses);
        builder.AppendLine();

        return builder.ToString();
    }

    private static StringBuilder GenerateVisitorInterface()
    {
        var builder = new StringBuilder();

        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// A visitor pattern interface for generic types.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public interface IVisitor<T>");
        builder.AppendLine("{");
        foreach (var cls in Classes)
        {
            builder.AppendLine($"    /// <summary>");
            builder.AppendLine($"    /// Visit the {cls.ClassName} Expression to perform an operation.");
            builder.AppendLine($"    /// </summary>");
            builder.AppendLine($"    public T Visit{cls.ClassName}Expression({cls.ClassName} expression);");
            builder.AppendLine();
        }
        builder.AppendLine("}");

        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <param name="thing"></param>
    private static StringBuilder GenerateAbstractExpressionClass()
    {
        var builder = new StringBuilder();

        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// An abstract syntax tree expression.");
        builder.AppendLine("/// </summary>");
        builder.AppendLine("public abstract class Expression");
        builder.AppendLine("{");
        builder.AppendLine("    /// <summary>");
        builder.AppendLine("    /// Accept the visitor to perform an operation.");
        builder.AppendLine("    /// </summary>");
        builder.AppendLine(@"    /// <param name=""visitor"">The visitor that performs the operation.</param>");
        builder.AppendLine(@"    /// <returns>The result of visiting the expression.</returns>");
        builder.AppendLine("    public abstract R Accept<R>(IVisitor<R> visitor);");
        builder.AppendLine("}");

        return builder;
    }

    private static StringBuilder GenerateExpressionClasses()
    {
        var builder = new StringBuilder();

        Classes
        .ForEach(cls =>
        {
            builder.AppendLine("/// <summary>");
            builder.AppendLine($"/// {cls.ClassComment}");
            builder.AppendLine("/// </summary>");
            builder.AppendLine($"public class {cls.ClassName}({cls.Parameters}) : Expression");
            builder.AppendLine("{");
            builder.AppendLine("    /// <summary>");
            builder.AppendLine("    /// Accept the visitor to perform an operation.");
            builder.AppendLine("    /// </summary>");
            builder.AppendLine($"    public override R Accept<R>(IVisitor<R> visitor)");
            builder.AppendLine("    {");
            builder.AppendLine($"        return visitor.Visit{cls.ClassName}Expression(this);");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            builder.AppendLine();

        });


        return builder;
    }
}