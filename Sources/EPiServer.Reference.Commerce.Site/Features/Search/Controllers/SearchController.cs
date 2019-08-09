using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Controllers
{
    public class SearchController : PageController<SearchPage>
    {
        private readonly SearchViewModelFactory _viewModelFactory;
        private readonly ISearchService _searchService;
        private readonly FlagshipProductIndexViewModelFactory _fsProductIndexViewModelFactory;

        public SearchController(
            SearchViewModelFactory viewModelFactory, 
            ISearchService searchService,
            FlagshipProductIndexViewModelFactory fsProductIndexViewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
            _searchService = searchService;
            _fsProductIndexViewModelFactory = fsProductIndexViewModelFactory;
        }

        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Index(SearchPage currentPage, FilterOptionViewModel filterOptions)
        {
            if (Request.Headers.Get("Accept").Contains("application/json"))
            {
                var fsProductIndex = _fsProductIndexViewModelFactory.Create(currentPage, filterOptions, Request.Url);
                var json = Shared.FlagshipViewModels.Serialize.ToJson(fsProductIndex);
                return Content(json, "application/json");
            }

            var viewModel = _viewModelFactory.Create(currentPage, filterOptions);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult QuickSearch(string q = "")
        {
            var result = _searchService.QuickSearch(q);
            return View("_QuickSearch", result);
        }
    }
}