using System;
using System.Xml.Serialization;

namespace HackandCraft.Config
{
    [Serializable]
    public class Setting
    {
        [XmlAttribute]
        public string key { get; set; }
        [XmlAttribute]
        public string value { get; set; }
    }
}
