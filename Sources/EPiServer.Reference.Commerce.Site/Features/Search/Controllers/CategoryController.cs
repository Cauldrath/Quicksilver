using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Controllers
{
    public class CategoryController : ContentController<FashionNode>
    {
        private readonly SearchViewModelFactory _viewModelFactory;
        private readonly FlagshipProductIndexViewModelFactory _fsProductIndexViewModelFactory;

        public CategoryController(
            SearchViewModelFactory viewModelFactory,
            FlagshipProductIndexViewModelFactory fsProductIndexViewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
            _fsProductIndexViewModelFactory = fsProductIndexViewModelFactory;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Index(FashionNode currentContent, FilterOptionViewModel viewModel)
        {
            if (Request.Headers.Get("Accept") == "application/json")
            {
                var fsProductIndex = _fsProductIndexViewModelFactory.Create(currentContent, viewModel);
                var json = Shared.FlagshipViewModels.Serialize.ToJson(fsProductIndex);
                return Content(json, "application/json");
            }

            var model = _viewModelFactory.Create(currentContent, viewModel);

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult Facet(FashionNode currentContent, FilterOptionViewModel viewModel)
        {
            return PartialView("_Facet", viewModel);
        }
    }
}