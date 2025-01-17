﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Dfc.CourseDirectory.Core.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class LayoutTests : MvcTestBase
    {
        public LayoutTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task UnauthenticatedUser_DoesNotRenderSignOutLink()
        {
            // Arrange
            User.SetNotAuthenticated();
            var request = new HttpRequestMessage(HttpMethod.Get, "/tests/empty-provider-context");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.QuerySelectorAll(".pttcd-sign-out-navigation-item").Length.Should().Be(0);
        }

        [Fact]
        public async Task AuthenticatedUser_RendersSignOutLink()
        {
            // Arrange
            // Default test setup runs with an authenticated user
            var request = new HttpRequestMessage(HttpMethod.Get, "/tests/empty-provider-context");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            doc.QuerySelectorAll(".pttcd-sign-out-navigation-item").Length.Should().Be(1);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserWithoutProviderContext_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            await User.AsTestUser(testUserType);

            // Act
            var response = await HttpClient.GetAsync("/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            topLevelLinks.Count.Should().Be(4);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-helpdeskdashboard");
                topLevelLinks[1].TestId.Should().Be("topnav-searchproviders");
                topLevelLinks[2].TestId.Should().Be("topnav-manageusers");
                topLevelLinks[3].TestId.Should().Be("topnav-signout");
            }

            subNavLinks.Count.Should().Be(0);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserWithFEOnlyProviderContext_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context?providerId={provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            topLevelLinks.Count.Should().Be(4);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-helpdeskdashboard");
                topLevelLinks[1].TestId.Should().Be("topnav-searchproviders");
                topLevelLinks[2].TestId.Should().Be("topnav-manageusers");
                topLevelLinks[3].TestId.Should().Be("topnav-signout");
            }

            Assert.Equal(4, subNavLinks.Count);

            using (new AssertionScope())
            {
                subNavLinks[0].TestId.Should().Be("adminsubnav-home");
                subNavLinks[1].TestId.Should().Be("adminsubnav-courses");
                subNavLinks[2].TestId.Should().Be("adminsubnav-locations");
                subNavLinks[3].TestId.Should().Be("adminsubnav-datamanagement");
                subNavLinks[3].Href.Should().Be($"/data-upload?providerId={provider.ProviderId}");
            }
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserWithTLevelsOnlyProviderContext_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context?providerId={provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            topLevelLinks.Count.Should().Be(4);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-helpdeskdashboard");
                topLevelLinks[1].TestId.Should().Be("topnav-searchproviders");
                topLevelLinks[2].TestId.Should().Be("topnav-manageusers");
                topLevelLinks[3].TestId.Should().Be("topnav-signout");
            }

            Assert.Equal(4, subNavLinks.Count);

            using (new AssertionScope())
            {
                subNavLinks[0].TestId.Should().Be("adminsubnav-home");
                subNavLinks[1].TestId.Should().Be("adminsubnav-tlevels");
                subNavLinks[2].TestId.Should().Be("adminsubnav-locations");
            }
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task AdminUserWithFEAndTLevelsProviderContext_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE | ProviderType.TLevels,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context?providerId={provider.ProviderId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            topLevelLinks.Count.Should().Be(4);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-helpdeskdashboard");
                topLevelLinks[1].TestId.Should().Be("topnav-searchproviders");
                topLevelLinks[2].TestId.Should().Be("topnav-manageusers");
                topLevelLinks[3].TestId.Should().Be("topnav-signout");
            }

            Assert.Equal(5, subNavLinks.Count);

            using (new AssertionScope())
            {
                subNavLinks[0].TestId.Should().Be("adminsubnav-home");
                subNavLinks[1].TestId.Should().Be("adminsubnav-courses");
                subNavLinks[2].TestId.Should().Be("adminsubnav-tlevels");
                subNavLinks[3].TestId.Should().Be("adminsubnav-locations");
                subNavLinks[4].TestId.Should().Be("adminsubnav-datamanagement");
                subNavLinks[4].Href.Should().Be($"/data-upload?providerId={provider.ProviderId}");
            }
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUserForFEOnlyProvider_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            topLevelLinks.Count.Should().Be(5);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-courses");
                topLevelLinks[2].TestId.Should().Be("topnav-locations");
                topLevelLinks[3].TestId.Should().Be("topnav-datamanagement");
                topLevelLinks[3].Href.Should().Be($"/data-upload?providerId={provider.ProviderId}");
                topLevelLinks[4].TestId.Should().Be("topnav-signout");
            }

            subNavLinks.Count.Should().Be(0);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUserForTLevelsOnlyProvider_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.TLevels,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            topLevelLinks.Count.Should().Be(5);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-tlevels");
                topLevelLinks[2].TestId.Should().Be("topnav-locations");
                topLevelLinks[3].TestId.Should().Be("topnav-datamanagement");
                topLevelLinks[4].TestId.Should().Be("topnav-signout");
            }

            subNavLinks.Count.Should().Be(0);
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task ProviderUserForFEAndTLevels_RendersExpectedNav(TestUserType testUserType)
        {
            // Arrange
            var provider = await TestData.CreateProvider(
                providerType: ProviderType.FE | ProviderType.TLevels,
                providerName: "Test Provider");

            await User.AsTestUser(testUserType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            response.EnsureSuccessStatusCode();

            var doc = await response.GetDocument();
            var topLevelLinks = GetTopLevelNavLinks(doc);
            var subNavLinks = GetSubNavLinks(doc);

            topLevelLinks.Count.Should().Be(6);

            using (new AssertionScope())
            {
                topLevelLinks[0].TestId.Should().Be("topnav-home");
                topLevelLinks[1].TestId.Should().Be("topnav-courses");
                topLevelLinks[2].TestId.Should().Be("topnav-tlevels");
                topLevelLinks[3].TestId.Should().Be("topnav-locations");
                topLevelLinks[4].TestId.Should().Be("topnav-datamanagement");
                topLevelLinks[4].Href.Should().Be($"/data-upload?providerId={provider.ProviderId}");
                topLevelLinks[5].TestId.Should().Be("topnav-signout");
            }

            subNavLinks.Count.Should().Be(0);
        }

        [Fact]
        public async Task NoCookiePreferencesSet_RendersCookieBanner()
        {
            // Arrange
            CookieSettingsProvider.SetPreferencesForCurrentUser(null);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            var doc = await response.GetDocument();
            doc.GetAllElementsByTestId("cookie-banner").Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CookiePreferencesSet_DoesNotRenderCookieBanner(bool allowAnalyticsCookies)
        {
            // Arrange
            CookieSettingsProvider.SetPreferencesForCurrentUser(new Cookies.CookieSettings()
            {
                AllowAnalyticsCookies = allowAnalyticsCookies
            });

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            var doc = await response.GetDocument();
            doc.GetElementByTestId("cookie-banner").Should().BeNull();
        }

        [Fact]
        public async Task AllCookiesAccepted_RendersConfirmation()
        {
            // Arrange
            CookieSettingsProvider.SetPreferencesForCurrentUser(null);

            // Act
            var response = await HttpClient.PostAsync("cookies/accept-all?returnUrl=/foo", null);

            // Assert
            var doc = await response.GetDocument();
            doc.GetAllElementsByTestId("cookie-banner-confirmation").Should().NotBeNull();
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public async Task RendersGATrackingCodeBasedOnUsersPreferences(
            bool? allowAnalyticsCookies,
            bool expectGATagsToBeRendered)
        {
            // Arrange
            Cookies.CookieSettings settings = null;
            if (allowAnalyticsCookies != null)
            {
                settings = new Cookies.CookieSettings() { AllowAnalyticsCookies = allowAnalyticsCookies.Value };
            }

            CookieSettingsProvider.SetPreferencesForCurrentUser(settings);

            // Act
            var response = await HttpClient.GetAsync($"/tests/empty-provider-context");

            // Assert
            var doc = await response.GetDocument();

            var gotGATags = doc.QuerySelectorAll("script")
                .Where(s => s.GetAttribute("src")?.StartsWith("https://www.google-analytics.com") == true)
                .Any();
            gotGATags.Should().Be(expectGATagsToBeRendered);
        }

        private IReadOnlyList<(string Href, string TestId)> GetTopLevelNavLinks(IHtmlDocument doc)
        {
            var results = new List<(string Href, string TestId)>();

            foreach (var item in doc.GetElementsByClassName("govuk-header__navigation-item"))
            {
                var anchor = item.GetElementsByTagName("a")[0];
                var href = anchor.GetAttribute("href");
                var testId = anchor.GetAttribute("data-testid");

                results.Add((href, testId));
            }

            return results;
        }

        private IReadOnlyList<(string Href, string TestId)> GetSubNavLinks(IHtmlDocument doc)
        {
            var results = new List<(string Href, string TestId)>();

            foreach (var item in doc.GetElementsByClassName("pttcd-subnav__item"))
            {
                var anchor = item.GetElementsByTagName("a")[0];
                var href = anchor.GetAttribute("href");
                var testId = anchor.GetAttribute("data-testid");

                results.Add((href, testId));
            }

            return results;
        }
    }
}
