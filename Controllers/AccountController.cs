using System.Text.RegularExpressions;
using Blog.Attributes;
using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Models.ViewModels;
using Blog.Services;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace Blog.Controllers;


[ApiController]
[Route("api/v1")]
public class AccountController : ControllerBase
{
    [HttpPost("accounts")]
    public async Task<IActionResult> Post(
        [FromBody] RegisterViewModel model,
        [FromServices] BlogDataContext context
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace("@", "-").Replace(".", "-")
        };

        var password = "senhanormal";
        user.PasswordHash = PasswordHasher.Hash(password);

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<dynamic>(new {user = user.Email, password}));
        } catch (DbUpdateException)
        {
            return StatusCode(400, new ResultViewModel<string>("05x99 - Este email ja esta cadastrado"));
        } catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna no servidor"));   
        }
    }

    [HttpPost("accounts/login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginViewModel model,
        [FromServices] TokenService tokenService,
        [FromServices] BlogDataContext context
    )
    {
        if(!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = await context
                        .Users
                        .AsNoTracking()
                        .Include(x => x.Roles)
                        .FirstOrDefaultAsync(x => x.Email == model.Email);
        
        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuario ou senha invalida"));
        
        if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
            return StatusCode(401, new ResultViewModel<string>("Usuario ou senha invalida"));

        try
        {
            var token = tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna do servidor"));
        }
    }

    [Authorize]
    [HttpPost("accounts/upload-image")]
    public async Task<IActionResult> UploadImage(
        [FromBody] UploadImageViewModel model,
        [FromServices] BlogDataContext context
    )
    {
        var filename = $"{Guid.NewGuid().ToString()}.jpg";
        var data = new Regex(@"^data:image\/[a-z]+;base64,")
                    .Replace(model.Base64Image, "");
        var bytes = Convert.FromBase64String(data);

        try
        {
            await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{filename}", bytes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna no servidor"));
        }

        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

        if (user is null)
            return NotFound(new ResultViewModel<User>("Usuario nao encontrado"));

        user.Image = $"https://localhost:0000/images/{filename}";

        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna no servidor"));
        }

        return Ok(new ResultViewModel<string>("Imagem alterada com sucesso", errors: null));
    }
}
