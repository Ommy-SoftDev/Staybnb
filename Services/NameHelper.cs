using Staybnb.Models;

namespace Staybnb.Services
{
    public static class NameHelper
    {
        // "First Last", or the email if the user has no name saved
        public static string Display(ApplicationUser? user)
        {
            if (user == null) return "Unknown user";

            var name = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrEmpty(name) ? (user.Email ?? "Unknown user") : name;
        }
    }
}
