using System.Threading.Tasks;
using Grpc.Core;

namespace Izhg.Libs.gRPC.Shared
{
    public class GrpcUtilForClients
    {
        public static async Task Handle<T>(AsyncDuplexStreamingCall<T, T> streams)
        {
            var read = streams.RequestStream;
            var write = streams.ResponseStream;

            throw new System.NotImplementedException();
        }
        public static async Task Handle<T1,T2>(AsyncDuplexStreamingCall<T1, T2> streams)
        {
            throw new System.NotImplementedException();
        }
    }
}