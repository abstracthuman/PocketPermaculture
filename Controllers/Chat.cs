using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PocketPermaculture.Hubs;

namespace PocketPermaculture.Controllers
{
    public class Chat : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public Chat(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}
