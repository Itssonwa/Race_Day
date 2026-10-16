using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RaceDay.Data;
using RaceDay.Models.Interface;  

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IProfileImageService _images;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IProfileImageService images)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _images = images;
    }

    public string? Username { get; set; }
    public string? CurrentImagePath { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(50), Display(Name = "First name")]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(50), Display(Name = "Last name")]
        public string LastName { get; set; } = "";

        public Gender Gender { get; set; }

        [Phone, Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Profile image")]
        public IFormFile? ProfileImage { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        Username = await _userManager.GetUserNameAsync(user);
        CurrentImagePath = user.ProfileImagePath;
        Input = new InputModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Gender = user.Gender,
            PhoneNumber = await _userManager.GetPhoneNumberAsync(user)
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("Unable to load user.");
        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("Unable to load user.");

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        user.FirstName = Input.FirstName;
        user.LastName = Input.LastName;
        user.Gender = Input.Gender;

        if (Input.PhoneNumber != await _userManager.GetPhoneNumberAsync(user))
            await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

        if (Input.ProfileImage != null)
        {
            var newPath = await _images.SaveAsync(Input.ProfileImage);
            if (newPath == null)
            {
                ModelState.AddModelError("Input.ProfileImage", "Image must be .jpg, .png or .webp and under 2 MB.");
                await LoadAsync(user);
                return Page();
            }
            _images.Delete(user.ProfileImagePath);
            user.ProfileImagePath = newPath;
        }

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }
}