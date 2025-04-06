using System;

namespace Lox;

public class ReturnValue(object? value) : Exception
{
    public object? Value { get; set; } = value;
}
