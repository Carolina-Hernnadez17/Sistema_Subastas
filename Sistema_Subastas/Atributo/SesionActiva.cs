using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Sistema_Subastas.Atributo
{
    public class SesionActiva : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var usuarioId = session.GetInt32("id_usuario");

            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            if (usuarioId == null && !(controller == "Usuarios" && action == "Login"))
            {
                context.Result = new RedirectToActionResult("Login", "Usuarios", null);
            }
            base.OnActionExecuting(context);
        }
    }
}
