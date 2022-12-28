using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using Genersoft.GS.HBYHQYSCommon.Bean.responseBean;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Genersoft.GS.HBYHQYSCommon.utils
{
    public class JSONUtils
    {
        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            });
        }

        public static bool GetBoolean(string status)
        {
            return status.Equals("true") ? true : false;
        }
    }
}