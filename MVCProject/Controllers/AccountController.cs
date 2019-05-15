using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace Authentication.Controllers
{
    public class AccountController : Controller
    {
        public void Login()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties() { RedirectUri = "/" }, Startup.SusiPolicyId);
            }
        }
        public void Logout()
        {
            if (Request.IsAuthenticated)
            {
                IEnumerable<AuthenticationDescription> authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
                HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
                Request.GetOwinContext().Authentication.GetAuthenticationTypes();
            }
        }
            public ActionResult Error(string message)
            {
                ViewBag.Message = message;

                return View("Error");
            }
        }

}