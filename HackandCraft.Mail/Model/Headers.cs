using System.Runtime.Serialization;

namespace mandrill.net
{
    [DataContract]
    public class Headers
    {
        [DataMember(Name = "Reply-To")]
        public string replyTo = Config.Instance.replyTo;
    }
}