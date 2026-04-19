using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelSol.Filters
{
    public class RolAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var rol = context.HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rol))
            {
                context.Result = new RedirectToActionResult(
                    "Index",
                    "Login",
                    null);
                return;
            }

            if (rol != "ADMIN")
            {
                context.Result = new RedirectToActionResult(
                    "Index",
                    "Home",
                    null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}