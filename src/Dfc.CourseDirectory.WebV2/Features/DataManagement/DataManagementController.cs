﻿using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement
{
    [RequireProviderContext]
    [RequireFeatureFlag(FeatureFlags.DataManagement)]
    [Route("data-upload")]
    public class DataManagementController : Controller
    {
        private readonly IMediator _mediator;

        public DataManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Dashboard() =>
            await _mediator.SendAndMapResponse(new Venues.Dashboard.Query(), vm => View(vm));
    }
}
