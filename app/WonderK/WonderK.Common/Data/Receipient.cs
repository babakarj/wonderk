namespace WonderK.Common.Data
{
    public record class Receipient
    {
        public string Name { get; init; } = string.Empty;
        public Address Address { get; init; } = new();
    }
}
