using EPiServer.Find.UnifiedSearch;
using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModels
{
    public class FindSearchPageViewModel : PageViewModel<SearchPage>
    {
        public FindSearchPageViewModel(SearchPage currentPage, string searchQuery, int page)
        {
            CurrentPage = currentPage;
            SearchQuery = searchQuery;
            Page = page;
        }
        public string SearchQuery { get; private set; }
        public int Page { get; set;  }
        public UnifiedSearchResults Results { get; set; }
    }
}