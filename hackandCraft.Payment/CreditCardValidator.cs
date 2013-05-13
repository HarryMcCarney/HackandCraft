using System;
using NLog;

namespace BurnPlus.Services.PaymentService
{

    public sealed class CardValidator
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private CardValidator() { } // static only

        public static bool Validate(string cardType, string cardNumber)
        {
            bool ret;
            try
            {

                int[] number = new int[16]; // number to validate



                // Remove non-digits
                int len = cardNumber.Length;
                for (int i = 0; i < cardNumber.Length; i++)
                {
                    number[i] = Convert.ToInt32(cardNumber.Substring(i, 1));
                }

                // Validate based on card type, first if tests length, second tests prefix
                switch (cardType)
                {
                    case "MC":
                        if (len != 16)
                            return false;
                        if (number[0] != 5 || number[1] == 0 || number[1] > 5)
                            return false;
                        break;

                    case "BANKCARD":
                        if (len != 16)
                            return false;
                        if (number[0] != 5 || number[1] != 6 || number[2] > 1)
                            return false;
                        break;

                    case "VISA":
                        if (len != 16 && len != 13)
                            return false;
                        if (number[0] != 4)
                            return false;
                        break;

                    case "AMEX":
                        if (len != 15)
                            return false;
                        if (number[0] != 3 || (number[1] != 4 && number[1] != 7))
                            return false;
                        break;

                    case "DISCOVER":
                        if (len != 16)
                            return false;
                        if (number[0] != 6 || number[1] != 0 || number[2] != 1 || number[3] != 1)
                            return false;
                        break;

                    case "DINERSCLUB":
                        if (len != 14)
                            return false;
                        if (number[0] != 3 || (number[1] != 0 && number[1] != 6 && number[1] != 8)
                           || number[1] == 0 && number[2] > 5)
                            return false;
                        break;

                    case "JCB":
                        if (len != 16 && len != 15)
                            return false;
                        if (number[0] != 3 || number[1] != 5)
                            return false;
                        break;
                }

                // Use Luhn Algorithm to validate
                int sum = 0;
                for (int i = len - 1; i >= 0; i--)
                {
                    if (i % 2 == len % 2)
                    {
                        int n = number[i] * 2;
                        sum += (n / 10) + (n % 10);
                    }
                    else
                        sum += number[i];
                }
                ret = (sum % 10 == 0);
            }
            catch (Exception exp)
            {
                log.Warn(exp);
                ret = false;
            }
            return ret;
        }
    }
}