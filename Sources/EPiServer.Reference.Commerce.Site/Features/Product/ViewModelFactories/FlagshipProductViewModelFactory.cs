using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.FlagshipViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Pricing;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class FlagshipProductViewModelFactory
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly UrlResolver _urlResolver;
        private readonly CatalogContentService _catalogContentService;

        public FlagshipProductViewModelFactory(
            IContentLoader contentLoader,
            IPricingService pricingService,
            UrlResolver urlResolver,
            CatalogContentService catalogContentService)
        {
            _contentLoader = contentLoader;
            _pricingService = pricingService;
            _urlResolver = urlResolver;
            _catalogContentService = catalogContentService;
        }

        public virtual Shared.FlagshipViewModels.Product Create(FashionProduct currentContent, string variationCode)
        {
            var variants = _catalogContentService.GetVariants<FashionVariant>(currentContent).ToList();
            var selectedVariant = variants.Find(variant => variant.Code == variationCode);
            var formattedVariants = variants.Select(CreateVariant).ToList();

            var code = selectedVariant?.Code ?? currentContent.Code;
            var images = selectedVariant?.GetAssets<IContentImage>(_contentLoader, _urlResolver) ?? currentContent.GetAssets<IContentImage>(_contentLoader, _urlResolver);

            return new Shared.FlagshipViewModels.Product
            {
                Available = false,
                Brand = currentContent.Brand,
                Description = (currentContent.LongDescription ?? currentContent.Description).ToString(),
                Handle = selectedVariant?.SeoUri ?? currentContent.SeoUri,
                Id = code,
                Images = images.Select(uri => new Image { Uri = uri }).ToList(),
                Options = CreateOptions(variants),
                OriginalPrice = CreateCurrencyValue(_pricingService.GetDefaultPrice(code)),
                Price = CreateCurrencyValue(_pricingService.GetPrice(code)),
                Title = selectedVariant?.DisplayName ?? currentContent.DisplayName,
                Variants = formattedVariants
            };
        }

        protected virtual List<Option> CreateOptions(List<FashionVariant> variants)
        {
            return new List<Option> {
                new Option {
                    Id = "Color",
                    Name = "Color",
                    Values = variants
                        .Where(variant => variant.Size != null)
                        .GroupBy(variant => variant.Color)
                        .Select(group => new OptionValue {
                            Name = group.Key,
                            Value = group.Key
                        })
                        .ToList()
                },
                new Option
                {
                    Id = "Size",
                    Name = "Size",
                    Values = variants
                        .Where(variant => variant.Color != null)
                        .GroupBy(variant => variant.Size)
                        .Select(group => new OptionValue {
                            Name = group.Key,
                            Value = group.Key
                        })
                        .ToList()
                }
            };
        }

        protected virtual Variant CreateVariant(FashionVariant variant)
        {
            var id = variant.Code;

            return new Variant
            {
                Id = id,
                OptionValues = new List<OptionValue>() {
                    new OptionValue
                    {
                        Name = "Color",
                        Value = variant.Color
                    },
                    new OptionValue
                    {
                        Name = "Size",
                        Value = variant.Size
                    }
                },
                Price = CreateCurrencyValue(_pricingService.GetPrice(id)),
                OriginalPrice = CreateCurrencyValue(_pricingService.GetDefaultPrice(id)),
                Title = variant.DisplayName,
                Images = variant.GetAssets<IContentImage>(_contentLoader, _urlResolver).Select(uri => new Image { Uri = uri }).ToList(),
                Available = _pricingService.GetDefaultPrice(id) != null
            };
        }

        protected virtual CurrencyValue CreateCurrencyValue(IPriceValue price)
        {
            if (price == null || price.UnitPrice == null || price.UnitPrice == 0)
            {
                return null;
            }

            return new CurrencyValue
            {
                CurrencyCode = price.UnitPrice.Currency,
                Value = price.UnitPrice.Amount.ToString()
            };
        }
    }
}