using Microsoft.AspNetCore.Mvc;
using RecipeBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;

public class RecipesController : Controller
{
    private readonly RecipeBookContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public RecipesController(UserManager<ApplicationUser> userManager, RecipeBookContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public ActionResult Index()
    {
        List<Recipe> model = _context.Recipes.ToList();
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new RecipeViewModel());
    }

    [HttpPost]
    public async Task<ActionResult> Create(RecipeViewModel model)
    {
        if (ModelState.IsValid)
        {
            Recipe recipe = new Recipe
            {
                Name = model.RecipeName,
                Instructions = model.Instructions,
                Description = model.Description,
                ImageUrl = model.ImageUrl
            };

            foreach (var ingredientViewModel in model.Ingredients)
            {
                string ingredientName = ingredientViewModel.Name.Trim();
                Ingredient existingIngredient = _context.Ingredients.FirstOrDefault(i => i.Name == ingredientName);
                Ingredient ingredient;
                if (existingIngredient != null)
                {
                    ingredient = existingIngredient;
                }
                else
                {
                    ingredient = new Ingredient { Name = ingredientName };
                    _context.Ingredients.Add(ingredient);
                }
                IngredientRecipe ingredientRecipe = new IngredientRecipe { Ingredient = ingredient, Recipe = recipe, Quantity = ingredientViewModel.Quantity };
                recipe.IRJoin.Add(ingredientRecipe);
                _context.IngredientRecipes.Add(ingredientRecipe);
            }

            foreach (var tagViewModel in model.Tags)
            {
                string tagName = tagViewModel.Name.Trim(); // Trim to handle leading/trailing whitespaces

                // Check if the tag with the same name already exists
                Tag existingTag = _context.Tags.FirstOrDefault(t => t.Name == tagName);

                Tag tag;

                if (existingTag != null)
                {
                    // If the tag exists, reuse it
                    tag = existingTag;
                }
                else
                {
                    // If the tag doesn't exist, create a new one
                    tag = new Tag { Name = tagName };
                    _context.Tags.Add(tag);
                }

                RecipeTag recipeTag = new RecipeTag { Recipe = recipe, Tag = tag };
                recipe.RTJoin.Add(recipeTag);
                _context.RecipeTags.Add(recipeTag);
            }
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);
            recipe.User = currentUser;
            _context.Recipes.Add(recipe);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(model);
    }

    public ActionResult Details(int id)
    {
        Recipe rec = _context.Recipes
            .Include(r => r.IRJoin)
            .ThenInclude(join => join.Ingredient)
            .Include(rec => rec.RTJoin)
            .ThenInclude(join => join.Tag)
            .FirstOrDefault(r => r.RecipeId == id);
        return View(rec);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        Recipe recipe = _context.Recipes
            .Include(r => r.IRJoin)
            .ThenInclude(join => join.Ingredient)
            .Include(r => r.RTJoin)
            .ThenInclude(join => join.Tag)
            .FirstOrDefault(r => r.RecipeId == id);

        if (recipe == null)
        {
            return NotFound();
        }

        return View(recipe);
    }

    [HttpPost]
    public IActionResult Edit(int id, Recipe model)
    {
        if (ModelState.IsValid)
        {
            // Update the recipe properties
            _context.Entry(model).State = EntityState.Modified;

            // Update or add ingredients
            foreach (var join in model.IRJoin)
            {
                if (join.Ingredient != null)
                {
                    _context.Entry(join.Ingredient).State = EntityState.Modified;
                }
                else
                {
                    _context.Ingredients.Add(join.Ingredient);
                }

                _context.Entry(join).State = EntityState.Modified;
            }

            // Update or add tags
            foreach (var join in model.RTJoin)
            {
                if (join.Tag != null)
                {
                    _context.Entry(join.Tag).State = EntityState.Modified;
                }
                else
                {
                    _context.Tags.Add(join.Tag);
                }

                _context.Entry(join).State = EntityState.Modified;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        Recipe recipe = _context.Recipes.Find(id);
        if (recipe == null)
        {
            return NotFound();
        }

        return View(recipe);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        Recipe recipe = _context.Recipes.Find(id);
        if (recipe == null)
        {
            return NotFound();
        }

        _context.Recipes.Remove(recipe);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

}