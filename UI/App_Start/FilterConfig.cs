using System.Web;
using System.Web.Mvc;
using UI.Models;

namespace UI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new PageViewAttribute());
        }
    }
}
