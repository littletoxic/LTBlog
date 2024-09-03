namespace LTBlog.Sensor;

using Microsoft.AspNetCore.SignalR;

public class SensorHub : Hub {

    public async Task SendState(object state) {
        await Clients.All.SendAsync("ReceiveState", state);
    }
}
