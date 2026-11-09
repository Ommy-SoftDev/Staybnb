using Staybnb.Models;

namespace Staybnb.Services
{
    public static class PropertyText
    {
        // "8 guests · 4 bedrooms · 5 beds · 4 baths"
        public static string Summary(HostProperty p)
        {
            var parts = new List<string>
            {
                $"{p.MaxGuests} {(p.MaxGuests == 1 ? "guest" : "guests")}"
            };

            parts.Add(p.Bedrooms > 0 ? $"{p.Bedrooms} {(p.Bedrooms == 1 ? "bedroom" : "bedrooms")}" : "Studio");

            if (p.Beds > 0) parts.Add($"{p.Beds} {(p.Beds == 1 ? "bed" : "beds")}");
            if (p.Bathrooms > 0) parts.Add($"{p.Bathrooms} {(p.Bathrooms == 1 ? "bath" : "baths")}");

            return string.Join(" \u00B7 ", parts);
        }
    }
}
