using MrCMS.Web.Apps.Ecommerce.Entities.Discounts;

namespace MrCMS.Web.Apps.Ecommerce.Entities.DiscountLimitations
{
    public class SingleUsage : DiscountLimitation
    {
        public override string DisplayName
        {
            get { return "Single use"; }
        }
    }
}