using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using WebApplication1.Data;

namespace WebApplication1.Middleware
{
    public class DisableUserMiddleware
    {
        private readonly RequestDelegate _next;

        public DisableUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null && user.IsDisabled)
                {
                    // Log out immediately
                    await signInManager.SignOutAsync();
                    
                    // Redirect to login page with a query parameter showing they are disabled
                    context.Response.Redirect("/Identity/Account/Login?disabled=true");
                    return;
                }
            }

            await _next(context);
        }
    }
}
