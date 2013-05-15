using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mandrill.net.Model
{
    public interface IMessage
    {
        string id { get; set; }
        string type { get; set; }
        string template { get; set; }
        string recipient { get; set; }
    }
}
