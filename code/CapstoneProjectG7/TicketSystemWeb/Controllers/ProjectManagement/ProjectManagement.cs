using Microsoft.AspNetCore.Mvc;

namespace TicketSystemWeb.Controllers.Projects
{
    public class ProjectManagement : Controller
    {
        public IActionResult Management()
        {
            return View();
        }
    }
}
