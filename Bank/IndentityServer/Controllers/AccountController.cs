using Bank.DAL.Enums;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using IdentityServer.Dtos;
using IdentityServer.Dtos.ViewModels;
using IdentityServer.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityServer.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public AccountController(IUserService userService, IIdentityServerInteractionService interaction, IEventService events)
        {
            _userService = userService;
            _interaction = interaction;
            _events = events;
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Console.WriteLine($"Original ReturnUrl: {model.ReturnUrl}");

            // Декодируем URL
            string decodedReturnUrl = Uri.UnescapeDataString(model.ReturnUrl ?? "");
            Console.WriteLine($"Decoded ReturnUrl: {decodedReturnUrl}");

            var user = await _userService.Login(new LoginRequestDto
            {
                Email = model.Username,
                Password = model.Password
            });

            if (user == null)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "Invalid credentials"));
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "idsrv");
            var principal = new ClaimsPrincipal(identity);

            await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.Email, clientId: context?.Client.ClientId));
            await HttpContext.SignInAsync("idsrv", principal);

            Console.WriteLine($"ReturnUrl: {model.ReturnUrl}");
            Console.WriteLine($"DecodedReturnUrl: {decodedReturnUrl}");
            Console.WriteLine($"IsValidReturnUrl: {_interaction.IsValidReturnUrl(decodedReturnUrl)}");
            Console.WriteLine($"Context: {context != null}");

            if (context != null)
            {
                if (_interaction.IsValidReturnUrl(model.ReturnUrl))
                {
                    Console.WriteLine("IT WOOOOOOOOOOOOOOOOOORKS");
                    return Redirect(model.ReturnUrl);
                    // или для отладки:
                    //return Redirect("/connect/authorize/callback?client_id=ebankclient&redirect_uri=http://158.160.18.15:5173/signin-oidc&response_type=token%20id_token&scope=openid%20bankapi&state=8bdf208665ad4f0e91e9e0e7b320abee&nonce=db0ec0fecc954745974e9c48d3365a85");
                }
                throw new Exception("Invalid return URL");
            }

            Console.WriteLine("Fallback redirect triggered");
            return Redirect("http://158.160.18.15:5173/signin-oidc");
        }

        [HttpGet("register")]
        public IActionResult Register(string returnUrl)
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userService.RegisterUser(new UserRegisterDto {Email = model.Email, Password = model.Password, Role = UserRole.Employee, UserName = model.Username });

                return RedirectToAction("Login", new { returnUrl = model.ReturnUrl });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await HttpContext.SignOutAsync();
            var logoutRequest = await _interaction.GetLogoutContextAsync(logoutId);
            return Redirect(logoutRequest?.PostLogoutRedirectUri ?? "~/");
        }
    }
}
