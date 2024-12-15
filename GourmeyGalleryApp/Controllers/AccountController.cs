using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using GourmeyGalleryApp.Models.DTOs;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Services.UserService.UserService;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using GourmeyGalleryApp.Models.DTOs.ApplicationUser;
using GourmeyGalleryApp.Services.EmailService;
using GourmeyGalleryApp.Services.RecipeService;
using Google.Apis.Auth;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly BlobStorageService _blobStorageService;
    private readonly IRecipeService _recipeService;
    private readonly RoleManager<IdentityRole> _roleManager; // Add this line
    private const string profilePictureUrl = "https://gourmetgallery01.blob.core.windows.net/gourmetgallery01/profile-circle.png";
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IUserService userService,
        IMapper mapper,
        IEmailService emailService,
        BlobStorageService blobStorageService,
        IRecipeService recipeService,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _userService = userService;
        _mapper = mapper;
        _emailService = emailService;
        _blobStorageService = blobStorageService;
        _recipeService = recipeService;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            UserName = registerDto.Email,
            Email = registerDto.Email,
            ProfilePictureUrl = registerDto.ProfilePictureUrl ?? profilePictureUrl,
            JoinedAt = DateTime.UtcNow 
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        string roleToAssign = "User"; 
        if (!await _roleManager.RoleExistsAsync(roleToAssign))
        {
            return BadRequest($"Role '{roleToAssign}' does not exist.");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors);
        }

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
            new { token, email = user.UserName }, Request.Scheme);

        // Send confirmation email
        var subject = "Confirm your email";
        var message = $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>link</a>";
        await _emailService.SendEmailAsync(user.Email, subject, message);

        return Ok(new AuthResult()
        {            
            Result = true,
            SuccessMessage = "Registration successful. Please confirm your email."
        });
       
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {

        var invalidResult = new AuthResult()
        {
            Result = false,
            Errors = { "Invalid login attempt." }
        };
        var user = await _userManager.FindByEmailAsync(loginDto.UserName);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            return Unauthorized(invalidResult);

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            invalidResult.Errors.Clear();
            invalidResult.Errors.Add("Email not confirmed.");
            return BadRequest(invalidResult);
        }

        if (user == null)
        {
            return Unauthorized(invalidResult);
        }

        var token = await GenerateJwtToken(user);
        return Ok(new AuthResult()
        {
            Token = token,
            Result = true
        });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtConfig").GetSection("Key").Value!);

        // Retrieve user roles
        var roles = await _userManager.GetRolesAsync(user); // Awaiting the task

        // Create claims including user roles
        List<Claim> claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.FamilyName, user.FirstName ?? ""),
        new Claim(JwtRegisteredClaimNames.GivenName, user.LastName ?? ""),
        new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? ""),
        new Claim(JwtRegisteredClaimNames.NameId, user.Id ?? ""),
        new Claim(JwtRegisteredClaimNames.UniqueName, user.ProfilePictureUrl ?? ""),
        new Claim(JwtRegisteredClaimNames.Aud, _configuration.GetSection("JwtConfig").GetSection("Audience").Value!),
        new Claim(JwtRegisteredClaimNames.Iss, _configuration.GetSection("JwtConfig").GetSection("Issuer").Value!)
    };

        // Add role claims
        foreach (var role in roles) // Iterate over the actual roles
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), // Set your desired expiration time
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string token, string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return BadRequest("Invalid email confirmation request.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return BadRequest("Email confirmation failed.");
        }

        return Ok("Email confirmed successfully.");
    }
    public class GoogleLoginRequest
    {
        public string IdToken { get; set; } // This matches the `idToken` field sent from the frontend.
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var payload = await VerifyGoogleToken(request.IdToken);

        if (payload == null)
        {
            return BadRequest("Invalid Google token.");
        }

        // Check if the user exists in the database
        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            // Create a new user if not found
            user = new ApplicationUser
            {
                UserName = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                Email = payload.Email,
                ProfilePictureUrl = profilePictureUrl,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "User creation failed.");
            }
        }

        string roleToAssign = "User";
        if (!await _roleManager.RoleExistsAsync(roleToAssign))
        {
            return BadRequest($"Role '{roleToAssign}' does not exist.");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors);
        }

        // Generate a JWT or login the user
        var token = GenerateJwtToken(user);

        return Ok(new AuthResult()
        {
            Token = token.Result,
            Result = true
        });
    }


    private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { "699433768038-tip0u2mr5q20vhkm41gjkk5cdk0j6hs2.apps.googleusercontent.com" } // Replace with your actual client ID
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch
        {
            return null;
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var invalidResult = new AuthResult()
        {
            Result = false,
            Errors = { }
        };

        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            invalidResult.Errors.Add("No user found with this email address.");
            return BadRequest(invalidResult);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Generate the URL for Angular app
        var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

        // Send the resetLink via email
        await SendResetPasswordEmail(user.Email, resetLink);
        var successMessage = new AuthResult { Result = true, SuccessMessage = "Password reset link has been sent to your email address." };

        return Ok(successMessage);
    }


    private async Task SendResetPasswordEmail(string email, string resetLink)
    {
          var subject = "Password Reset Request";
          var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f4f4;
                    color: #333;
                    margin: 0;
                    padding: 20px;
                }}
                .container {{
                    background: #ffffff;
                    border-radius: 8px;
                    max-width: 600px;
                    margin: 0 auto;
                    padding: 20px;
                    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    text-align: center;
                    margin-bottom: 20px;
                }}
                .header img {{
                    max-width: 150px;
                }}
                .content {{
                    margin-bottom: 20px;
                }}
                .footer {{
                    font-size: 0.875rem;
                    color: #888;
                    text-align: center;
                }}
                .btn {{
                    display: inline-block;
                    font-size: 1rem;
                    color: #ffffff;
                    background-color: #007bff;
                    padding: 12px 20px;
                    text-decoration: none;
                    border-radius: 4px;
                    margin: 10px 0;
                    text-align: center;
                }}
                .btn:hover {{
                    background-color: #0056b3;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <img src='https://www.shutterstock.com/image-vector/circle-line-simple-design-logo-600nw-2174926871.jpg' alt='Company Logo'>
                </div>
                <div class='content'>
                    <h2>Password Reset Request</h2>
                    <p>Hello,</p>
                    <p>We received a request to reset your password. Click the button below to reset it:</p>
                    <a href='{resetLink}' class='btn'>Reset Password</a>
                    <p>If you didn’t request this, please ignore this email.</p>
                </div>
                <div class='footer'>
                    <p>&copy; 2024 Gourmet Gallery. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>";

        await _emailService.SendEmailAsync(email, subject, body);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var invalidResult = new AuthResult()
        {
            Result = false,
            Errors = { "Token has expired" }
        };

        if (string.IsNullOrEmpty(resetPasswordDto.Token) || string.IsNullOrEmpty(resetPasswordDto.Email))
        {
            return BadRequest("Invalid request parameters.");
        }

        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            return BadRequest("Invalid request.");
        }

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(invalidResult);
        }

        return Ok("Password has been reset.");
    }

    [HttpPut("profile-picture")]
    public async Task<IActionResult> UpdateProfilePicture(IFormFile file)
    {        
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var userId = User.FindFirstValue("nameId");
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var profilePictureUrl = await _blobStorageService.UploadFile(file);
        user.ProfilePictureUrl = profilePictureUrl;

        await _userManager.UpdateAsync(user);

        return NoContent();
    }


    [HttpPut("remove-profile-picture")]
    public async Task<IActionResult> RemoveProfilePicture()
    {

        var userId = User.FindFirstValue("nameId");
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        user.ProfilePictureUrl = "https://gourmetgallery01.blob.core.windows.net/gourmetgallery01/profile-circle.png";

        await _userManager.UpdateAsync(user);

        return NoContent();
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue("nameId");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var userDto = _mapper.Map<ApplicationUserDto>(user);
        return Ok(userDto);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(ApplicationUserDto userDto)
    {
        var userId = User.FindFirstValue("nameId");
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        user.ProfilePictureUrl = userDto.ProfilePictureUrl;

        await _userService.UpdateUserAsync(user);

        return NoContent();
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var friends = await _userService.GetFriendsAsync(userId);

        if (friends == null)
        {
            return NotFound();
        }

        var friendDtos = _mapper.Map<IEnumerable<ApplicationUserDto>>(friends);
        return Ok(friendDtos);
    }

    [HttpGet("user-recipes")]
    public async Task<IActionResult> UserRecipes()
    {
        var userId = User.FindFirstValue("nameId");

        var recipes = await _recipeService.GetRecipesByUserIdAsync(userId);

        return Ok(recipes); 
    }

    [HttpPost("add-friend/{friendId}")]
    public async Task<IActionResult> AddFriend(string friendId)
    {
        var userId = User.FindFirstValue("nameId");

        await _userService.AddFriendAsync(userId, friendId);

        return Ok();
    }

    [HttpPost("accept-friend/{friendId}")]
    public async Task<IActionResult> AcceptFriend(string friendId)
    {
        var userId = User.FindFirstValue("nameId");

        await _userService.AcceptFriendAsync(userId, friendId);

        return Ok();
    }

    [HttpPost("add-favorite/{recipeId}")]
    public async Task<IActionResult> AddFavorite(int recipeId)
    {
        var userId = User.FindFirstValue("nameId");
        await _userService.AddToFavoritesAsync(userId, recipeId);
        return Ok();
    }

    [HttpPost("remove-favorite/{recipeId}")]
    public async Task<IActionResult> RemoveFavorite(int recipeId)
    {
        var userId = User.FindFirstValue("nameId");
        await _userService.RemoveFromFavoritesAsync(userId, recipeId);
        return Ok();
    }

    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = User.FindFirstValue("nameId");
        var favorites = await _userService.GetFavoriteRecipesAsync(userId);
        return Ok(favorites);
    }
}

