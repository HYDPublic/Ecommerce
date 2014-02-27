﻿using System.Web.Mvc;
using MrCMS.Web.Apps.Articles.ModelBinders;
using MrCMS.Web.Apps.Core.ModelBinders;
using MrCMS.Web.Apps.Core.Models.Search;
using MrCMS.Web.Apps.Core.Pages;
using MrCMS.Web.Apps.Core.Services.Search;
using MrCMS.Website.Binders;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Apps.Core.Controllers
{
    public class SearchPageController : MrCMSAppUIController<CoreApp>
    {
        private readonly IWebpageSearchService _webpageSearchService;

        public SearchPageController(IWebpageSearchService webpageSearchService)
        {
            _webpageSearchService = webpageSearchService;
        }

        public ActionResult Show(SearchPage page, [IoCModelBinder(typeof(WebpageSearchQueryModelBinder))]WebpageSearchQuery model)
        {
            ViewData["searchResults"] = _webpageSearchService.Search(model);
            ViewData["webpageSearchQueryModel"] = model;
            return View(page);
        }
    }
}
