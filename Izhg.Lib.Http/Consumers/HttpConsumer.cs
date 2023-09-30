using IziHardGames.Proxy.Consuming;
using System.Text;
using System;
using IziHardGames.Libs.NonEngine.Collections;
using IziHardGames.Libs.Networking.States;

namespace IziHardGames.Libs.ForHttp
{
    public class HttpConsumer
    {
        public event Action OnCloseIndicatedEvent;
        protected ManagerForHttpSession manager;

        public virtual void PushResponseHttp20(HttpSource dataSource, ReadOnlyMemory<byte> mem)
        {
            var session = manager.GetSession(dataSource);
            session.AddResponse20(mem);
            session.CheckClose(dataSource);
        }
        public virtual void PushRequestHttp20(HttpSource dataSource, ReadOnlyMemory<byte> mem)
        {
            var session = manager.GetSession(dataSource);
            session.AddRequest20(mem);
            session.CheckClose(dataSource);
        }


        public virtual void PushRequestHttp11(HttpSource dataSource, ReadOnlyMemory<byte> mem)
        {
            var session = manager.GetSession(dataSource);
            session.AddResponse11(mem);
            session.CheckClose(dataSource);
        }
        public virtual void PushResponseHttp11(HttpSource dataSource, ReadOnlyMemory<byte> mem)
        {
            var session = manager.GetSession(dataSource);
            session.AddRequest11(mem);
            session.CheckClose(dataSource);
        }

        public void SetManager(ManagerForHttpSession manager)
        {
            this.manager = manager;
        }
    }

    public abstract class ManagerForHttpSession
    {
        public abstract HttpSession GetSession(HttpSource dataSource);
    }

    public abstract class HttpSession
    {
        public abstract void CheckClose(HttpSource dataSource);

        public abstract void AddResponse20(ReadOnlyMemory<byte> mem);
        public abstract void AddRequest20(ReadOnlyMemory<byte> mem);

        public abstract void AddResponse11(ReadOnlyMemory<byte> mem);
        public abstract void AddRequest11(ReadOnlyMemory<byte> mem);
    }
}
