using Microsoft.AspNetCore.Identity;

namespace RecipeBook.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Recipe> Recipes { get; set; }
    }
}