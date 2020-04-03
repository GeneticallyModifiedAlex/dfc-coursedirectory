﻿CREATE TABLE [Tribal].[Provider] (
    [ProviderId]                         INT            NOT NULL,
    [ProviderName]                       NVARCHAR (200) NOT NULL,
    [ProviderNameAlias]                  NVARCHAR (200) NULL,
    [Loans24Plus]                        BIT            NOT NULL,
    [Ukprn]                              INT            NOT NULL,
    [UPIN]                               INT            NULL,
    [ProviderTypeId]                     INT            NOT NULL,
    [RecordStatusId]                     INT            NOT NULL,
    [CreatedByUserId]                    NVARCHAR (128) NOT NULL,
    [CreatedDateTimeUtc]                 DATETIME       NOT NULL,
    [ModifiedByUserId]                   NVARCHAR (128) NULL,
    [ModifiedDateTimeUtc]                DATETIME       NULL,
    [ProviderRegionId]                   INT            NULL,
    [IsContractingBody]                  BIT            NOT NULL,
    [ProviderTrackingUrl]                NVARCHAR (255) NULL,
    [VenueTrackingUrl]                   NVARCHAR (255) NULL,
    [CourseTrackingUrl]                  NVARCHAR (255) NULL,
    [BookingTrackingUrl]                 NVARCHAR (255) NULL,
    [RelationshipManagerUserId]          NVARCHAR (128) NULL,
    [InformationOfficerUserId]           NVARCHAR (128) NULL,
    [AddressId]                          INT            NULL,
    [Email]                              NVARCHAR (255) NULL,
    [Website]                            NVARCHAR (255) NULL,
    [Telephone]                          NVARCHAR (30)  NULL,
    [Fax]                                NVARCHAR (30)  NULL,
    [FeChoicesLearner]                   DECIMAL (3, 1) NULL,
    [FeChoicesEmployer]                  DECIMAL (3, 1) NULL,
    [FeChoicesDestination]               INT            NULL,
    [FeChoicesUpdatedDateTimeUtc]        DATETIME       NULL,
    [QualityEmailsPaused]                BIT            NOT NULL,
    [QualityEmailStatusId]               INT            NULL,
    [TrafficLightEmailDateTimeUtc]       DATE           NULL,
    [DFE1619Funded]                      BIT            NOT NULL,
    [SFAFunded]                          BIT            NOT NULL,
    [DfENumber]                          INT            NULL,
    [DfEUrn]                             INT            NULL,
    [DfEProviderTypeId]                  INT            NULL,
    [DfEProviderStatusId]                INT            NULL,
    [DfELocalAuthorityId]                INT            NULL,
    [DfERegionId]                        INT            NULL,
    [DfEEstablishmentTypeId]             INT            NULL,
    [DfEEstablishmentNumber]             INT            NULL,
    [StatutoryLowestAge]                 INT            NULL,
    [StatutoryHighestAge]                INT            NULL,
    [AgeRange]                           VARCHAR (10)   NULL,
    [AnnualSchoolCensusLowestAge]        INT            NULL,
    [AnnualSchoolCensusHighestAge]       INT            NULL,
    [CompanyRegistrationNumber]          INT            NULL,
    [Uid]                                INT            NULL,
    [SecureAccessId]                     INT            NULL,
    [BulkUploadPending]                  BIT            NOT NULL,
    [PublishData]                        BIT            NOT NULL,
    [MarketingInformation]               NVARCHAR (900) NULL,
    [NationalApprenticeshipProvider]     BIT            NOT NULL,
    [ApprenticeshipContract]             BIT            NOT NULL,
    [PassedOverallQAChecks]              BIT            NOT NULL,
    [DataReadyToQA]                      BIT            NOT NULL,
    [RoATPFFlag]                         BIT            NOT NULL,
    [LastAllDataUpToDateTimeUtc]         DATETIME       NULL,
    [RoATPProviderTypeId]                INT            NULL,
    [RoATPStartDate]                     DATE           NULL,
    [MarketingInformationUpdatedDateUtc] DATETIME       NULL,
    [TradingName]                        NVARCHAR (255) NULL,
    [MaxLocations]                       INT            NULL,
    [MaxLocationsUserId]                 NVARCHAR (128) NULL,
    [MaxLocationsDateTimeUtc]            DATETIME       NULL,
    [TASRefreshOverride]                 BIT            NOT NULL,
    CONSTRAINT [PK_Provider] PRIMARY KEY CLUSTERED ([ProviderId] ASC)
);

