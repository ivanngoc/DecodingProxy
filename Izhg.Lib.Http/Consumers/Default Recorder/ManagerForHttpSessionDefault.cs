using IziHardGames.Proxy.Consuming;
using System.Collections.Concurrent;
using System.Linq;

namespace IziHardGames.Libs.ForHttp
{
    public class ManagerForHttpSessionDefault : ManagerForHttpSession
    {
        public ConcurrentDictionary<int, Generation> items = new ConcurrentDictionary<int, Generation>();
        public override HttpSession GetSession(HttpSource dataSource)
        {
            var gen = items.GetOrAdd(dataSource.id, (x) =>
            {
                var result = new Generation()
                {
                    id = x,
                };
                return result;
            });
            var result = gen.items.FirstOrDefault(x => x.version == dataSource.generation);
            if (result is null)
            {
                result = new HttpSessionDefault()
                {
                    id = dataSource.id,
                    version = dataSource.generation,
                    host = dataSource.host,
                    port = dataSource.port,
                    flagsAgent = dataSource.flagsAgent,
                    flagsOrigin = dataSource.flagsOrigin,
                };
                gen.items.Add(result);
            }
            return result;
        }
    }
}
