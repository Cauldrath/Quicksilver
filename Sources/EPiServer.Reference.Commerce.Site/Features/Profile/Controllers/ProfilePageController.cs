using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Profile.Pages;
using EPiServer.Reference.Commerce.Site.Features.Profile.ViewModels;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Profile.Controllers
{
    [Authorize]
    public class ProfilePageController : PageController<ProfilePage>
    {
        private readonly OptinService _optinService;
        private readonly IContentLoader _contentLoader;

        public ProfilePageController(OptinService optinService, IContentLoader contentLoader)
        {
            _optinService = optinService;
            _contentLoader = contentLoader;
        }

        public ActionResult Index(ProfilePage currentPage)
        {
            System.Reflection.PropertyInfo[] props = currentPage.GetType().GetProperties();
            System.Collections.Generic.Dictionary<string, object> cmsValues = new System.Collections.Generic.Dictionary<string, object>();
            foreach (System.Reflection.PropertyInfo property in props)
            {
                System.Attribute[] customAttrs = System.Attribute.GetCustomAttributes(property);
                foreach(System.Attribute customAttr in customAttrs)
                {
                    if (customAttr.GetType().Name == "DisplayAttribute")
                    {
                        cmsValues[property.Name] = currentPage[property.Name];
                    }
                }
            }
            var viewModel = CreateViewModel(currentPage);
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult EditForm(ProfilePage currentPage)
        {
            var viewModel = CreateViewModel(currentPage);
            if (viewModel.ConsentData.ConsentUpdated == null)
            {
                return RedirectToAction("Index");
            }

            return View("EditOptins", viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult UpdateOptins(ProfilePageViewModel viewModel)
        {
            _optinService.UpdateOptinForCurrentContact(viewModel.ConsentData.AcceptMarketingEmail);

            return RedirectToAction("Index");
        }

        private ProfilePageViewModel CreateViewModel(ProfilePage profilePage)
        {
            return new ProfilePageViewModel
            {
                CurrentPage = profilePage,
                ConsentData = _optinService.GetCurrentContactConsentData()
            };
        }
    }
}