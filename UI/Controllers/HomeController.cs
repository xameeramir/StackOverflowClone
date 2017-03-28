using System.Web.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Session["VisitorIP"] = this.GetVisitorIP(Session["VisitorIP"]);
            TempData["Title"] = "Glad to see you!";

            return View();
        }

        public ActionResult Privacy()
        {
            TempData["Title"] = "Privacy";

            return View();
        }

        public ActionResult Overview()
        {
            TempData["Title"] = "Get familiar";

            return View();
        }

        public ActionResult VoteDown()
        {
            return View();
        }

        public ActionResult Legal()
        {
            TempData["Title"] = "Legal";
            return View();
        }

        public ActionResult Modification()
        {
            return View();
        }
    }
}