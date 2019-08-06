using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.FlagshipViewModels;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories
{
    public class FlagshipProductIndexViewModelFactory
    {
        private readonly ISearchService _searchService;
        private readonly LocalizationService _localizationService;

        public FlagshipProductIndexViewModelFactory(LocalizationService localizationService, ISearchService searchService)
        {
            _searchService = searchService;
            _localizationService = localizationService;
        }

        public virtual ProductIndex Create(IContent currentContent, FilterOptionViewModel viewModel)
        {
            var customSearchResult = _searchService.Search(currentContent, viewModel);
            var totalResultCount = customSearchResult.SearchResult?.TotalCount ?? 0;

            return new ProductIndex
            {
                Products = CreateProducts(customSearchResult.ProductViewModels),
                Keyword = viewModel.Q,
                Limit = totalResultCount, // Results aren't paginated
                Page = 1,
                Total = totalResultCount,
                SortingOptions = GetSortingOptions(),
                SelectedSortingOption = viewModel.Sort,
                Refinements = CreateRefinements(customSearchResult.FacetGroups),
                SelectedRefinements = CreateSelectedRefinements(customSearchResult.FacetGroups),
            };
        }

        protected virtual List<Shared.FlagshipViewModels.Product> CreateProducts(IEnumerable<ProductTileViewModel> items)
        {
            return items
                .Select(item =>
                {
                    var Product = new Shared.FlagshipViewModels.Product
                    {
                        Title = item.DisplayName,
                        Images = new List<Image> { new Image { Uri = item.ImageUrl } },
                        Brand = item.Brand,
                        Id = item.Code,
                        Available = item.IsAvailable
                    };

                    if (item.PlacedPrice != null && item.PlacedPrice != 0)
                    {
                        Product.OriginalPrice = new CurrencyValue
                        {
                            CurrencyCode = item.PlacedPrice.Currency,
                            Value = item.PlacedPrice.Amount.ToString()
                        };
                        Product.Price = Product.OriginalPrice;
                    }

                    if (item.DiscountedPrice.HasValue && item.DiscountedPrice.Value != 0)
                    {
                        Product.Price = new CurrencyValue
                        {
                            CurrencyCode = item.DiscountedPrice.Value.Currency,
                            Value = item.DiscountedPrice.Value.Amount.ToString()
                        };
                    }

                    return Product;
                })
                .ToList();
        }

        protected virtual List<SortingOption> GetSortingOptions()
        {
            return _searchService
                .GetSortOrder()
                .Select(sort => new SortingOption
                {
                    Title = _localizationService.GetString("/Category/Sort/" + sort.Name),
                    Id = sort.Name.ToString()
                })
                .ToList();
        }

        protected virtual List<Refinement> CreateRefinements(IEnumerable<FacetGroupOption> FacetGroups)
        {
            return FacetGroups
                .Select(group => new Refinement
                {
                    Id = group.GroupFieldName,
                    Title = group.GroupName,
                    Values = group.Facets.Select(option => new RefinementValue
                    {
                        Count = option.Count,
                        Title = option.Name,
                        Value = option.Key
                    }).ToList()
                })
                .ToList();
        }

        protected virtual Dictionary<string, List<string>> CreateSelectedRefinements(IEnumerable<FacetGroupOption> FacetGroups)
        {
            return FacetGroups
                .Where(option => option.Facets.Exists(facet => facet.Selected))
                .ToDictionary(
                    group => group.GroupFieldName,
                    group => group.Facets.FindAll(facet => facet.Selected).Select(facet => facet.Key).ToList()
                );
        }
    }
}