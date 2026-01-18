using Blog.Data;
using Blog.Models;
using Blog.Models.ViewModels;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers;

[ApiController]
[Route("api/v1")]
public class PostController : ControllerBase
{
    [HttpGet("posts")]
    public async Task<IActionResult> GetAsync(
        [FromServices] BlogDataContext context,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25
    )
    {
        try
        {
        var count = await context.Posts.AsNoTracking().CountAsync();
        var posts = await context.Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Select(x => new ListPostsViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                LastUpdateDate = x.LastUpdateDate,
                Author = $"{x.Author.Name} ({x.Author.Email})"
            })
            .Skip(page * pageSize)
            .Take(pageSize)
            .OrderByDescending(x => x.LastUpdateDate)
            .ToListAsync();

        
        return Ok(new ResultViewModel<dynamic>( new {
            total = count,
            posts,
            page,
            pageSize
            }, null));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<List<Post>>("05x04 - Falha interna no servidor"));
        }
    }

    [HttpGet("posts/{id:int}")]
    public async Task<IActionResult> DetailAsync(
        [FromRoute] int id,
        [FromServices] BlogDataContext context
    )
    {
        try
        {
            var post = await context.Posts
                                .AsNoTracking()
                                .Include(x => x.Author)
                                    .ThenInclude(x => x.Roles)
                                .Include(x => x.Category)
                                .FirstOrDefaultAsync(x => x.Id == id);

            if (post is null)
                return NotFound(new ResultViewModel<Post>("Conteudo nao encontrado"));
            
            return Ok(new ResultViewModel<Post>(post));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<Post>("Falha no servidor"));
        }
    }

    [HttpGet("posts/category/{category:string}")]
    public async Task<IActionResult> GetByCategoryAsync(
        [FromRoute] string category,
        [FromServices] BlogDataContext context,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 25
    )
    {
        try
        {
            var count = await context.Posts.AsNoTracking().CountAsync();
            var posts = await context.Posts
                                .AsNoTracking()
                                .Include(x => x.Author)
                                .Include(x => x.Category)
                                .Where(x => x.Category.Slug == category)
                                .Select(x => new ListPostsViewModel
                                {
                                    Id = x.Id,
                                    Title = x.Title,
                                    Slug = x.Slug,
                                    LastUpdateDate = x.LastUpdateDate,
                                    Category = x.Category.Name,
                                    Author = $"{x.Author.Name} ({x.Author.Email})"
                                })
                                .Skip(page * pageSize)
                                .Take(pageSize)
                                .OrderDescending()
                                .ToListAsync();

            return Ok(new ResultViewModel<dynamic>( new {
                total = count,
                posts,
                page,
                pageSize
                }, null));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<Post>("Erro no servidor"));
        }
        
    }
}