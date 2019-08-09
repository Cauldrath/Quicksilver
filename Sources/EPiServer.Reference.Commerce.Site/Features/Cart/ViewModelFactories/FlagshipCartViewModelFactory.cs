using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;


using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories
{
    [ServiceConfiguration(typeof(FlagshipCartViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class FlagshipCartViewModelFactory
    {
        private readonly CatalogContentService _catalogContentService;
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ShipmentViewModelFactory _shipmentViewModelFactory;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IRelationRepository _relationRepository;
        private readonly UrlResolver _urlResolver;

        public FlagshipCartViewModelFactory(
            CatalogContentService catalogContentService,
            IContentLoader contentLoader,
            IPricingService pricingService,
            IOrderGroupCalculator orderGroupCalculator,
            ShipmentViewModelFactory shipmentViewModelFactory,
            ReferenceConverter referenceConverter,
            IRelationRepository relationRepository,
            UrlResolver urlResolver)
        {
            _catalogContentService = catalogContentService;
            _contentLoader = contentLoader;
            _pricingService = pricingService;
            _orderGroupCalculator = orderGroupCalculator;
            _shipmentViewModelFactory = shipmentViewModelFactory;
            _referenceConverter = referenceConverter;
            _relationRepository = relationRepository;
            _urlResolver = urlResolver;
        }

        public virtual Shared.FlagshipViewModels.Cart Create(ICart cart, System.Uri RequestUrl)
        {
            if (cart == null)
            {
                return new Shared.FlagshipViewModels.Cart
                {
                    Items = new List<Shared.FlagshipViewModels.CartItem>(),
                    Subtotal = new Shared.FlagshipViewModels.CurrencyValue(_pricingService.GetMoney(0)),
                    Tax = new Shared.FlagshipViewModels.CurrencyValue(_pricingService.GetMoney(0)),
                    Total = new Shared.FlagshipViewModels.CurrencyValue(_pricingService.GetMoney(0)),
                };
            }

            var items = GetCartLineItems(cart);
            return new Shared.FlagshipViewModels.Cart
            {
                Items = items.Select(item => {
                    return ParseLineItem(cart, item, item.GetEntryContent(_referenceConverter, _contentLoader), RequestUrl);
                }).ToList(),
                Subtotal = new Shared.FlagshipViewModels.CurrencyValue(_orderGroupCalculator.GetSubTotal(cart)),
                Tax = new Shared.FlagshipViewModels.CurrencyValue(_orderGroupCalculator.GetTaxTotal(cart)),
                Total = new Shared.FlagshipViewModels.CurrencyValue(_orderGroupCalculator.GetTotal(cart))
            };
        }

        private IEnumerable<ILineItem> GetCartLineItems(ICart cart)
        {
            return cart
                .GetAllLineItems()
                .Where(c => !ContentReference.IsNullOrEmpty(_referenceConverter.GetContentLink(c.Code)));
        }

        public virtual Shared.FlagshipViewModels.CartItem ParseLineItem(ICart cart, ILineItem lineItem, EntryContentBase entry, System.Uri RequestUrl)
        {
            var images = entry.GetAssets<IContentImage>(_contentLoader, _urlResolver);
            var price = _pricingService.GetDiscountPrice(lineItem.Code).UnitPrice;

            var viewModel = new Shared.FlagshipViewModels.CartItem
            {
                Available = _pricingService.GetPrice(entry.Code) != null,
                Handle = entry.GetUrl(_relationRepository, _urlResolver),

                Images = images.Select(relativeUrl => {
                    var uri = new System.Uri(RequestUrl, relativeUrl);
                    return new Shared.FlagshipViewModels.Image { Uri = uri.AbsoluteUri };
                }).ToList(),
                ItemId = lineItem.Code,
                OriginalPrice = new Shared.FlagshipViewModels.CurrencyValue(_pricingService.GetDefaultPrice(lineItem.Code).UnitPrice),
                Price = new Shared.FlagshipViewModels.CurrencyValue(price),
                ProductId = entry.GetUrl(_relationRepository, _urlResolver).Substring(1).Split('?')[0],
                Quantity = lineItem.Quantity,
                Title = entry.DisplayName,
                TotalPrice = new Shared.FlagshipViewModels.CurrencyValue(price * lineItem.Quantity)
            };

            var productLink = entry is VariationContent ?
                entry.GetParentProducts(_relationRepository).FirstOrDefault() :
                entry.ContentLink;

            FashionProduct product;
            if (_contentLoader.TryGet(productLink, out product))
            {
                viewModel.Brand = GetBrand(product);

                var variants = _catalogContentService.GetVariants<FashionVariant>(product).ToList();
                viewModel.Variants = variants.Select(CreateVariant).ToList();

                var variant = entry as FashionVariant;
                if (variant != null)
                {
                    viewModel.ItemId = variant.Code;
                    viewModel.Options = new List<Shared.FlagshipViewModels.Option>();

                    var sizeValues = new List<Shared.FlagshipViewModels.OptionValue>();
                    sizeValues.Add(new Shared.FlagshipViewModels.OptionValue
                    {
                        Name = variant.Size,
                        Value = variant.Size
                    });
                    viewModel.Options.Add(new Shared.FlagshipViewModels.Option
                    {
                        Id = "Size",
                        Name = "Size",
                        Values = sizeValues
                    });

                    var colorValues = new List<Shared.FlagshipViewModels.OptionValue>();
                    colorValues.Add(new Shared.FlagshipViewModels.OptionValue
                    {
                        Name = variant.Color,
                        Value = variant.Color
                    });
                    viewModel.Options.Add(new Shared.FlagshipViewModels.Option
                    {
                        Id = "Color",
                        Name = "Color",
                        Values = colorValues
                    });
                }
            }

            return viewModel;
        }

        protected virtual List<Shared.FlagshipViewModels.Option> CreateOptions(List<FashionVariant> variants)
        {
            return new List<Shared.FlagshipViewModels.Option> {
                new Shared.FlagshipViewModels.Option {
                    Id = "Color",
                    Name = "Color",
                    Values = variants
                        .Where(variant => variant.Size != null)
                        .GroupBy(variant => variant.Color)
                        .Select(group => new Shared.FlagshipViewModels.OptionValue {
                            Name = group.Key,
                            Value = group.Key
                        })
                        .ToList()
                },
                new Shared.FlagshipViewModels.Option
                {
                    Id = "Size",
                    Name = "Size",
                    Values = variants
                        .Where(variant => variant.Color != null)
                        .GroupBy(variant => variant.Size)
                        .Select(group => new Shared.FlagshipViewModels.OptionValue {
                            Name = group.Key,
                            Value = group.Key
                        })
                        .ToList()
                }
            };
        }

        protected virtual Shared.FlagshipViewModels.Variant CreateVariant(FashionVariant variant)
        {
            var id = variant.Code;

            return new Shared.FlagshipViewModels.Variant
            {
                Id = id,
                OptionValues = new List<Shared.FlagshipViewModels.OptionValue>() {
                    new Shared.FlagshipViewModels.OptionValue
                    {
                        Name = "Color",
                        Value = variant.Color
                    },
                    new Shared.FlagshipViewModels.OptionValue
                    {
                        Name = "Size",
                        Value = variant.Size
                    }
                },
                Price = new Shared.FlagshipViewModels.CurrencyValue(_pricingService.GetPrice(id).UnitPrice),
                OriginalPrice = new Shared.FlagshipViewModels.CurrencyValue(_pricingService.GetDefaultPrice(id).UnitPrice),
                Title = variant.DisplayName,
                Images = variant.GetAssets<IContentImage>(_contentLoader, _urlResolver).Select(uri => new Shared.FlagshipViewModels.Image { Uri = uri }).ToList(),
                Available = _pricingService.GetDefaultPrice(id) != null
            };
        }

        private string GetBrand(FashionProduct product)
        {
            return product?.Brand;
        }
    }
}