namespace WonderK.Common.Data
{
    public record class Address
    {
        public string Street { get; init; } = string.Empty;
        public int HouseNumber { get; init; }
        public string PostalCode { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
    }
}
