using BlazorApi.Data;
using BlazorApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MemesController : ControllerBase
{
    private readonly AppDBContext _context;

    public MemesController(AppDBContext context)
    {
        _context = context;
    }

    // /name/forspoken
    [HttpGet]
    [Route("name/{name}")]
    public IActionResult GetMemeByName(string name)
    {
        return Ok(new { message = "Under construction, here is your parameter: " + name });
    }

    // /download?name=forespoken
    [HttpGet]
    [Route("download")]
    public async Task<IActionResult> DownloadMemeByName(string? name)
    {
        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "No name provided." });

        Meme? meme = await _context.Memes.FirstOrDefaultAsync(m => m.Name == name);

        if (meme == null) return NotFound(new { message = "Meme not found." });

        return File(meme.FileData, meme.MimeType, meme.Name + meme.Extension);
    }


    [HttpPost]
    [Route("init")]
    public IActionResult Init()
    {
        var memesFolder = Path.Combine(Directory.GetCurrentDirectory(), "memes");
        if (Directory.Exists(memesFolder))
            return Ok(new { message = "Memes folder already existed." });

        Directory.CreateDirectory(memesFolder);
        return Ok(new { message = "Memes folder created." });
    }

    // /upload?name=forspoken
    [HttpPost]
    [Route("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, string? name)
    {
        if (file == null || file.Length < 1) return BadRequest(new { message = "No file uploaded." });

        if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "No name provided." });

        if (await _context.Memes.AnyAsync(m => m.Name == name))
            return BadRequest(new { message = "Meme name already exists." });

        

        Meme tempMeme = new Meme
        {
            Name = name,
            Extension = Path.GetExtension(file.FileName),
            MimeType = file.ContentType,
            FileData = GetFileBytes(file)
        };

        try
        {
            _context.Memes.Add(tempMeme);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "Something went wrong." });
        }

        return Ok(new
        {
            message = "Meme has been uploaded! ",
            meme = new
            {
                id = tempMeme.Id,
                name = tempMeme.Name,
                extension = tempMeme.Extension,
                mimeType = tempMeme.MimeType,
            }
        });
    }

    // /memes/delete?id=5
    [HttpDelete]
    [Route("delete")]
    // [Authorize(Roles = "Admin")]
    private async Task<IActionResult> DeleteMeme(int id)
    {
        Meme? meme = await _context.Memes.FirstOrDefaultAsync(m => m.Id == id);
        if (meme == null) return NotFound(new { message = "Meme not found or has already been deleted." });
        
        try
        {
            _context.Memes.Remove(meme);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = $"Something went wrong deleting meme ({id})." });
        }
        
        return Ok(new
        {
            message = "Meme has been deleted! ",
            meme = new
            {
                id = meme.Id,
                name = meme.Name,
                extension = meme.Extension,
                mimeType = meme.MimeType,
            }
        });
    }

    private byte[] GetFileBytes(IFormFile file)
    {
        using var ms = new MemoryStream();
        file.CopyTo(ms);
        return ms.ToArray();
    }

    private bool AuthenticateUser()
    {
        return true;
    }
}