using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Models
{
    [CatalogContentType(
        GUID = "a23da2a1-7843-4828-9322-c63e28059f6a",
        MetaClassName = "FashionNode",
        DisplayName = "Fashion Node",
        Description = "Display fashion products.")]
    [AvailableContentTypes(Include = new[] 
    {
        typeof(FashionProduct),
        typeof(FashionPackage),
        typeof(FashionBundle),
        typeof(FashionVariant),
        typeof(NodeContent),
        typeof(IContent)
    })]
    public class FashionNode : NodeContent, IContent
    {
        [CultureSpecific]
        [Display(
            Name = "Page image",
            Description = "Link to image that will be displayed on the page.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        [UIHint(Web.UIHint.Image)]
        public virtual ContentReference Image { get; set; }
    }
}