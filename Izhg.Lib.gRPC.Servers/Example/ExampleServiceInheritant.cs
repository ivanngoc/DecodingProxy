using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using IziHardGames.Libs.gRPC.Examples;

namespace IziHardGames.Libs.gRPC.Example
{
    public class ExampleServiceInheritant : ExampleService.ExampleServiceBase
    {
        public override Task<ExampleResponse> ExampleMethodSimple(ExampleRequest request, ServerCallContext context)
        {
            ExampleResponse response = new ExampleResponse();

            string strinValue = response.ValueString;
            int valueInt32 = response.ValueInt32;
            Google.Protobuf.Collections.RepeatedField<int> valueArrayInt32 = response.ValueArrayInt32;
            Google.Protobuf.ByteString bytes = response.ValueBytes;


            return base.ExampleMethodSimple(request, context);
        }
    }
}
