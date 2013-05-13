using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HackandCraft.Payment
{
    public interface IPayment
    {
       
            [XmlAttribute]
            string paymentRef { get; set; }

            [XmlAttribute]
            int amount { get; set; }

            [XmlAttribute]
            string method { get; set; }

            [XmlAttribute]
            string holder { get; set; }

            [XmlAttribute]
            string expiryYear { get; set; }

            [XmlAttribute]
            string expiryMonth { get; set; }

            [XmlAttribute]
            string cvs { get; set; }

            [XmlAttribute]
            string number { get; set; }

            [XmlAttribute]
            bool saveDetails { get; set; }

            [XmlAttribute]
            bool useSavedDetails { get; set; }

            [XmlAttribute]
            string currency { get; set; }

            [XmlAttribute]
            string shopperRef { get; set; }

            [XmlAttribute]
            string shopperEmail { get; set; }

            [XmlAttribute]
            string accountHolderName { get; set; }

            [XmlAttribute]
            string bankAccountNumber { get; set; }

            [XmlAttribute]
            string bankLocation { get; set; }

            [XmlAttribute]
            string bankLocationId { get; set; }

            [XmlAttribute]
            string bankName { get; set; }

            [XmlAttribute]
            string userToken { get; set; }

            [XmlAttribute]
            string transactionId { get; set; }
        }
    }

