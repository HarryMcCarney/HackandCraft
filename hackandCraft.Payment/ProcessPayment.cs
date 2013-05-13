using System;
using HackandCraft.Config;
using HackandCraft.Payment;
using HackandCraft.Payment.PaymentService;
using HackandCraft.Payment.comadyenpaltest;
using NLog;
using adyen = HackandCraft.Payment.comadyenpaltest;
using DBVC;
using Payment = HackandCraft.Payment.Payment;


namespace BurnPlus.Services.PaymentService
{
    
    public class ProcessPayment
    {

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private PaymentStatus paymentStatus;
        private adyen.Payment paymentEndPoint;
        private Payment     payment;

        public ProcessPayment(Payment _payment)
        {
             payment = _payment;
             paymentStatus = new PaymentStatus();
             paymentEndPoint = new PaymentEndPoint().paymentService;
        }

        public PaymentStatus pay()
        {

            if (payment.method == "PAYPAL")
                paypalPayment();
            else
            if (payment.method == "ELV")
            {
                if (getElvDetails())
                    makePayment();
            }
            else //credit cards
            {
                validate();
                if (paymentStatus.success)
                    makePayment();
            }

            if (paymentStatus.success && payment.method != "PAYPAL")
            {
                new CancelPayment(payment).cancelPayment();
            }
         
            return paymentStatus;
        }

        private bool getElvDetails()
        {
            bool ret = false;
            var orm = new Orm(new MSSQLData());
            var result = orm.execObject<Result>(payment,"api.get_elv_details");
            if (result.dbMessage == "INVALID_SORT_CODE")
            {
                paymentStatus.success = false;
                paymentStatus.message = result.dbMessage;
            }
            else 
            {
                payment.bankName = result.Payment.bankName;
                payment.bankLocation = result.Payment.bankLocation;
                ret = true;
            }
            return ret;
        }

       private void makePayment()
        {
            try
            {
                var response = paymentEndPoint.authorise(buildPaymentRequest());
                payment.transactionId = response.pspReference;
                CreatePaymentNotice.createSave(payment, response);
                paymentStatus.success = (response.resultCode != "Refused");
                paymentStatus.message = (paymentStatus.success) ? "PAYMENT_SUCCESSFUL" : "PAYMENT_FAILED";
            }

            catch (Exception exp)
            {
                paymentStatus.success = false;
                paymentStatus.message = exp.Message;
                log.Error(exp);
            }

        }


       public void paypalPayment()
       {
           paymentStatus.success = true;
           paymentStatus.redirectUrl = PaypalBuilder.redirectUrl(payment);
       }


        private void validate()
        {
            paymentStatus = PaymentValidator.validatePayment(payment);
        }

        private PaymentRequest buildPaymentRequest()
        {
            try
            {
                var request = new PaymentRequest
                                  {
                                      merchantAccount = Globals.Instance.settings["AdyenMerchantAccount"],
                                      amount = new Amount {currency = payment.currency, value = payment.amount},
                                      reference = payment.paymentRef
                                  };

                if (payment.method == "ELV")
                {
                    request.elv = new ELV()
                    {
                        accountHolderName = payment.accountHolderName,//"Simon わくわく Hopper", 
                        bankAccountNumber = payment.bankAccountNumber,
                        bankLocation = payment.bankLocation,
                        bankLocationId = payment.bankLocationId,
                        bankName = payment.bankName//"TestBank" 
                    };
                
                 }
                else {// credit card
                    
                        request.card = new Card
                                       {
                                           brand = payment.method.ToLower(),
                                           cvc = payment.cvs,
                                           expiryMonth = payment.expiryMonth,
                                           expiryYear = payment.expiryYear,
                                           holderName = payment.holder,
                                           number = payment.number
                                       };
                        }

                    request.recurring = new Recurring() {contract = "ONECLICK"};
                    request.shopperReference = payment.shopperRef;
                    request.shopperEmail = payment.shopperRef;
    
                return request;
            }
            catch (Exception exp)
            {
                log.Error(exp);
                throw;
            }
        
        }


       


       
    }
    
}