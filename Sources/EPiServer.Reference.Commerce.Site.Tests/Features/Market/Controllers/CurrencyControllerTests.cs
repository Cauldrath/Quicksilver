﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Market.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;
using Mediachase.Commerce;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market.Controllers
{
    [TestClass]
    public class CurrencyControllerTests
    {
        [TestMethod]
        public void Index_ShouldReturnCorrectTypes()
        {
            var result = _subject.Index();

            Assert.IsInstanceOfType(result, typeof(ViewResultBase));
            Assert.IsInstanceOfType((result as ViewResultBase).Model, typeof(CurrencyViewModel));
        }

        [TestMethod]
        public void Index_ShouldReturnCorrectModel()
        {
            _currencyServiceMock.Setup(x => x.GetCurrentCurrency()).Returns(new Currency("USD"));
            _currencyServiceMock.Setup(x => x.SetCurrentCurrency("USD")).Returns(true);
            _currencyServiceMock.Setup(x => x.GetAvailableCurrencies())
                .Returns(new[] { new Currency("USD"), new Currency("SEK") });

            var result = _subject.Index();

            var model = ((ViewResultBase)result).Model as CurrencyViewModel;

            Assert.AreEqual<Currency>(new Currency("USD"), model.CurrentCurrency);
            CollectionAssert.AreEquivalent(
                new []{ new Currency("USD"), new Currency("SEK") }, model.Currencies.ToList());
        }

        [TestMethod]
        public void Set_WhenInValidCurrency_ShouldReturnHttpError()
        {
            var result = _subject.Set("UNKNOWN CURRENCY");

            Assert.AreEqual<int>(400, ((HttpStatusCodeResult) result).StatusCode);
        }

        private Mock<ICurrencyService> _currencyServiceMock;
        private CurrencyController _subject;

        [TestInitialize]
        public void Setup()
        {
            _currencyServiceMock = new Mock<ICurrencyService>();
            _subject = new CurrencyController(_currencyServiceMock.Object);
        }
    }
}
