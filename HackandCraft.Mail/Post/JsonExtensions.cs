using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace mandrill.net
{
   internal static  class JsonExtensions
    {
       internal static string serialise(this Template template)
       {
           return JsonConvert.SerializeObject(template, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

       }

    }
}
