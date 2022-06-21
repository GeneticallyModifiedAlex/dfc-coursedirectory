﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation.Results;
using FormFlow;
using GovUk.Frontend.AspNetCore;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification.CheckAndPublish
{

    public class Query : IRequest<ViewModel>
    {
        public CourseDeliveryMode DeliveryMode { get; set; }

    }

    public class ViewModel
    {
        public CourseDeliveryMode? DeliveryMode { get; set; }
        public string Delivery { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public string StartDate { get; set; }
        public bool? NationalDelivery { get; set; }        
        public string CourseWebPage { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public int? Duration { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public string StudyMode { get; set; }
        public string AttendancePattern { get; set; }
        public Guid? VenueId { get; set; }
        public string WhoThisCourseIsFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYouWillLearn { get; set; }
        public string HowYouWillLearn { get; set; }
        public string WhatYouWillNeedToBring { get; set; }
        public string HowYouWillBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public IEnumerable<string> SubRegionIds { get; set; }
        public string VenueName { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ProviderId { get; set; }
    }
    public class ViewModelProviderVenuesItem
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>
        //IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly IProviderContextProvider _providerContextProvider;

        public Handler(
            MptxInstanceContext<FlowModel> flow,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            IProviderContextProvider providerContextProvider)
        {
            _flow = flow;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
            _providerContextProvider = providerContextProvider;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            return await CreateViewModel();
           
        }

        //public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
        //    Command request,
        //    CancellationToken cancellationToken)
        //{

        //}


        private async Task<ViewModel> CreateViewModel()
        {
            var providerVenues = _flow.State.DeliveryMode == CourseDeliveryMode.ClassroomBased ?
                (await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = _providerContextProvider.GetProviderId() }))
                    .Select(v => new ViewModelProviderVenuesItem()
                    {
                        VenueId = v.VenueId,
                        VenueName = v.VenueName
                    })
                    .OrderBy(v => v.VenueName)
                    .ToArray() :
                null;
            var VenueName = "";
            if (providerVenues != null)
            {
                foreach (var venue in providerVenues)
                {
                    if (venue.VenueId == _flow.State.VenueId)
                        VenueName = venue.VenueName;
                }
            }


            var StartDate = _flow.State.FlexibleStartDate == true ? "Flexible" : _flow.State.StartDate.ToString();
            
            var Delivery = "";
            if (_flow.State.DeliveryMode == CourseDeliveryMode.ClassroomBased)
            {
                Delivery = "Classroom based";
            }
            else if (_flow.State.DeliveryMode == CourseDeliveryMode.WorkBased)
            {
                Delivery = "Work based";
            }
            else
            {
                Delivery = "Online";
            }
            //if ((bool)_flow.State.NationalDelivery)
            //{ var regions = "National provider"; }
            //foreach()

                return new ViewModel()
            {
                DeliveryMode = _flow.State.DeliveryMode,
                Delivery = Delivery,
                CourseName = _flow.State.CourseName,
                ProviderCourseRef = _flow.State.ProviderCourseRef,
                StartDate = /*(Date)_flow.State.*/StartDate/*.ToDateTime()*/,
                NationalDelivery = _flow.State.NationalDelivery,
                CourseWebPage = _flow.State.CourseWebPage,
                Cost = _flow.State.Cost,
                CostDescription = _flow.State.CostDescription,
                Duration = _flow.State.Duration,
                DurationUnit = _flow.State.DurationUnit,
                StudyMode = _flow.State.StudyMode.ToDescription(),
                AttendancePattern = _flow.State.AttendancePattern.ToDescription(),
                VenueId = _flow.State.VenueId,
                WhoThisCourseIsFor = _flow.State.WhoThisCourseIsFor,
                EntryRequirements = _flow.State.EntryRequirements,
                WhatYouWillLearn = _flow.State.WhatYouWillLearn,
                HowYouWillLearn = _flow.State.HowYouWillLearn,
                WhatYouWillNeedToBring = _flow.State.WhatYouWillNeedToBring,
                HowYouWillBeAssessed = _flow.State.HowYouWillBeAssessed,
                WhereNext = _flow.State.WhereNext,
                SubRegionIds =_flow.State.SubRegionIds,
                VenueName = VenueName
            };
        }

    }

}
