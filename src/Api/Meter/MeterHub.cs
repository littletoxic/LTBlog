using LTBlog.Data;
using Microsoft.AspNetCore.SignalR;

namespace LTBlog.Api.Meter;

public class MeterHub : Hub<IDataClient> {
    public async Task SendState(MeterData data) {
        await Clients.All.ReceiveData(data);
    }
}

public interface IDataClient {
    Task ReceiveData(MeterData state);
}