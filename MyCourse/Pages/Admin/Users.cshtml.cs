using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCourse.Models.Entities;
using MyCourse.Models.Enums;
using MyCourse.Models.InputModels.Users;

namespace MyCourse.Pages.Admin
{
     [Authorize(Roles =nameof(Role.Administrator))]
     public class UsersModel : PageModel
     {
          private readonly UserManager<ApplicationUser> _userManager;

          public UsersModel(UserManager<ApplicationUser> userManager)
          {
               this._userManager = userManager;
          }

          [BindProperty]
          public UserRoleInputModel Input { get; set; }
          public IList<ApplicationUser> Users { get; set; }//viene usata nella view come Model.Users

          [BindProperty(SupportsGet = true)]
          public Role InRole { get; set; }

          public async Task<IActionResult> OnGetAsync()
          {
               ViewData["Title"] = "Gestione utenti";
               Claim claim = new(ClaimTypes.Role, InRole.ToString());
                Users = await _userManager.GetUsersForClaimAsync(claim);
               return Page();
          }
          public async Task<IActionResult> OnPostAssignAsync()
          {
               //check if model state is ok
               if (!ModelState.IsValid)
               {
                    return await OnGetAsync();
               }
               ApplicationUser user = await _userManager.FindByEmailAsync(Input.Email);

               //check if user exists
               if (user == null)
               {
                    ModelState.AddModelError(nameof(Input.Email), $"L'indirizzo email {Input.Email} non corrisponde ad alcun utente");
                    return await OnGetAsync();
               }
               IList<Claim> claims = await _userManager.GetClaimsAsync(user);
               Claim roleClaim = new(ClaimTypes.Role, Input.Role.ToString());

               //check if the role is already assigned to the user
               if (claims.Any(claim => claim.Type == roleClaim.Type && claim.Value == roleClaim.Value))
               {
                    ModelState.AddModelError(nameof(Input.Role), $"Il ruolo {Input.Role} è già assegnato all'utente {Input.Email}");
                    return await OnGetAsync();
               }

               //Add new role into the user claim
               IdentityResult result = await _userManager.AddClaimAsync(user, roleClaim);
               if (!result.Succeeded)
               {
                    ModelState.AddModelError(string.Empty, $"L'operazione è fallita: {result.Errors.FirstOrDefault()?.Description}");
                    return await OnGetAsync();
               }
               TempData["ConfirmationMessage"] = $"Il ruolo {Input.Role} è stato assegnato all'utente {Input.Email}";
               return RedirectToPage(new { inrole = (int)InRole });
          }
          public async Task<IActionResult> OnPostRevokeAsync()
          {
               if (!ModelState.IsValid)
               {
                    return await OnGetAsync();
               }

               ApplicationUser user = await _userManager.FindByEmailAsync(Input.Email);
               if (user == null)
               {
                    ModelState.AddModelError(nameof(Input.Email), $"L'indirizzo email {Input.Email} non corrisponde ad alcun utente");
                    return await OnGetAsync();
               }

               IList<Claim> claims = await _userManager.GetClaimsAsync(user);

               Claim roleClaim = new(ClaimTypes.Role, Input.Role.ToString());
               if (!claims.Any(claim => claim.Type == roleClaim.Type && claim.Value == roleClaim.Value))
               {
                    ModelState.AddModelError(nameof(Input.Role), $"Il ruolo {Input.Role} non era assegnato all'utente {Input.Email}");
                    return await OnGetAsync();
               }

               IdentityResult result = await _userManager.RemoveClaimAsync(user, roleClaim);
               if (!result.Succeeded)
               {
                    ModelState.AddModelError(string.Empty, $"L'operazione è fallita: {result.Errors.FirstOrDefault()?.Description}");
                    return await OnGetAsync();
               }

               TempData["ConfirmationMessage"] = $"Il ruolo {Input.Role} è stato revocato all'utente {Input.Email}";
               return RedirectToPage(new { inrole = (int)InRole });
          }
     }
}