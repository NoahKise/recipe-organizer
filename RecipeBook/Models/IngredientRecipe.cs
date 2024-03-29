namespace RecipeBook.Models;

public class IngredientRecipe
{
    public int IngredientRecipeId { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }
    public string Quantity { get; set; }
}