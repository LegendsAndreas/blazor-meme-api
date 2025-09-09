using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorApi.Data;
using BlazorApi.Models;
using BlazorApi.Services;

namespace BlazorApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;

        public UsersController(AppDBContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // Send it from the body as raw JSON variables in Postman.
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody]User.LogOnDto dto)
        {
            if (string.IsNullOrEmpty(dto.email)) return BadRequest("Email is not sent");
            if (string.IsNullOrEmpty(dto.password)) return BadRequest("Password is not sent");

            User? user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == dto.email);

            if (user == null) return BadRequest($"User not found with email '{dto.email}'");
            if (!BCrypt.Net.BCrypt.Verify(dto.password, user.HashedPassword))
                return BadRequest("Password is incorrect");
            
            TimeService timeHelper = new TimeService();
            user.LastLogin = timeHelper.GetCopenhagenUtcDateTime();
            await _context.SaveChangesAsync();

            string token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful!",
                token
            });
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<User>> PostUser([FromBody] User.RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("A user with this email already exists.");

            string salt = BCrypt.Net.BCrypt.GenerateSalt(16);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

            // Find standard User role
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Guest");
            if (userRole == null)
                return BadRequest("Default user role not found.");

            var timeService = new TimeService();
            DateTime utcNow = timeService.GetCopenhagenUtcDateTime();
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = dto.Email,
                HashedPassword = hashedPassword,
                Salt = salt,
                RoleId = userRole.Id,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created!", user.Email, role = userRole.Name });
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}