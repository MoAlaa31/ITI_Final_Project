namespace ITI_Project.Api.DTO.Location
{
    public class NearbyProviderDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Profession { get; set; }
        public string? Experience { get; set; }
        public double Rating { get; set; }
        public double Distance { get; set; }
        public string Status { get; set; } = "متاح الآن";
        public PositionDTO Position { get; set; } = new();
        public string? Avatar { get; set; }
    }

    public class PositionDTO
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}