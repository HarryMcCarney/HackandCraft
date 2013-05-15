using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace mandrill.net.Fetching
{
    [Serializable]
    public class Message
    {
        [XmlAttribute]
        public string id { get; set; }
        [XmlAttribute]
        public string type { get; set; }
        [XmlAttribute]
        public string template { get; set; }
        [XmlAttribute]
        public string recipient { get; set; }
        [XmlElement]
        public List<Field> Field { get; set; }

    }
}
