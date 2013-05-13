using System.Net;
using HackandCraft.Config;

namespace HackandCraft.Payment.PaymentService
{
    public class PaymentEndPoint
    {
        private string adyenRecurringEndPoint;
        private string adyenEndPoint;
        private string adyenUser;
        private string adyenPwd;
        public comadyenpaltest.Payment paymentService;
       

        public PaymentEndPoint()
        {
        
            adyenRecurringEndPoint = Globals.Instance.settings["AdyenRecurringEndPoint"];
            adyenEndPoint = Globals.Instance.settings["AdyenEndPoint"];
            adyenUser = Globals.Instance.settings["AdyenUser"];
            adyenPwd = Globals.Instance.settings["AdyenPwd"];
            paymentService = new comadyenpaltest.Payment
                {
                    Url = adyenEndPoint,
                    Credentials = new NetworkCredential(adyenUser, adyenPwd)
                };
        }

    }
}