using System;

namespace SQLiteNetExtensions.Extensions.TextBlob.Serializers
{
    public class JsonBlobSerializer : ITextBlobSerializer
    {
		// Stripped out to quickly prevent dll issue on Win8 background tasks

        public string Serialize(object element)
        {
			throw new NotImplementedException();

            // return JsonConvert.SerializeObject(element);
        }

        public object Deserialize(string text, Type type) {
			throw new NotImplementedException();

            // return JsonConvert.DeserializeObject(text, type);
        }
    }
}