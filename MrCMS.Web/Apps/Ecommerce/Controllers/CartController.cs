﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.Cart;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services;
using MrCMS.Web.Apps.Ecommerce.Services.Cart;
using MrCMS.Web.Apps.Ecommerce.Services.Orders;
using MrCMS.Website.Binders;
using MrCMS.Website.Controllers;
using NHibernate;

namespace MrCMS.Web.Apps.Ecommerce.Controllers
{
    public class CartController : MrCMSAppUIController<EcommerceApp>
    {
        private readonly CartModel _cart;
        private readonly IUniquePageService _uniquePageService;
        private readonly ICartManager _cartManager;
        private readonly ICartValidationService _cartValidationService;
        private readonly IOrderShippingService _orderShippingService;
        private readonly IDocumentService _documentService;

        public CartController(ICartManager cartManager, ICartValidationService cartValidationService,
                              IOrderShippingService orderShippingService, IDocumentService documentService, CartModel cart, IUniquePageService uniquePageService)
        {
            _cartManager = cartManager;
            _cartValidationService = cartValidationService;
            _orderShippingService = orderShippingService;
            _documentService = documentService;
            _cart = cart;
            _uniquePageService = uniquePageService;
        }

        public ViewResult Show(Cart page)
        {
            SetupViewData();
            return View(page);
        }

        private void SetupViewData()
        {
            ViewData["cart"] = _cart;
            ViewData["shipping-calculations"] = _orderShippingService.GetCheapestShippingOptions(_cart);
        }

        [HttpGet]
        public PartialViewResult Details()
        {
            SetupViewData();
            return PartialView(_cart);
        }

        public JsonResult CanAddQuantity(AddToCartModel model)
        {
            var result = _cartValidationService.CanAddQuantity(model);
            return result.Valid
                       ? Json(true, JsonRequestBehavior.AllowGet)
                       : Json(result.Message, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public PartialViewResult AddToCart(AddToCartModel model)
        {
            return PartialView(model);
        }

        [HttpPost]
        [ActionName("AddToCart")]
        public RedirectResult AddToCart_POST(AddToCartModel model)
        {
            if (_cartValidationService.CanAddQuantity(model).Valid)
            {
                _cartManager.AddToCart(model);
                var addedToCart = _uniquePageService.GetUniquePage<ProductAddedToCart>();
                if (addedToCart != null && addedToCart.Published)
                    return Redirect(UniquePageHelper.GetUrl<ProductAddedToCart>(new { id = model.ProductVariant.Id, quantity = model.Quantity }));
                return Redirect(UniquePageHelper.GetUrl<Cart>());
            }
            return Redirect(UniquePageHelper.GetUrl<ProductSearch>());
        }

        [ActionName("DeleteCartItem")]
        [HttpPost]
        public JsonResult DeleteCartItem_POST(CartItem item)
        {
            _cartManager.Delete(item);
            return Json(true);
        }

        [HttpGet]
        public ViewResult DiscountCode()
        {
            return View(_cart);
        }

        [HttpGet]
        public ViewResult AddDiscountCode()
        {
            return View(_cart);
        }

        [HttpPost]
        public JsonResult ApplyDiscountCode(string discountCode)
        {
            if (String.IsNullOrWhiteSpace(discountCode))
            {
                _cartManager.SetDiscountCode(discountCode);
                return Json("Removed");
            }
            _cartManager.SetDiscountCode(discountCode);
            return Json(discountCode);
        }

        [HttpPost]
        public JsonResult UpdateBasket([IoCModelBinder(typeof(UpdateBasketModelBinder))] List<CartUpdateValue> quantities)
        {
            _cartManager.UpdateQuantities(quantities);
            return Json(true);
        }

        [HttpPost]
        public JsonResult EmptyBasket()
        {
            _cartManager.EmptyBasket();
            return Json(true);
        }
    }

    public class UpdateBasketModelBinder : MrCMSDefaultModelBinder
    {
        public UpdateBasketModelBinder(ISession session)
            : base(() => session)
        {
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var cartUpdateValues = new List<CartUpdateValue>();

            var splitQuantities = (controllerContext.HttpContext.Request["quantities"] ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            splitQuantities.ForEach(s =>
                                        {
                                            var strings = s.Split(':');
                                            cartUpdateValues.Add(new CartUpdateValue
                                            {
                                                ItemId = Convert.ToInt32(strings[0]),
                                                Quantity = Convert.ToInt32(strings[1])
                                            });
                                        });
            return cartUpdateValues;
        }
    }
}