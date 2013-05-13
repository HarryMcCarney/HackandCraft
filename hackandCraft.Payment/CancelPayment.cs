using HackandCraft.Config;
using HackandCraft.Payment;
using HackandCraft.Payment.PaymentService;
using NLog;
using HackandCraft.Payment.comadyenpaltest;
using Payment = HackandCraft.Payment.Payment;
using adyen = HackandCraft.Payment.comadyenpaltest;

namespace BurnPlus.Services.PaymentService
{
 
    public class CancelPayment
    {
        private adyen.Payment paymentEndPoint;
        private Payment payment;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();    
        public CancelPayment(Payment _payment)
        {
            payment = _payment;
            paymentEndPoint = new PaymentEndPoint().paymentService;
        }

        public void cancelPayment()
        {
            log.Info("Cancelling payment");
            var cancelRequest = new adyen.ModificationRequest
                {
                    originalReference = payment.transactionId,
                    merchantAccount = Globals.Instance.settings["AdyenMerchantAccount"]
                };
            var result = paymentEndPoint.cancel(cancelRequest);
            var paymentResult = new adyen.PaymentResult{resultCode = "CANCELLATION", pspReference = result.pspReference};
            CreatePaymentNotice.createSave(payment, paymentResult);


        }

        public void refundPayment()
        {
            log.Info("Cancelling payment");
            var cancelRequest = new adyen.ModificationRequest();
            var adyenAmount = new adyen.Amount() { currency = "EUR", value = payment.amount };
            cancelRequest.modificationAmount = adyenAmount;
            cancelRequest.originalReference = payment.transactionId;
            cancelRequest.merchantAccount = Globals.Instance.settings["AdyenMerchantAccount"];
            var result = paymentEndPoint.refund(cancelRequest);
            var paymentResult = new adyen.PaymentResult { resultCode = "REFUND", pspReference = result.pspReference };
            CreatePaymentNotice.createSave(payment, paymentResult);


        }

    
    
    }
}