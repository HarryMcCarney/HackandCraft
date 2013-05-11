using System;
using System.Xml.Serialization;

namespace HackandCraft.Config
{
    [Serializable]
    public class Global
    {
        [XmlElement]
        public Setting[] Setting { get; set; }
    }
}
