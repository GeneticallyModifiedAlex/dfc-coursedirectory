﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Xunit;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<(ApprenticeshipUpload ApprenticeshipUpload, ApprenticeshipUploadRow[] Rows)> CreateApprenticeshipUpload(
            Guid providerId,
            UserInfo createdBy,
            UploadStatus uploadStatus,
            Action<ApprenticeshipUploadRowBuilder> configureRows = null)
        {
            var createdOn = _clock.UtcNow;

            DateTime? processingStartedOn = uploadStatus >= UploadStatus.Processing ? createdOn.AddSeconds(3) : (DateTime?)null;
            DateTime? processingCompletedOn = uploadStatus >= UploadStatus.ProcessedWithErrors ? processingStartedOn.Value.AddSeconds(30) : (DateTime?)null;
            DateTime? publishedOn = uploadStatus == UploadStatus.Published ? processingCompletedOn.Value.AddHours(2) : (DateTime?)null;
            DateTime? abandonedOn = uploadStatus == UploadStatus.Abandoned ? processingCompletedOn.Value.AddHours(2) : (DateTime?)null;

            var isValid = uploadStatus switch
            {
                UploadStatus.ProcessedWithErrors => false,
                UploadStatus.Created | UploadStatus.Processing => (bool?)null,
                _ => true
            };

            var (courseUpload, rows) = await CreateApprenticeshipUpload(
                providerId,
                createdBy,
                createdOn,
                processingStartedOn,
                processingCompletedOn,
                publishedOn,
                abandonedOn,
                isValid,
                configureRows);

            Assert.Equal(uploadStatus, courseUpload.UploadStatus);

            return (courseUpload, rows);
        }

        public Task<(ApprenticeshipUpload ApprenticeshipUpload, ApprenticeshipUploadRow[] Rows)> CreateApprenticeshipUpload(
            Guid providerId,
            UserInfo createdBy,
            DateTime? createdOn = null,
            DateTime? processingStartedOn = null,
            DateTime? processingCompletedOn = null,
            DateTime? publishedOn = null,
            DateTime? abandonedOn = null,
            bool? isValid = null,
            Action<ApprenticeshipUploadRowBuilder> configureRows = null)
        {
            var apprenticeshipUploadId = Guid.NewGuid();
            createdOn ??= _clock.UtcNow;

            return WithSqlQueryDispatcher(async dispatcher =>
            {
                ApprenticeshipUploadRow[] rows = null;

                await dispatcher.ExecuteQuery(new CreateApprenticeshipUpload()
                {
                    ApprenticeshipUploadId = apprenticeshipUploadId,
                    ProviderId = providerId,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn.Value
                });

                if (processingStartedOn.HasValue)
                {
                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessing()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        ProcessingStartedOn = processingStartedOn.Value
                    });
                }

                if (processingCompletedOn.HasValue)
                {
                    if (!processingStartedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingStartedOn));
                    }

                    if (!isValid.HasValue)
                    {
                        throw new ArgumentNullException(nameof(isValid));
                    }

                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessed()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        ProcessingCompletedOn = processingCompletedOn.Value,
                        IsValid = isValid.Value
                    });

                    var allRegions = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new GetAllRegions()));
                    var rowBuilder = new ApprenticeshipUploadRowBuilder(allRegions);

                    if (configureRows != null)
                    {
                        configureRows(rowBuilder);
                    }
                    else
                    {
                        var standard = await CreateStandard();

                        if (isValid.Value)
                        {
                            rowBuilder.AddValidRow(standard.StandardCode, standard.Version);
                        }
                        else
                        {
                            rowBuilder.AddRow(standard.StandardCode, standard.Version, record =>
                            {
                                record.IsValid = false;
                                record.Errors = new[] { ErrorRegistry.All["APPRENTICESHIP_INFORMATION_REQUIRED"].ErrorCode };
                            });
                        }
                    }

                    rows = (await dispatcher.ExecuteQuery(new UpsertApprenticeshipUploadRows()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        Records = rowBuilder.GetUpsertQueryRows(),
                        UpdatedOn = processingCompletedOn.Value,
                        ValidatedOn = processingCompletedOn.Value
                    })).ToArray();
                }

                if (publishedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }

                    await dispatcher.ExecuteQuery(new PublishApprenticeshipUpload()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        PublishedBy = createdBy,
                        PublishedOn = publishedOn.Value
                    });
                }
                else if (abandonedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }

                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadAbandoned()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        AbandonedOn = abandonedOn.Value
                    });
                }

                var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetApprenticeshipUpload()
                {
                    ApprenticeshipUploadId = apprenticeshipUploadId
                });

                return (apprenticeshipUpload, rows);
            });
        }

        public class ApprenticeshipUploadRowBuilder
        {
            private readonly List<UpsertApprenticeshipUploadRowsRecord> _records = new List<UpsertApprenticeshipUploadRowsRecord>();
            private readonly IEnumerable<Region> _allRegions;

            public ApprenticeshipUploadRowBuilder(IEnumerable<Region> allRegions)
            {
                _allRegions = allRegions;
            }

            public ApprenticeshipUploadRowBuilder AddRow(int standardCode, int standardVersion, Action<UpsertApprenticeshipUploadRowsRecord> configureRecord)
            {
                var record = CreateValidRecord(standardCode, standardVersion);
                configureRecord(record);
                _records.Add(record);
                return this;
            }

            public ApprenticeshipUploadRowBuilder AddRow(
                Guid apprenticeshipId,
                Guid apprenticeshipLocationId,
                int standardCode,
                int standardVersion,
                string apprenticeshipInformation,
                string apprenticeshipWebpage,
                string contactEmail,
                string contactPhone,
                string contactUrl,
                string deliveryMethod,
                string venue,
                string yourVenueReference,
                string radius,
                string deliveryMode,
                string nationalDelivery,
                string subRegions,
                Guid? venueId,
                IEnumerable<string> errors = null)
            {
                var record = CreateRecord(
                    apprenticeshipId,
                    apprenticeshipLocationId,
                    standardCode,
                    standardVersion,
                    apprenticeshipInformation,
                    apprenticeshipWebpage,
                    contactEmail,
                    contactPhone,
                    contactUrl,
                    deliveryMethod,
                    venue,
                    yourVenueReference,
                    radius,
                    deliveryMode,
                    nationalDelivery,
                    subRegions,
                    venueId,
                    errors);

                _records.Add(record);

                return this;
            }

            public ApprenticeshipUploadRowBuilder AddValidRow(int standardCode, int standardVersion)
            {
                var record = CreateValidRecord(standardCode, standardVersion);
                _records.Add(record);
                return this;
            }

            internal IReadOnlyCollection<UpsertApprenticeshipUploadRowsRecord> GetUpsertQueryRows() => _records;

            private UpsertApprenticeshipUploadRowsRecord CreateRecord(
                Guid apprenticeshipId,
                Guid apprenticeshipLocationId,
                int standardCode,
                int standardVersion,
                string apprenticeshipInformation,
                string apprenticeshipWebpage,
                string contactEmail,
                string contactPhone,
                string contactUrl,
                string deliveryMethod,
                string venue,
                string yourVenueReference,
                string radius,
                string deliveryModes,
                string nationalDelivery,
                string subRegions,
                Guid? venueId,
                IEnumerable<string> errors = null)
            {
                var errorsArray = errors?.ToArray() ?? Array.Empty<string>();
                var isValid = !errorsArray.Any();

                var resolvedDeliveryModes = ParsedCsvApprenticeshipRow.ResolveDeliveryModes(deliveryModes);
                var resolvedDeliveryMethod = ParsedCsvApprenticeshipRow.ResolveDeliveryMethod(deliveryMethod, resolvedDeliveryModes);

                return new UpsertApprenticeshipUploadRowsRecord()
                {
                    RowNumber = _records.Count + 2,
                    IsValid = isValid,
                    ApprenticeshipId = apprenticeshipId,
                    ApprenticeshipLocationId = apprenticeshipLocationId,
                    StandardCode = standardCode,
                    StandardVersion = standardVersion,
                    ApprenticeshipInformation = apprenticeshipInformation,
                    ApprenticeshipWebpage = apprenticeshipWebpage,
                    ContactEmail = contactEmail,
                    ContactPhone = contactPhone,
                    ContactUrl = contactUrl,
                    DeliveryMethod = deliveryMethod,
                    VenueName = venue,
                    YourVenueReference = yourVenueReference,
                    Radius = radius,
                    DeliveryModes = deliveryModes,
                    NationalDelivery = nationalDelivery,
                    SubRegions = subRegions,
                    VenueId = venueId,
                    Errors = errors,
                    ResolvedDeliveryModes = resolvedDeliveryModes,
                    ResolvedDeliveryMethod = resolvedDeliveryMethod,
                    ResolvedNationalDelivery = ParsedCsvApprenticeshipRow.ResolveNationalDelivery(nationalDelivery, subRegions, resolvedDeliveryMethod),
                    ResolvedRadius = ParsedCsvApprenticeshipRow.ResolveRadius(radius),
                    ResolvedSubRegions = ParsedCsvApprenticeshipRow.ResolveSubRegions(subRegions, _allRegions)?.Select(r => r.Id)
                };
            }

            private UpsertApprenticeshipUploadRowsRecord CreateValidRecord(int standardCode, int standardVersion)
            {
                return CreateRecord(
                    apprenticeshipId: Guid.NewGuid(),
                    apprenticeshipLocationId: Guid.NewGuid(),
                    standardCode: standardCode,
                    standardVersion: standardVersion,
                    apprenticeshipInformation: "Some Apprenticeship Information",
                    apprenticeshipWebpage: "provider.com/apprenticeship",
                    contactEmail: "info@provider.com",
                    contactPhone: "01234 567890",
                    contactUrl: "provider.com",
                    deliveryModes: "employer",
                    venue: "",
                    yourVenueReference: "",
                    radius: "",
                    deliveryMethod: "employer based",
                    nationalDelivery: "yes",
                    subRegions: "",
                    venueId: null);
            }
        }
    }
}