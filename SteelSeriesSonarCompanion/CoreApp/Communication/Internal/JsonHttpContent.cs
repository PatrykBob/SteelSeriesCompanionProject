using SteelSeriesSonarCompanion.Shared.Core;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
    public class JsonHttpContent : HttpContent
    {
        private readonly string data;

        public JsonHttpContent (object toSerialize)
        {
            data = JsonConverter.ConvertToJSON(toSerialize);
        }

        protected override Task SerializeToStreamAsync (Stream stream, TransportContext? context)
        {
            return stream.WriteAsync(Encoding.UTF8.GetBytes(data)).AsTask();
        }

        protected override bool TryComputeLength (out long length)
        {
            length = 0;
            return false;
        }
    }
}
