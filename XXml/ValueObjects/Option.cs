using System.Runtime.CompilerServices;
using XXml.Internal;

namespace XXml.ValueObjects;

/// <summary>Тип, представляющий значение, которое может существовать.</summary>
/// <typeparam name="T">тип значения</typeparam>
public readonly struct Option<T> : IEquatable<Option<T>> where T : unmanaged, IReference
{
    private readonly T _v;

    /// <summary>Получение пустого экземпляра <see cref="Option{T}" />.</summary>
    /// .
    public static Option<T> Null => default;

    /// <summary>Создание экземпляра <see cref="Option{T}" />.</summary>
    /// <param name="v">оригинальное значение</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option(in T v)
    {
        _v = v;
    }

    /// <summary>Получает значение, если оно существует, или выбрасывает <see cref="InvalidOperationException" />.</summary>
    /// .
    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_v.IsNull) ThrowHelper.ThrowInvalidOperation("No value exist.");
            return _v;
        }
    }

    /// <summary>Получение информации о том, существует ли значение или нет.</summary>
    public bool HasValue => _v.IsNull == false;

    /// <summary>Попытка получить значение, если оно существует, или метод возвращает false.</summary>
    /// <param name="value">значение, если оно существует. (Не используйте его, если метод возвращает false.)</param>
    /// <returns>succeed or not</returns>
    /// .
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out T value)
    {
        value = _v;
        return _v.IsNull == false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _v.IsNull ? "null" : _v.ToString() ?? "";
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Option<T> option && Equals(option);
    }

    /// <summary>Возвращает значение, совпадает ли оно с указанным экземпляром.</summary>
    /// <param name="other">экземпляр для проверки</param>
    /// <returns>equal or not</returns>
    public bool Equals(Option<T> other)
    {
        return EqualityComparer<T>.Default.Equals(_v, other._v);
    }

    /// <summary>Возвращает значение, совпадает ли оно с указанным экземпляром.</summary>
    /// <param name="other">экземпляр для проверки</param>
    /// <returns>equal or not</returns>
    public bool Equals(in T other)
    {
        return EqualityComparer<T>.Default.Equals(_v, other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _v.GetHashCode();
    }

    /// <summary>Явная операция приведения <typeparamref name="T" /> к <see cref="Option{T}" /></summary>
    /// <param name="value">a value to cast</param>
    public static implicit operator Option<T>(in T value)
    {
        return new Option<T>(value);
    }

    public static bool operator ==(Option<T> left, Option<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Option<T> left, Option<T> right)
    {
        return !(left == right);
    }
}

public interface IReference
{
    public bool IsNull { get; }
}