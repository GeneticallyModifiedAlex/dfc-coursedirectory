﻿using Dfc.CourseDirectory.WebV2.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.ApprenticeshipQA
{
    public class QAStatusReportTests : TestBase
    {
        public QAStatusReportTests(CourseDirectoryApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Get_QAReport_Returns_Returns_No_Results()
        {
            // Arrange
            // nothing to arrange

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results = await response.AsCsvListOf<QAStatusReport>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(results);
        }


        [Fact]
        public async Task Get_QAReport_For_Passed_Assessment()
        {
            //arange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);
            var requestedOn = Clock.UtcNow;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);

            await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            //create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            //update qa submission
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);


            //create provider assessment
            Clock.UtcNow = Clock.UtcNow.AddDays(1);
            var PassedQAOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                adminUserId,
                Clock.UtcNow,
                true,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.None,
                true, //Style passed,
                null,   //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.None
                );

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results = await response.AsCsvListOf<QAStatusReport>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Collection(results, item =>
            {
                Assert.Equal(providerId, item.ProviderId);
                Assert.Equal(ukprn.ToString(), item.UKPRN);
                Assert.Equal(providerName, item.ProviderName);
                Assert.Equal(email, item.Email);
                Assert.True(item.PassedQA);
                Assert.Equal(PassedQAOn, item.PassedQAOn);
                Assert.False(item.FailedQA);
                Assert.Null(item.FailedQAOn);
                Assert.False(item.UnableToComplete);
                Assert.Null(item.UnableToCompleteOn);
                Assert.Equal("", item.Notes);
                Assert.Null(item.UnableToCompleteReasons);
                Assert.Equal(ApprenticeshipQAStatus.Passed, item.QAStatus);
            });
        }

        [Fact]
        public async Task Get_QAReport_Returns_Most_Recent_Assessment()
        {
            //arange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);
            var requestedOn = Clock.UtcNow;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Failed);

            await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            //create submission1
            var submissionId1 = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            //create provider assessment1
            Clock.UtcNow = Clock.UtcNow.AddDays(2);
            var FailedQAON = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId1,
                adminUserId,
                Clock.UtcNow,
                false,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.UnverifiableClaim,
                false, //Style passed,
                null,   //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.TermFrameworkUsed
                );


            //create submission2
            Clock.UtcNow = Clock.UtcNow.AddDays(50);
            requestedOn = Clock.UtcNow;
            var submissionId2 = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            //create provider assessment1
            Clock.UtcNow = Clock.UtcNow.AddDays(10);
            var PassedQAOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId2,
                adminUserId,
                Clock.UtcNow,
                true,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.None,
                true, //Style passed,
                null,   //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.None
                );

            //update qa submission2
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId2,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: true,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            //set provider to be passed now
            await TestData.SetProviderApprenticeshipQAStatus(providerId, ApprenticeshipQAStatus.Passed);

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results = await response.AsCsvListOf<QAStatusReport>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Collection(results, item =>
            {
                Assert.Equal(providerId, item.ProviderId);
                Assert.Equal(ukprn.ToString(), item.UKPRN);
                Assert.Equal(providerName, item.ProviderName);
                Assert.Equal(email, item.Email);
                Assert.True(item.PassedQA);
                Assert.Equal(PassedQAOn, item.PassedQAOn);
                Assert.False(item.FailedQA);
                Assert.Null(item.FailedQAOn);
                Assert.False(item.UnableToComplete);
                Assert.Null(item.UnableToCompleteOn);
                Assert.Equal("", item.Notes);
                Assert.Null(item.UnableToCompleteReasons);
                Assert.Equal(ApprenticeshipQAStatus.Passed, item.QAStatus);
            });
        }

        [Fact]
        public async Task Get_QAReport_For_Failed_Assessment()
        {
            //arange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);
            var requestedOn = Clock.UtcNow;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Failed);

            await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            //create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            //update qa submission
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: false,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);


            //create provider assessment
            Clock.UtcNow = Clock.UtcNow.AddDays(2);
            var FailedQAON = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId,
                adminUserId,
                Clock.UtcNow,
                false,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.UnverifiableClaim,
                false, //Style passed,
                null,   //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.TermFrameworkUsed
                );

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results = await response.AsCsvListOf<QAStatusReport>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Collection(results, item =>
            {
                Assert.Equal(providerId, item.ProviderId);
                Assert.Equal(ukprn.ToString(), item.UKPRN);
                Assert.Equal(providerName, item.ProviderName);
                Assert.Equal(email, item.Email);
                Assert.False(item.PassedQA);
                Assert.Null(item.PassedQAOn);
                Assert.True(item.FailedQA);
                Assert.Equal(FailedQAON, item.FailedQAOn);
                Assert.False(item.UnableToComplete);
                Assert.Null(item.UnableToCompleteOn);
                Assert.Equal("", item.Notes);
                Assert.Null(item.UnableToCompleteReasons);
                Assert.Equal(ApprenticeshipQAStatus.Failed, item.QAStatus);
            });
        }

        [Theory]
        [InlineData(ApprenticeshipQAUnableToCompleteReasons.ProviderDevelopingProvision)]
        [InlineData(ApprenticeshipQAUnableToCompleteReasons.ProviderHasAppliedToTheWrongRoute)]
        [InlineData(ApprenticeshipQAUnableToCompleteReasons.ProviderHasWithdrawnApplication)]
        [InlineData(ApprenticeshipQAUnableToCompleteReasons.StandardNotReady)]
        [InlineData(ApprenticeshipQAUnableToCompleteReasons.Other)]
        public async Task Get_QAReport_For_Unable_To_CompleteQA(ApprenticeshipQAUnableToCompleteReasons unableReason)
        {
            //arange
            var ukprn = 12345;
            var email = "somebody@provider1.com";
            var providerName = "Provider 1";
            var providerUserId = $"{ukprn}-user";
            var adminUserId = $"admin-user";
            Clock.UtcNow = new DateTime(2019, 5, 17, 9, 3, 27, DateTimeKind.Utc);
            var requestedOn = Clock.UtcNow;
            var TribalComments = "QA Cannot be completed because x";

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: providerName,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.UnableToComplete);

            await TestData.CreateUser(providerUserId, email, "Provider 1", "Person", providerId);
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);
            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            //create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            //update qa submission
            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: false,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);


            //create provider assessment
            Clock.UtcNow = Clock.UtcNow.AddDays(5);
            var unableToCompleteOn = Clock.UtcNow;

            await TestData.CreateApprenticeshipQAUnableToCompleteInfo(
                providerId,
                unableReason,
                TribalComments,
                providerUserId,
                unableToCompleteOn);

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results = await response.AsCsvListOf<QAStatusReport>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Collection(results, item =>
            {
                Assert.Equal(providerId, item.ProviderId);
                Assert.Equal(ukprn.ToString(), item.UKPRN);
                Assert.Equal(providerName, item.ProviderName);
                Assert.Equal(email, item.Email);
                Assert.False(item.PassedQA);
                Assert.Null(item.PassedQAOn);
                Assert.False(item.FailedQA);
                Assert.Null(item.FailedQAOn);
                Assert.True(item.UnableToComplete);
                Assert.Equal(unableToCompleteOn, item.UnableToCompleteOn);
                Assert.Equal(TribalComments, item.Notes);
                Assert.Equal(unableReason, item.UnableToCompleteReasons);
                Assert.Equal(ApprenticeshipQAStatus.UnableToComplete, item.QAStatus);
            });
        }

        [Fact]
        public async Task Get_QAReport_Returns_MultipleProviders()
        {
            //arange
            var ukprn1 = 12345;
            var ukprn2 = 54321;
            var ukprn3 = 54322;
            var ukprn4 = 54324;
            var ukprn5 = 54325;
            var provider1Name = "test1";
            var provider2Name = "test2";
            var provider3Name = "test3";
            var provider4Name = "test4";
            var provider5Name = "test5";
            var providerUserId1 = "providerUserId1";
            var providerUserId2 = "providerUserId2";
            var providerUserId3 = "providerUserId3";
            var providerUserId4 = "providerUserId4";
            var providerUserId5 = "providerUserId5";
            var providerEmail1 = "provider1@provider1.com";
            var providerEmail2 = "provider2@provider2.com";
            var providerEmail3 = "provider3@provider3.com";
            var providerEmail4 = "provider4@provider4.com";
            var providerEmail5 = "provider5@provider5.com";
            var adminUserId = "adminUser";
            var provider5requestedOn = Clock.UtcNow;
            var provider5TribalComments = "QA Cannot be completed because x";
            var provider5ApprenticeshipQAUnableToCompleteReasons = ApprenticeshipQAUnableToCompleteReasons.ProviderHasWithdrawnApplication;

            var providerId1 = await TestData.CreateProvider(
                ukprn: ukprn1,
                providerName: provider1Name,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerId2 = await TestData.CreateProvider(
                ukprn: ukprn2,
                providerName: provider2Name,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.NotStarted);

            var providerId3 = await TestData.CreateProvider(
                ukprn: ukprn3,
                providerName: provider3Name,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Passed);

            var providerId4 = await TestData.CreateProvider(
                ukprn: ukprn4,
                providerName: provider4Name,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Failed);

            var providerId5 = await TestData.CreateProvider(
                ukprn: ukprn5,
                providerName: provider5Name,
                apprenticeshipQAStatus: ApprenticeshipQAStatus.UnableToComplete);

            //create multiple users for each provider
            await TestData.CreateUser(providerUserId1, providerEmail1, "Provider 1", "Person1", providerId1);
            await TestData.CreateUser(providerUserId2, providerEmail2, "Provider 2", "Person2", providerId2);
            await TestData.CreateUser(providerUserId3, providerEmail3, "Provider 3", "Person3", providerId3);
            await TestData.CreateUser(providerUserId4, providerEmail4, "Provider 4", "Person4", providerId4);
            await TestData.CreateUser(providerUserId5, providerEmail5, "Provider 5", "Person5", providerId5);

            //create single admin
            await TestData.CreateUser(adminUserId, "admin", "admin", "admin", null);

            var apprenticeshipId1 = await TestData.CreateApprenticeship(ukprn1);
            var apprenticeshipId2 = await TestData.CreateApprenticeship(ukprn2);
            var apprenticeshipId3 = await TestData.CreateApprenticeship(ukprn3);
            var apprenticeshipId4 = await TestData.CreateApprenticeship(ukprn4);
            var apprenticeshipId5 = await TestData.CreateApprenticeship(ukprn5);

            //provider1 Submission - Submitted
            var submissionId1 = await TestData.CreateApprenticeshipQASubmission(
                providerId1,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId1,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId1 });

            //provider3 Submission - Passed
            var provider3SubmittedOn = Clock.UtcNow.AddDays(3);
            Clock.UtcNow = provider3SubmittedOn;
            var submissionId3 = await TestData.CreateApprenticeshipQASubmission(
                providerId3,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId3,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId3 });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId3,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);


            //create provider assessment
            var provider3AssessOn = Clock.UtcNow.AddDays(1);
            Clock.UtcNow = provider3AssessOn;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId3,
                adminUserId,
                Clock.UtcNow,
                true,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.None,
                true,  //Style passed,
                null,  //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.None
                );

            //provider 4 - Failed
            var submissionId4 = await TestData.CreateApprenticeshipQASubmission(
                providerId4,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId4,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId4 });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId4,
                providerAssessmentPassed: false,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            Clock.UtcNow = Clock.UtcNow.AddDays(2);
            var provider4FailedOn = Clock.UtcNow;
            await TestData.CreateApprenticeshipQAProviderAssessment(
                submissionId4,
                adminUserId,
                Clock.UtcNow,
                false,
                null,   //Compliance Comments
                ApprenticeshipQAProviderComplianceFailedReasons.UnverifiableClaim,
                false, //Style passed,
                null,   //Style Comments
                ApprenticeshipQAProviderStyleFailedReasons.TermFrameworkUsed
                );

            // provider 5 - unable to complete
            var submissionId5 = await TestData.CreateApprenticeshipQASubmission(
                providerId5,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId5,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId5 });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId5,
                providerAssessmentPassed: false,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            Clock.UtcNow = Clock.UtcNow.AddDays(5);
            var provider5unableToCompleteOn = Clock.UtcNow;

            await TestData.CreateApprenticeshipQAUnableToCompleteInfo(
                providerId5,
                provider5ApprenticeshipQAUnableToCompleteReasons,
                provider5TribalComments,
                providerUserId5,
                provider5unableToCompleteOn);


            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results = await response.AsCsvListOf<QAStatusReport>();

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Collection(results.OrderBy(x => x.UKPRN), item1 =>
            {
                Assert.Equal(providerId1, item1.ProviderId);
                Assert.Equal(ukprn1.ToString(), item1.UKPRN);
                Assert.Equal(provider1Name, item1.ProviderName);
                Assert.Equal(providerEmail1, item1.Email);
                Assert.False(item1.PassedQA);
                Assert.Null(item1.PassedQAOn);
                Assert.False(item1.FailedQA);
                Assert.Null(item1.FailedQAOn);
                Assert.False(item1.UnableToComplete);
                Assert.Null(item1.UnableToCompleteOn);
                Assert.Equal("", item1.Notes);
                Assert.Null(item1.UnableToCompleteReasons);
                Assert.Equal(ApprenticeshipQAStatus.Submitted, item1.QAStatus);
            },
            item2 =>
            {
                Assert.Equal(providerId3, item2.ProviderId);
                Assert.Equal(ukprn3.ToString(), item2.UKPRN);
                Assert.Equal(provider3Name, item2.ProviderName);
                Assert.Equal(providerEmail3, item2.Email);
                Assert.True(item2.PassedQA);
                Assert.Equal(provider3AssessOn, item2.PassedQAOn);
                Assert.False(item2.FailedQA);
                Assert.Null(item2.FailedQAOn);
                Assert.False(item2.UnableToComplete);
                Assert.Null(item2.UnableToCompleteOn);
                Assert.Equal("", item2.Notes);
                Assert.Null(item2.UnableToCompleteReasons);
                Assert.Equal(ApprenticeshipQAStatus.Passed, item2.QAStatus);
            },
            item3 =>
            {
                Assert.Equal(providerId4, item3.ProviderId);
                Assert.Equal(ukprn4.ToString(), item3.UKPRN);
                Assert.Equal(provider4Name, item3.ProviderName);
                Assert.Equal(providerEmail4, item3.Email);
                Assert.False(item3.PassedQA);
                Assert.Null(item3.PassedQAOn);
                Assert.True(item3.FailedQA);
                Assert.Equal(provider4FailedOn, item3.FailedQAOn);
                Assert.False(item3.UnableToComplete);
                Assert.Null(item3.UnableToCompleteOn);
                Assert.Equal("", item3.Notes);
                Assert.Null(item3.UnableToCompleteReasons);
                Assert.Equal(ApprenticeshipQAStatus.Failed, item3.QAStatus);
            },
            item4 =>
            {
                Assert.Equal(providerId5, item4.ProviderId);
                Assert.Equal(ukprn5.ToString(), item4.UKPRN);
                Assert.Equal(provider5Name, item4.ProviderName);
                Assert.Equal(providerEmail5, item4.Email);
                Assert.False(item4.PassedQA);
                Assert.Null(item4.PassedQAOn);
                Assert.False(item4.FailedQA);
                Assert.Null(item4.FailedQAOn);
                Assert.True(item4.UnableToComplete);
                Assert.Equal(provider5unableToCompleteOn, item4.UnableToCompleteOn);
                Assert.Equal(provider5TribalComments, item4.Notes);
                Assert.Equal(provider5ApprenticeshipQAUnableToCompleteReasons, item4.UnableToCompleteReasons);
                Assert.Equal(ApprenticeshipQAStatus.UnableToComplete, item4.QAStatus);
            });
        }

        [Fact]
        public async Task Get_QAReport_Returns_One_Result()
        {
            var ukprn = 12345;

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);

            var apprenticeshipId = await TestData.CreateApprenticeship(ukprn);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await TestData.UpdateApprenticeshipQASubmission(
                submissionId,
                providerAssessmentPassed: true,
                apprenticeshipAssessmentsPassed: null,
                passed: null,
                lastAssessedByUserId: User.UserId.ToString(),
                lastAssessedOn: Clock.UtcNow);

            await User.AsHelpdesk();

            // Act
            var response = await HttpClient.GetAsync($"apprenticeship-qa/qareport");
            var results = await response.AsCsvListOf<QAStatusReport>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Collection(results, item =>
            {
                Assert.Equal(item.ProviderId, providerId);
            });
        }
    }
}