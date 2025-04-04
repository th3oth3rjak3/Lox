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
    
    private readonly static  string[] ClassConfiguration =
    [
        "Binary : Expression left, Token token, Expression right : A binary expression with two operands and an operator.",
        "Grouping : Expression expression : An expression that is parenthesized.",
        "Literal : object? value : A literal like string, number, true, false, etc.",
        "Unary : Token token, Expression right : An expression that contains a single operator and a single operand."
    ];

    private readonly static  List<ClassConfig> Classes = [.. ClassConfiguration.Select(ClassConfig.FromString)];

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
            builder.AppendLine("    /// <summary>");
            builder.AppendLine($"    /// Visit the {cls.ClassName} Expression to perform an operation.");
            builder.AppendLine("    /// </summary>");
            builder.AppendLine($"    public T Visit{cls.ClassName}Expression({cls.ClassName} expression);");
            builder.AppendLine();
        }
        builder.AppendLine("}");

        return builder;
    }
    
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
        builder.AppendLine("""    /// <param name="visitor">The visitor that performs the operation.</param>""");
        builder.AppendLine("    /// <returns>The result of visiting the expression.</returns>");
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
            builder.AppendLine($"public class {cls.ClassName}({FunctionParameter.ToString(cls.Parameters)}) : Expression");
            builder.AppendLine("{");
            
            cls.Parameters.ForEach(parameter =>
            {
                builder.AppendLine($"    public {parameter.TypeName} {parameter.Name.ToProperCase()} {{ get; set; }} = {parameter.Name};");
            });
            builder.AppendLine();
            builder.AppendLine("    /// <summary>");
            builder.AppendLine("    /// Accept the visitor to perform an operation.");
            builder.AppendLine("    /// </summary>");
            builder.AppendLine("    public override R Accept<R>(IVisitor<R> visitor)");
            builder.AppendLine("    {");
            builder.AppendLine($"        return visitor.Visit{cls.ClassName}Expression(this);");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            builder.AppendLine();

        });

        return builder;
    }
}