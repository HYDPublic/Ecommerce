using System.Collections.Generic;
using System.Linq;
using System.Text;
using MrCMS.Web.Apps.Ecommerce.Entities.Users;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Web.Apps.Ecommerce.Payment.Paypoint;

namespace MrCMS.Web.Apps.Ecommerce.Services
{
    public class PaypointRequestHelper : IPaypointRequestHelper
    {
        private readonly PaypointSettings _paypointSettings;

        public PaypointRequestHelper(PaypointSettings paypointSettings)
        {
            _paypointSettings = paypointSettings;
        }

        public string GetTotal(decimal total)
        {
            return total.ToString("0.00");
        }

        public string GetOptions(PaypointPaymentDetailsModel model)
        {
            return string.Format("test_status={0},card_type={1},cv2={2},mail_customer=false",
                                 _paypointSettings.IsLive ? "live" : "true",
                                 model.CardType, model.CardVerificationCode);
        }

        public string GetDate(int? month, int? year)
        {
            if (month.GetValueOrDefault() == 0 || year.GetValueOrDefault() == 0)
                return string.Empty;

            var yearString = year.ToString();
            return string.Format("{0}{1}", month == null ? "00" : month.ToString().PadLeft(2, '0'),
                                 year == null ? "00" : yearString.Substring(yearString.Length - 2, 2));
        }

        public IDictionary<string, string> ParseEnrolmentResponse(string response)
        {
            response = response.Trim('?');
            var parameterList = response.Split('&');
            return parameterList.ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);
        }

        public string GetAddress(Address address, string email)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("name={0},", address.Name);
            stringBuilder.AppendFormat("company={0},", address.Company);
            stringBuilder.AppendFormat("addr_1={0},", address.Address1);
            stringBuilder.AppendFormat("addr_2={0},", address.Address2);
            stringBuilder.AppendFormat("city={0},", address.City);
            stringBuilder.AppendFormat("state={0},", address.StateProvince);
            stringBuilder.AppendFormat("post_code={0},", address.PostalCode);
            stringBuilder.AppendFormat("country={0},", address.Country.Name);
            stringBuilder.AppendFormat("email={0},", email);
            return stringBuilder.ToString();
        }

        public string GetOrderDetails(CartModel cartModel)
        {
            var lineData =
                cartModel.Items.Select(
                    line =>
                    string.Format("prod={0},item_amount={1:0.00}x{2}", line.Name, line.UnitPrice, line.Quantity))
                         .ToList();
            if (cartModel.DiscountAmount > 0)
            {
                lineData.Add(string.Format("prod=DISCOUNT,item_amount={0:N2}", cartModel.DiscountAmount));
            }
            return string.Join(";", lineData);
        }

    }
}