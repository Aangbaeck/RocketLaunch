using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace BetterStart.Helper
{
    public static class Bson
    {
        public static void SaveToBson<T>(T value, string path)
        {
            using (FileStream ms = new FileStream(path, FileMode.Create))
            using (BsonDataWriter datawriter = new BsonDataWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(datawriter, value);
            }

        }

        public static T FromBson<T>(string path)
        {
            
            using (FileStream ms = new FileStream(path,FileMode.Open))
            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}
