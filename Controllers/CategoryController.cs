using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Models.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;

namespace Blog.Controllers {

    [ApiController]
    [Route("api/v1")]
    public class CategoryController : ControllerBase
    {
        [HttpGet("categories")]
        public async Task<IActionResult> GetAsync(
            [FromServices] IMemoryCache cache,
            [FromServices] BlogDataContext context
        )
        {
            
            
            try
            {
                var categories = cache.GetOrCreate("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories(context);
                });

                return Ok(new ResultViewModel<List<Category>>(categories));
            } catch 
            {
                return StatusCode(
                    500,
                    new ResultViewModel<List<Category>>("05x04 - Falha interna no servidor"));
            }
        }

        private List<Category> GetCategories(BlogDataContext context)
        {
            return context.Categories.ToList();
        }

        [HttpGet("categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context
        )
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            
            if (category is null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            return StatusCode(200, new ResultViewModel<Category>(category));
        }

        [HttpPost("categories")]
        public async Task<IActionResult> PostAsync(
            [FromBody] EditorCategoryViewModel categoryViewModel,
            [FromServices] BlogDataContext context
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

            var category = new Category
            {
                Id = 0,
                Name = categoryViewModel.Name,
                Slug = categoryViewModel.Slug.ToLower()
            };

            try
            {
                if (categoryViewModel is null)
                    return BadRequest();

                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return Created(
                $"categories/{category.Id}",
                new ResultViewModel<Category>(category)
                );
        }

        [HttpPut("categories/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromRoute] int id,
            [FromBody] EditorCategoryViewModel categoryViewModel,
            [FromServices] BlogDataContext context
        )
        {
            var category = new Category
            {
                Id = 0,
                Name = categoryViewModel.Name,
                Slug = categoryViewModel.Slug.ToLower()
            };

            var model = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            
            if (model is null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            model.Name = category.Name;
            model.Slug = category.Slug;

            context.Categories.Update(model);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<Category>(model));
        }

        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context
        )
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            
            if (category is null)
                return NotFound();

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return NoContent();
        }


    }
}