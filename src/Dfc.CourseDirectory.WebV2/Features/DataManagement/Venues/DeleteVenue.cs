﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation.Results;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.Delete
{
    public class Query : IRequest<OneOf<NotFound, Response>>
    {
        public int Row { get; set; }
    }

    public enum DeleteVenueResult
    {
        VenueDeletedUploadHasNoMoreErrors = 1,
        VenueDeletedUploadHasMoreErrors = 2
    }

    public class ViewModel
    {
        public string YourRef { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; }
        public List<string> Errors { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Response>, NotFound, DeleteVenueResult>>
    {
        public bool Confirm { get; set; }
        public Guid VenueUploadId { get; set; }
        public int Row { get; set; }
    }


    public class Response
    {
        public int Row { get; set; }
        public string YourRef { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; }
        public string Errors { get; set; }
        public bool Confirm { get; set; }
        public Guid? VenueUploadId { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<NotFound, Response>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Response>, NotFound, DeleteVenueResult>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public Handler(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IFileUploadProcessor fileUploadProcessor)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _fileUploadProcessor = fileUploadProcessor;
        }


        public async Task<OneOf<NotFound, Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();
            var venueUpload = await _sqlQueryDispatcher.ExecuteQuery(new GetLatestUnpublishedVenueUploadForProvider()
            {
                ProviderId = providerId
            });

            if (venueUpload == null)
            {
                return new NotFound();
            }

            var (venueUploadRows, _) = await _sqlQueryDispatcher.ExecuteQuery(new GetVenueUploadRows()
            {
                VenueUploadId = venueUpload.VenueUploadId
            });

            var row = venueUploadRows.FirstOrDefault(x => x.RowNumber == request.Row);
            if (row == null)
            {
                return new NotFound();
            }

            return new Response
            {
                Row = row.RowNumber,
                YourRef = row.ProviderVenueRef,
                VenueName = row.VenueName,
                Errors = GetUniqueErrorMessages(row),
                Address = FormatAddress(row),
                VenueUploadId = venueUpload.VenueUploadId
            };
        }

        private string FormatAddress(VenueUploadRow row)
        {
            var addressParts = new List<string> { row.AddressLine1, row.AddressLine2, row.County, row.Postcode };
            var address = addressParts.Where(p => !string.IsNullOrEmpty(p)).ToList();
            return string.Join(",", address);
        }

        private string GetUniqueErrorMessages(VenueUploadRow row)
        {
            var errors = row.Errors.Select(errorCode => Core.DataManagement.Errors.MapVenueErrorToFieldGroup(errorCode));
            return string.Join(",", errors.Distinct().ToList());
        }

        public async Task<OneOf<ModelWithErrors<Response>, NotFound, DeleteVenueResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            var (venueUploadRows, _) = await _sqlQueryDispatcher.ExecuteQuery(new GetVenueUploadRows()
            {
                VenueUploadId = request.VenueUploadId
            });

            var row = venueUploadRows.FirstOrDefault(x => x.RowNumber == request.Row);
            if (row == null)
            {
                return new NotFound();
            }

            if (!request.Confirm)
            {
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete this venue")
                });
                return new ModelWithErrors<Response>(new Response()
                {
                    Row = row.RowNumber,
                    YourRef = row.ProviderVenueRef,
                    VenueName = row.VenueName,
                    Errors = GetUniqueErrorMessages(row),
                    Address = FormatAddress(row),
                    VenueUploadId = request.VenueUploadId
                }, validationResult);
            }

            var deleted = await _fileUploadProcessor.DeleteVenueUploadRow(request.VenueUploadId, request.Row);
            if (!deleted)
                return new NotFound();

            var (existingRows, lastRowNumber) = await _sqlQueryDispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = request.VenueUploadId });
            if (existingRows.Any(x => x.Errors.Count > 0))
                return DeleteVenueResult.VenueDeletedUploadHasMoreErrors;
            else
                return DeleteVenueResult.VenueDeletedUploadHasNoMoreErrors;
        }
    }
}
