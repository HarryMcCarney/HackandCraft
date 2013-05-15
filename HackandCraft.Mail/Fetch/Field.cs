using System;
using System.Xml.Serialization;

namespace mandrill.net.Fetching
{
    [Serializable]
    public  class Field
    {
        [XmlAttribute]
        public string key { get; set; }
        [XmlAttribute]
        public string value { get; set; }
    }
}