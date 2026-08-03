using System.Collections.Concurrent;
using System.Threading.Channels;

namespace NumuneYonetim.Web.Services;

public class CanliBildirimService
{
    private readonly ConcurrentDictionary<Guid, Channel<string>> _aboneler = new();
    public (Guid Id, ChannelReader<string> Reader) AboneOl()
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<string>();
        _aboneler[id] = channel;
        return (id, channel.Reader);
    }
    public void AboneliktenCik(Guid id) => _aboneler.TryRemove(id, out _);
    public void Yayinla(string olay)
    {
        foreach (var channel in _aboneler.Values) channel.Writer.TryWrite(olay);
    }
}
