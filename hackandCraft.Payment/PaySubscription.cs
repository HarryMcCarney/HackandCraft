using System.Xml.Serialization;
using DBVC;
using HackandCraft.Payment;

namespace BurnPlus.Services.PaymentService
{
    public class PaySubscription
    {

        private IPayment payment;

        public PaySubscription(IPayment _payment, string _userToken)
        {
            payment = _payment;
            payment.userToken = _userToken;
        }

        public PaymentStatus makeInitalPayment()
        {
            var orm = new Orm();


            var result = orm.execObject<Result>(payment, "api.payment_create");

            if (result.errorMessage == null && result.dbMessage == null)
            {
                payment.paymentRef = result.Payment.paymentRef;
                payment.shopperEmail = result.Payment.shopperEmail;
                payment.shopperRef = result.Payment.shopperRef;
                payment.amount = result.Payment.amount;
                payment.currency = result.Payment.currency;
                payment.saveDetails = true;
                return new ProcessPayment(payment).pay();
            }
            return new PaymentStatus() {success = false, message = "PAYMENT_FAILED"};
        }
    }

    
}