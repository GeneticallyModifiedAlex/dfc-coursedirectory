﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Behaviors;
using FluentValidation;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;

namespace Dfc.CourseDirectory.WebV2.Features.EditVenue.Name
{
    public class Query : IRequest<Command>
    {
        public Guid VenueId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid VenueId { get; set; }
        public string Name { get; set; }
    }

    public class Handler :
        IRequireUserCanAccessVenue<Query>,
        IRequestHandler<Query, Command>,
        IRequireUserCanAccessVenue<Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly FormFlowInstance<EditVenueFlowModel> _formFlowInstance;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(
            FormFlowInstance<EditVenueFlowModel> formFlowInstance,
            IProviderOwnershipCache providerOwnershipCache,
            IProviderInfoCache providerInfoCache,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _formFlowInstance = formFlowInstance;
            _providerOwnershipCache = providerOwnershipCache;
            _providerInfoCache = providerInfoCache;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Command()
            {
                VenueId = request.VenueId,
                Name = _formFlowInstance.State.Name
            });
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var providerId = await _providerOwnershipCache.GetProviderForVenue(request.VenueId);
            var providerInfo = await _providerInfoCache.GetProviderInfo(providerId.Value);

            var validator = new CommandValidator(providerInfo.Ukprn, request.VenueId, _cosmosDbQueryDispatcher);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _formFlowInstance.UpdateState(state => state.Name = request.Name);

            return new Success();
        }

        Guid IRequireUserCanAccessVenue<Query>.GetVenueId(Query request) => request.VenueId;

        Guid IRequireUserCanAccessVenue<Command>.GetVenueId(Command request) => request.VenueId;

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(
                int providerUkprn,
                Guid venueId,
                ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
            {
                RuleFor(c => c.Name)
                    .VenueName(providerUkprn, venueId, cosmosDbQueryDispatcher);
            }
        }
    }
}
