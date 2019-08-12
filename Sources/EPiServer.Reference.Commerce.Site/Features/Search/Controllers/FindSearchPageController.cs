using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Controllers
{
    public class FindSearchPageController : PageController<SearchPage>
    {
        public int pageSize = 10;

        public ActionResult Index(SearchPage currentPage, string q, int page = 1)
        {
            var model = new FindSearchPageViewModel(currentPage, q, page);
            if (String.IsNullOrEmpty(q))
            {
                return View(model);
            }
            var unifiedSearch = SearchClient.Instance.UnifiedSearchFor(q);
            model.Results = unifiedSearch.Skip((page - 1) * pageSize).Take(pageSize).Filter(x => 
                x.SearchTypeName.Match("Product")
            ).GetResult();
            return View(model);
        }
    }
}