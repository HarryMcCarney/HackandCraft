using System;
using System.Xml.Serialization;

namespace HackandCraft.Payment
{
    [Serializable]
    public class Payment :IPayment
    {
        [XmlAttribute]
        public string paymentRef { get; set; }

        [XmlAttribute]
        public int amount { get; set; }

        [XmlAttribute]
        public string method { get; set; }

        [XmlAttribute]
        public string holder { get; set; }

        [XmlAttribute]
        public string expiryYear { get; set; }

        [XmlAttribute]
        public string expiryMonth { get; set; }

        [XmlAttribute]
        public string cvs { get; set; }

        [XmlAttribute]
        public string number { get; set; }

        [XmlAttribute]
        public bool saveDetails { get; set; }

        [XmlAttribute]
        public bool useSavedDetails { get; set; }

        [XmlAttribute]
        public string currency { get; set; }

        [XmlAttribute]
        public string shopperRef { get; set; }

        [XmlAttribute]
        public string shopperEmail { get; set; }

        [XmlAttribute]
        public string accountHolderName { get; set; }

        [XmlAttribute]
        public string bankAccountNumber { get; set; }

        [XmlAttribute]
        public string bankLocation { get; set; }

        [XmlAttribute]
        public string bankLocationId { get; set; }

        [XmlAttribute]
        public string bankName { get; set; }

        [XmlAttribute]
        public string userToken { get; set; }

        [XmlAttribute]
        public string transactionId { get; set; }
    }
}
