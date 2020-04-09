﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Validation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ApprenticeshipLocations
{
    public class Query : IRequest<Command>, IProviderScopedRequest
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>, IProviderScopedRequest
    {
        public Guid ProviderId { get; set; }
        public Models.ApprenticeshipLocationType? LocationType { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequireUserCanSubmitQASubmission<Query>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>,
        IRequireUserCanSubmitQASubmission<Command>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;

        public Handler(MptxInstanceContext<FlowModel> flow)
        {
            _flow = flow;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            var command = new Command()
            {
                LocationType = _flow.State.ApprenticeshipLocationType,
                ProviderId = request.ProviderId
            };
            return Task.FromResult(command);
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _flow.Update(s => s.SetApprenticeshipLocationType(request.LocationType.Value));

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.LocationType)
                    .NotNull()
                    .WithMessage("Enter where this apprenticeship training will be delivered");
            }
        }
    }
}