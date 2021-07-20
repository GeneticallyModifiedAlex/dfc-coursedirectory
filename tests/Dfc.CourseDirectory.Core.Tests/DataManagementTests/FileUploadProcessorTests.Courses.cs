﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public partial class FileUploadProcessorTests
    {
        [Fact]
        public async Task FindVenue_RowHasVenueIdHint_ReturnsVenueMatchingHint()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "xxx"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid(), venueIdHint: venue.VenueId);

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }
        
        [Fact]
        public async Task FindVenue_RefProvidedButDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "VENUE2"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatchedButNameProvidedAndDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1",
                VenueName = "another name"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatchedAndNameProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1",
                VenueName = "my venue"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }

        [Fact]
        public async Task FindVenue_NameProvidedButDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                VenueName = "another name"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_NameProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                VenueName = "my venue"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }

        [Fact]
        public async Task GetCourseUploadRowsRequiringRevalidation_MatchedVenueUpdatedSinceRowLastValidated_ReturnsRow()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "REF");

            var (courseUpload, rows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                user,
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learningAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = venue.ProviderVenueRef;
                        record.VenueId = venue.VenueId;
                    });
                });

            Clock.UtcNow += TimeSpan.FromHours(1);
            await TestData.UpdateVenue(venue.VenueId, updatedBy: user);

            // Act
            var result = await WithSqlQueryDispatcher(
                dispatcher => fileUploadProcessor.GetCourseUploadRowsRequiringRevalidation(dispatcher, courseUpload));

            // Assert
            Assert.Collection(
                result,
                row => Assert.Equal(rows[0].CourseRunId, row.CourseRunId));
        }

        [Fact]
        public async Task GetCourseUploadRowsRequiringRevalidation_MatchedVenueNotUpdatedSinceRowLastValidated_DoesNotReturnRow()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "REF");

            var (courseUpload, rows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                user,
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learningAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = venue.ProviderVenueRef;
                        record.VenueId = venue.VenueId;
                    });
                });

            // Act
            var result = await WithSqlQueryDispatcher(
                dispatcher => fileUploadProcessor.GetCourseUploadRowsRequiringRevalidation(dispatcher, courseUpload));

            // Assert
            Assert.Collection(result);
        }

        [Fact]
        public async Task GetCourseUploadRowsRequiringRevalidation_MatchedVenueNotUpdatedSinceRowLastValidatedButOtherVenuesAmended_DoesNotReturnRow()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "REF");

            var (courseUpload, rows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                user,
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learningAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = venue.ProviderVenueRef;
                        record.VenueId = venue.VenueId;
                    });
                });

            Clock.UtcNow += TimeSpan.FromHours(1);
            await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Another Venue");

            // Act
            var result = await WithSqlQueryDispatcher(
                dispatcher => fileUploadProcessor.GetCourseUploadRowsRequiringRevalidation(dispatcher, courseUpload));

            // Assert
            Assert.Collection(result);
        }

        [Fact]
        public async Task GetCourseUploadRowsRequiringRevalidation_NoMatchedVenueAndProviderVenuesAmendedSinceRowLastValidated_ReturnsRow()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, rows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                user,
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learningAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = "REF";
                    });
                });

            Clock.UtcNow += TimeSpan.FromHours(1);
            await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Another Venue");

            // Act
            var result = await WithSqlQueryDispatcher(
                dispatcher => fileUploadProcessor.GetCourseUploadRowsRequiringRevalidation(dispatcher, courseUpload));

            // Assert
            Assert.Collection(
                result,
                row => Assert.Equal(rows[0].CourseRunId, row.CourseRunId));
        }

        [Fact]
        public async Task ProcessCourseFile_AllRecordsValid_SetStatusToProcessedSuccessfully()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learningAimRef = await TestData.CreateLearningAimRef();
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var uploadRows = DataManagementFileHelper.CreateCourseUploadRows(learningAimRef, rowCount: 3).ToArray();
            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedSuccessfully);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessCourseFile_RowHasErrors_SetStatusToProcessedWithErrors()
        {
            // Arrange
            var blobServiceClient = new Mock<BlobServiceClient>();
            blobServiceClient.Setup(mock => mock.GetBlobContainerClient(It.IsAny<string>())).Returns(Mock.Of<BlobContainerClient>());

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                blobServiceClient.Object,
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(
                // Empty record will always yield errors
                new CsvCourseRow());

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedWithErrors);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ValidateCourseUploadRows_RowsHaveNoLarsCode_AreNotGrouped()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, createdBy: user, null);
            var learningAimRef = await TestData.CreateLearningAimRef();

            var fileUploadProcessor = new FileUploadProcessor(
                SqlQueryDispatcherFactory,
                Mock.Of<BlobServiceClient>(),
                Clock,
                new RegionCache(SqlQueryDispatcherFactory));

            var rows = DataManagementFileHelper.CreateCourseUploadRows(learningAimRef, rowCount: 2).ToArray();
            rows[0].LearnAimRef = string.Empty;
            rows[1].LearnAimRef = string.Empty;

            var uploadRows = rows.ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                var (_, rows) = await fileUploadProcessor.ValidateCourseUploadRows(
                    dispatcher,
                    courseUpload.CourseUploadId,
                    provider.ProviderId,
                    uploadRows);

                // Assert
                rows.First().CourseId.Should().NotBe(rows.Last().CourseId);
            });
        }
    }
}