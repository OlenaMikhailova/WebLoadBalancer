using Microsoft.AspNetCore.Mvc;

namespace WebLoadBalancer.Controllers
{
    [Route("balance")]
    public class LoadBalancerController : Controller
    {
        private readonly List<string> serverUrls = new List<string>
        {
        "http://localhost:5000",
        "http://localhost:5001",
        // Додайте інші сервери за потреби
        };

        private readonly Dictionary<string, int> serverRequestCounts = new Dictionary<string, int>();

        [HttpGet]
        
        public IActionResult BalanceRequest()
        {
            // Знайдіть сервер з найменшою кількістю запитів
            string selectedServer = FindServerWithMinimumRequests();

            // Збільште лічильник запитів для обраного сервера
            IncrementRequestCount(selectedServer);

            // Перенаправте запит на обраний сервер
            return Redirect(selectedServer);
        }

        private string FindServerWithMinimumRequests()
        {
            string selectedServer = serverUrls[0];
            int minRequestCount = int.MaxValue;

            foreach (var server in serverUrls)
            {
                if (!serverRequestCounts.ContainsKey(server))
                {
                    serverRequestCounts[server] = 0;
                }

                int requestCount = serverRequestCounts[server];

                if (requestCount < minRequestCount)
                {
                    minRequestCount = requestCount;
                    selectedServer = server;
                }
            }

            return selectedServer;
        }

        private void IncrementRequestCount(string server)
        {
            if (serverRequestCounts.ContainsKey(server))
            {
                serverRequestCounts[server]++;
            }
            else
            {
                serverRequestCounts[server] = 1;
            }
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
