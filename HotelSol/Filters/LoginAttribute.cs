using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelSol.Filters
{
    public class LoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuario = context.HttpContext.Session.GetString("Usuario");

            if (string.IsNullOrEmpty(usuario))
            {
                context.Result = new RedirectToActionResult(
                    "Index",
                    "Login",
                    null
                );
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}