namespace Staybnb.Models
{
    public class PropertyImage
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public HostProperty Property { get; set; } = null!;
        public string ImageUrl { get; set; } = string.Empty;
    }
}