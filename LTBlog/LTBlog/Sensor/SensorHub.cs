namespace LTBlog.Sensor;

using Client.Model;
using Microsoft.AspNetCore.SignalR;

public class SensorHub : Hub<IStateClient> {

    public async Task SendState(SensorState state) {
        await Clients.All.ReceiveState(state);
    }
}

public interface IStateClient {
    Task ReceiveState(SensorState state);
}