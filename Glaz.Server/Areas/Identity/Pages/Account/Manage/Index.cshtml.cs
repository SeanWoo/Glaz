using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Glaz.Server.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Glaz.Server.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<GlazAccount> _userManager;
        private readonly SignInManager<GlazAccount> _signInManager;

        public IndexModel(
            UserManager<GlazAccount> userManager,
            SignInManager<GlazAccount> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Логин")]
            public string Username { get; set; }

            [DataType(DataType.EmailAddress)]
            [Display(Name = "Электронный адрес")]
            public string Email { get; set; }

            [Display(Name = "Имя")]
            public string FirstName { get; set; }

            [Display(Name = "Фамилия")]
            public string LastName { get; set; }
        }

        private async Task LoadAsync(GlazAccount user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var firstName = user.FirstName;
            var lastName = user.LastName;
            var email = await _userManager.GetEmailAsync(user);

            Input = new InputModel
            {
                Username = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            if (Input.Username != userName)
            {
                var setResult = await _userManager.SetUserNameAsync(user, Input.Username);
                if (!setResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set username.";
                    return RedirectToPage();
                }
            }
            if (Input.Email != email)
            {
                var setResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set email.";
                    return RedirectToPage();
                }
            }

            if (Input.FirstName != user.FirstName || Input.LastName != user.LastName)
            {
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                await _userManager.UpdateAsync(user);
            }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Ваш профиль был обновлён";
            return RedirectToPage();
        }
    }
}
