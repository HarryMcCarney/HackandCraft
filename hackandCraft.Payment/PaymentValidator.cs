



using HackandCraft.Payment;

namespace BurnPlus.Services.PaymentService
{
    public static class PaymentValidator
    {

        public static PaymentStatus validatePayment(Payment payment)
        {

            var vp = new PaymentStatus();

            if (payment.method == "PAYPAL" || payment.useSavedDetails)
            {

                vp.success = true; //paypal always true for now

            }
            else
            {
                vp.success = CardValidator.Validate(payment.method, payment.number);
            }
            if (vp.success == false) // all invalid mean bad card number for now.
            {
                vp.message = "INVALID_CARD_NUMBER";
            }
            return vp;
        }

    }
}