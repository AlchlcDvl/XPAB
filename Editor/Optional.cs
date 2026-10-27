namespace XPAB;

[Serializable]
internal struct Optional<T>
{
    public bool HasValue;
    public T Value;

    public Optional(T value)
    {
        HasValue = true;
        Value = value;
    }

    public Optional() => HasValue = false;

    public static implicit operator T?(Optional<T> value) => value.HasValue ? value.Value! : default;

    public static implicit operator Optional<T>(T? value) => value is null ? new() : new(value);
}
