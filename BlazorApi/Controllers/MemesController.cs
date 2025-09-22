using System.Security.Claims;
using BlazorApi.Data;
using BlazorApi.Models;
using BlazorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Meme>> GetMeme(int id)
    {
        var meme = await _context.Memes.FirstOrDefaultAsync(m => m.Id == id);
        if (meme == null) return NotFound("Meme not found.");
        return Ok(meme);
    }

    [HttpGet]
    [Route("test")]
    public async Task<IActionResult> TestAsync()
    {
        return Ok("You can connect!");
    }

    [HttpGet]
    [Route("page/{pageNumber}")]
    public async Task<IActionResult> GetMemesByPage(int pageNumber)
    {
        const int pageSize = 20;
        if (pageNumber < 1)
            return BadRequest("Page number must be greater than 0.");

        try
        {
            var memes = await _context.Memes
                .OrderBy(m => m.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(memes);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Internal server error getting page memes: " + ex.Message);
        }
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

    [HttpGet]
    [Route("stats")]
    public async Task<IActionResult> GetMemeStats()
    {
        var memes = await _context.Memes.ToListAsync();
        var helper = new HelperService();

        try
        {
            MemesStatsDto memeStats = new MemesStatsDto
            {
                MemesCount = memes.Count,
                GifCount = memes.Count(m => m.Extension == ".gif"),
                JpgCount = memes.Count(m => m.Extension == ".jpg" || m.Extension == ".jpeg"),
                PngCount = memes.Count(m => m.Extension == ".png"),
                VideosCount = memes.Count(m => m.Extension == ".mp4" || m.Extension == ".webm"),
                WebpCount = memes.Count(m => m.Extension == ".webp"),
                ContributedUsers = helper.SetContributedUsers(memes)
            };
            return Ok(memeStats);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Internal server error getting memes stats: " + ex.Message);
        }
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Upload([FromForm] UploadMemeDto dto)
    {
        if (dto.File.Length < 1) return BadRequest(new { message = "No file uploaded." });

        if (string.IsNullOrEmpty(dto.Name)) return BadRequest(new { message = "No name provided." });

        if (await _context.Memes.AnyAsync(m => m.Name == dto.Name))
            return BadRequest(new { message = "Meme name already exists." });

        string? email = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email)) return BadRequest("No email in JWT");

        TimeService timeService = new TimeService();

        Meme newMeme = new Meme
        {
            Name = dto.Name,
            Extension = Path.GetExtension(dto.File.FileName),
            MimeType = dto.File.ContentType,
            AddedBy = email,
            FileData = GetFileBytes(dto.File),
            CreatedAt = timeService.GetCopenhagenUtcDateTime(),
            UpdatedAt = timeService.GetCopenhagenUtcDateTime()
        };

        try
        {
            _context.Memes.Add(newMeme);
            // We save early, to get the ID of the newly created meme. The ID will automatically be set in "tempMeme"
            await _context.SaveChangesAsync();

            // Adds all the tags, that are not null/empty, trims them and lowers them. Also keeps them distinct.
            var normalizedTags = dto.Tags?
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>();

            // Get all tags from db that match the normalized tags.
            var tagsInDb = await _context.Tags
                .Where(t => normalizedTags.Contains(t.Name.ToLower()))
                .ToListAsync();

            // Meant for adding the tags not found in the DB.
            var missingTagNames = normalizedTags
                .Except(tagsInDb.Select(t => t.Name.ToLower()))
                .ToList();

            // Creates new tags, based on the missing tags.
            var newTags = missingTagNames.Select(n =>
                new Tag
                {
                    Name = n,
                    AddedBy = email,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

            // Saves the newly (if any) created tags to the DB and adds tags to the tagsInDb for use in adding a relation
            // to the actual memes and tags.
            if (newTags.Any())
            {
                await _context.Tags.AddRangeAsync(newTags);
                await _context.SaveChangesAsync();
                tagsInDb.AddRange(newTags);
            }

            // Actually selects the tags from the DB
            var memesTags = tagsInDb.Select(tag => new MemesTags
            {
                MemeId = newMeme.Id,
                TagId = tag.Id,
                createdBy = email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            // Finally, adds relations of memes and tags to the DB.
            if (memesTags.Any())
            {
                await _context.MemesTags.AddRangeAsync(memesTags);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Something went wrong uploading the meme: " + ex.Message);
        }

        return Ok(new
        {
            message = "Meme has been uploaded! ",
            meme = new
            {
                id = newMeme.Id,
                name = newMeme.Name,
                extension = newMeme.Extension,
                mimeType = newMeme.MimeType,
            }
        });
    }

    // /memes/delete?id=5
    [HttpDelete]
    [Route("delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMeme(int id)
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
}