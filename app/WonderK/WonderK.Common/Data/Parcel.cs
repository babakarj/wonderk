namespace WonderK.Common.Data
{
    public record class Parcel
    {
        public Receipient Receipient { get; init; } = new();
        public double Weight { get; init; }
        public double Value { get; init; }
    }
}
