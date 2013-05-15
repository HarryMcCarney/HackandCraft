using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using mandrill.net.Fetching;

namespace mandrill.net.Model
{
    [Serializable]
    public class MessageStatus
    {
        [XmlElement]
        public Message Message { get; set; }
        [XmlAttribute]
        public string status { get; set; }
    }
}
