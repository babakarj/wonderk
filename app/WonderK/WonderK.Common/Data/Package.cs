namespace WonderK.Common.Data
{
    public class Package
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public LinkedList<string> Departments { get; init; } = new();
        public LinkedList<string> Metadata { get; init; } = new();
        public Parcel Parcel { get; init; } = new();

        public Package()
        {
            // Default constructor
        }

        public Package(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON cannot be null or empty", nameof(json));

            var package = System.Text.Json.JsonSerializer.Deserialize<Package>(json);

            if (package == null)
                throw new InvalidOperationException($"Failed to deserialize JSON to Package. Json: {json}");

            Id = package.Id;
            Departments = package.Departments;
            Metadata = package.Metadata;
            Parcel = package.Parcel;
        }

        public Package(Parcel parcel, HashSet<string> departments)
        {
            Parcel = parcel ?? throw new ArgumentNullException(nameof(parcel));

            if (departments == null || departments.Count == 0)
                throw new ArgumentException("Departments cannot be null or empty", nameof(departments));

            foreach (var department in departments)
            {
                if (string.IsNullOrWhiteSpace(department))
                    throw new ArgumentException("Department name cannot be null or whitespace", nameof(departments));

                Departments.AddLast(department);
            }
        }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
