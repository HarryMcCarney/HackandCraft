using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Web;
using HackandCraft.Config;
using NLog;


namespace HackandCraft.Payment.PaymentService
{
    public static class PaypalBuilder
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static string redirectUrl(IPayment payment)
        {
            try
            {
                var redirectDomain = Globals.Instance.settings["AdyenRedirectDomain"];
                var queryString = getQueryString(payment);
                queryString["merchantSig"] = HttpUtility.UrlPathEncode(buildSignature(queryString));
                return redirectDomain + "?" + queryString.ToString();
            }
            catch(Exception exp)
            {
                log.Error(exp);
                throw;
            }


        }
    
        private static NameValueCollection getQueryString(IPayment payment)
        {
            try
            {
                var queryString = HttpUtility.ParseQueryString(String.Empty);
                queryString["merchantAccount"] = Globals.Instance.settings["AdyenMerchantAccount"];
                queryString["skinCode"] = Globals.Instance.settings["AdyenPaypalSkinCode"];
                queryString["shipBeforeDate"] = DateTime.Now.AddDays(5).ToUniversalTime().ToString("yyyy-MM-dd");
                queryString["sessionValidity"] = DateTime.Now.AddHours(3).ToUniversalTime().ToString("s", DateTimeFormatInfo.InvariantInfo) + "Z"; ;
                queryString["allowedMethods"] = "paypal";
                //queryString["paymentAmount"] = payment.amount.ToString(CultureInfo.InvariantCulture);
                queryString["paymentAmount"] = 50.ToString(CultureInfo.InvariantCulture);
                queryString["currencyCode"] = payment.currency;
                queryString["shopperLocale"] = "de_DE";
                queryString["merchantReference"] = payment.paymentRef;
                queryString["brandCode"] = "paypal";
                queryString["merchantReturnData"] = payment.paymentRef;
                return queryString;
            }
            catch(Exception exp)
            {
                log.Error(exp);
                throw;
            }

        }

        private static string buildSignature(NameValueCollection queryString)
        {
            try
            {
            

            //// The HMAC secret as configured in the skin
                string hmacSecret = Globals.Instance.settings["PayPalhmacSecret"];

            //// Generate the signing string
            string signingString = queryString["paymentAmount"] + queryString["currencyCode"] +
                                   queryString["shipBeforeDate"] + queryString["merchantReference"] +
                                   queryString["skinCode"] + queryString["merchantAccount"] +
                                   queryString["sessionValidity"] + queryString["allowedMethods"] + queryString["merchantReturnData"];

            //// Values are always transferred using UTF-8 encoding
            var encoding = new System.Text.UTF8Encoding();

            //// Calculate the HMAC
            var myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            return Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signingString)));
                }
            catch (Exception exp)
            {
                log.Error(exp);
                throw;
            }

        }
    }
}