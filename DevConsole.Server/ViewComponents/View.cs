using Microsoft.AspNetCore.Mvc;

namespace DevConsole.Server.ViewComponents
{
    public class CompareWindowComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}