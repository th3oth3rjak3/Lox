namespace Lox;

/// <summary>
/// A unit type is a type that allows only one value (and thus can hold no information)
/// </summary>
[Serializable]
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
{
    /// <summary>
    /// Provide a default value for unit.
    /// </summary>
    public readonly static Unit Default = new();

    /// <summary>
    /// Get the hash code for unit.
    /// </summary>
    /// <returns>The value '0' since all unit types are equivalent.</returns>
    public override int GetHashCode() =>
        0;

    /// <summary>
    /// Compare any object to the unit type.
    /// </summary>
    /// <param name="obj">The input object.</param>
    /// <returns>True when the object is a unit type.</returns>
    public override bool Equals(object? obj) =>
        obj is Unit;

    /// <summary>
    /// Convert a unit into its string representation.
    /// </summary>
    /// <returns>A string which represents an empty tuple.</returns>
    public override string ToString() =>
        "()";

    /// <summary>
    /// Perform equality comparison on unit.
    /// </summary>
    /// <param name="_">Ignored unit.</param>
    /// <returns>Always true since two units are equal.</returns>
    public bool Equals(Unit _) =>
        true;

    /// <summary>
    /// Perform equals comparison on unit.
    /// </summary>
    /// <param name="_">Ignored unit.</param>
    /// <param name="__">Ignored unit.</param>
    /// <returns>Always true since two units are equal.</returns>
    public static bool operator ==(Unit _, Unit __) =>
        true;

    /// <summary>
    /// Perform not equals comparison on unit.
    /// </summary>
    /// <param name="_">Ignored unit.</param>
    /// <param name="__">Ignored unit.</param>
    /// <returns>Always false since two units are equal.</returns>
    public static bool operator !=(Unit _, Unit __) =>
        false;

    /// <summary>
    /// Perform greater than comparison on unit.
    /// </summary>
    /// <param name="_">Ignored unit.</param>
    /// <param name="__">Ignored unit.</param>
    /// <returns>Always false since two units are equal.</returns>
    public static bool operator >(Unit _, Unit __) =>
        false;

    /// <summary>
    /// Perform greater than or equal to comparison on unit.
    /// </summary>
    /// <param name="_">Ignored unit.</param>
    /// <param name="__">Ignored unit.</param>
    /// <returns>Always true since two units are equal.</returns>
    public static bool operator >=(Unit _, Unit __) =>
        true;

    /// <summary>
    /// Perform less than comparison on unit.
    /// </summary>
    /// <param name="_">Ignored unit.</param>
    /// <param name="__">Ignored unit.</param>
    /// <returns>Always false since two units are equal.</returns>
    public static bool operator <(Unit _, Unit __) =>
        false;

    /// <summary>
    /// Perform less than or equal to comparison on unit.
    /// </summary>
    /// <param name="_">Ignored unit.</param>
    /// <param name="__">Ignored unit.</param>
    /// <returns>Always true since two units are equal.</returns>
    public static bool operator <=(Unit _, Unit __) =>
        true;

    /// <summary>
    /// Provide an alternative value to unit
    /// </summary>
    /// <typeparam name="T">Alternative value type</typeparam>
    /// <param name="anything">Alternative value</param>
    /// <returns>Alternative value</returns>
    public T Pipe<T>(T anything) => anything;

    /// <summary>
    /// Provide an alternative value to unit
    /// </summary>
    /// <typeparam name="T">Alternative value type</typeparam>
    /// <param name="anything">Alternative value</param>
    /// <returns>Alternative value</returns>
    public T Pipe<T>(Func<T> anything) => anything();

    /// <summary>
    /// Perform a comparison on two unit types which are always equal.
    /// </summary>
    /// <param name="other">The other unit to compare with.</param>
    /// <returns>0 because they are always equal.</returns>
    public int CompareTo(Unit other) =>
        0;

    /// <summary>
    /// Adding two units together produces unit.
    /// </summary>
    /// <param name="_">Unit which is ignored.</param>
    /// <param name="__">Unit which is ignored.</param>
    /// <returns>A new unit.</returns>
    public static Unit operator +(Unit _, Unit __) =>
        Default;
}