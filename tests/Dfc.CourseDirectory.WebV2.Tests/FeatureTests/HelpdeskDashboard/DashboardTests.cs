using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.HelpdeskDashboard
{
    public class DashboardTests : MvcTestBase
    {
        public DashboardTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(TestUserType.ProviderSuperUser)]
        [InlineData(TestUserType.ProviderUser)]
        public async Task Get_ProviderUserCannotAccess(TestUserType userType)
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            await User.AsTestUser(userType, provider.ProviderId);

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Theory]
        [InlineData(TestUserType.Developer)]
        [InlineData(TestUserType.Helpdesk)]
        public async Task Get_AdminsCanAccess(TestUserType userType)
        {
            // Arrange
            await User.AsTestUser(userType);

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task Get_RendersDownloadProviderTypeReportLink()
        {
            // Arrange
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            var downloadProviderTypeReportLink = doc.GetElementByTestId("download-provider-type-report-link");

            downloadProviderTypeReportLink.Should().NotBeNull();
            downloadProviderTypeReportLink.TextContent.Should().Be("Provider type report");
            downloadProviderTypeReportLink.Attributes["href"].Value.Should().Be("/providers/reports/provider-type");
        }

        [Fact]
        public async Task Get_RendersDownloadProvidersMissingContactDetailsReportLink()
        {
            // Arrange
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            var downloadProviderTypeReportLink = doc.GetElementByTestId("download-providers-missing-contact-details-link");

            downloadProviderTypeReportLink.Should().NotBeNull();
            downloadProviderTypeReportLink.TextContent.Should().Be("Providers missing contact details");
            downloadProviderTypeReportLink.Attributes["href"].Value.Should().Be("/providers/reports/providers-missing-primary-contact");
        }

        [Fact]
        public async Task Get_RendersDownloadLiveTLevelsReportLink()
        {
            // Arrange
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            var downloadLiveTLevelsReportLink = doc.GetElementByTestId("download-live-tlevels-report-link");

            downloadLiveTLevelsReportLink.Should().NotBeNull();
            downloadLiveTLevelsReportLink.TextContent.Should().Be("Live T Levels");
            downloadLiveTLevelsReportLink.Attributes["href"].Value.Should().Be("/t-levels/reports/live-t-levels");
        }

        [Fact]
        public async Task Get_RendersOutOfDateCoursesReportLink()
        {
            // Arrange
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            var downloadOutOfDateCoursesReportLink = doc.GetElementByTestId("download-provider-outofdatecourses-report-link");

            downloadOutOfDateCoursesReportLink.Should().NotBeNull();
            downloadOutOfDateCoursesReportLink.TextContent.Should().Be("Out of date courses report");
            downloadOutOfDateCoursesReportLink.Attributes["href"].Value.Should().Be("/providers/reports/out-of-date-courses");
        }


        [Fact]
        public async Task Get_RendersOpenDataCoursesReportLink()
        {
            // Arrange
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            var downloadOutOfDateCoursesReportLink = doc.GetElementByTestId("download-opendata-live-courses-report-link");

            downloadOutOfDateCoursesReportLink.Should().NotBeNull();
            downloadOutOfDateCoursesReportLink.TextContent.Should().Be("Live courses report");
        }

        [Fact]
        public async Task Get_RendersOpenDataCourseProvidersReportLink()
        {
            // Arrange
            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync("helpdesk-dashboard");

            // Assert
            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var doc = await response.GetDocument();
            var downloadOutOfDateCoursesReportLink = doc.GetElementByTestId("download-opendata-live-courseproviders-report-link");

            downloadOutOfDateCoursesReportLink.Should().NotBeNull();
            downloadOutOfDateCoursesReportLink.TextContent.Should().Be("Live course providers report");
        }
    }
}
