using Microsoft.AspNetCore.SignalR;


namespace WebLoadBalancer.Hubs
{
    public class ProgressHub : Hub
    {
        public async Task SendProgressUpdate(int progress)
        {
            Console.WriteLine($"Received progress update: {progress}");
            await Clients.All.SendAsync("ReceiveProgressUpdate", progress);
        }
    }
}
