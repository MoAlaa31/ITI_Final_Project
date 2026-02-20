namespace ITI_Project.Api.DTO.Location
{
    public class GovernorateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<RegionDTO> Regions { get; set; }
    }
}
