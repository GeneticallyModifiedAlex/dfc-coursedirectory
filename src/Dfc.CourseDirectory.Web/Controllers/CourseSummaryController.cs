﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.CourseSummary;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class CourseSummaryController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public CourseSummaryController(
            ICourseService courseService,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            if (courseService == null)
            {
                throw new ArgumentNullException(nameof(courseService));
            }

            _courseService = courseService;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }
        public async Task<IActionResult> Index(Guid? courseId, Guid? courseRunId)
        {
            Course course = null;
            CourseRun courseRun = null;
            if (courseId.HasValue)
            {
                course = await _sqlQueryDispatcher.ExecuteQuery(new GetCourse() { CourseId = courseId.Value });
                courseRun = course.CourseRuns.Where(x => x.CourseRunId == courseRunId.Value).FirstOrDefault();
            }

            CourseSummaryViewModel vm = new CourseSummaryViewModel

            {
                ProviderUKPRN = course.ProviderUkprn,
                CourseId = course.CourseId,
                QualificationCourseTitle = course.LearnAimRefTitle,
                LearnAimRef = course.LearnAimRef,
                NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                AwardOrgCode = course.AwardOrgCode,
                CourseDescription = course.CourseDescription,
                EntryRequirements = course.EntryRequirements,
                WhatYoullLearn = course.WhatYoullLearn,
                HowYoullLearn = course.HowYoullLearn,
                WhatYoullNeed = course.WhatYoullNeed,
                HowYoullBeAssessed = course.HowYoullBeAssessed,
                WhereNext = course.WhereNext,
                IsValid = true,
                QualificationType = course.LearnAimRefTypeDesc,

                //Course run deets
                CourseInstanceId = courseRunId,
                CourseName = courseRun.CourseName,
                VenueId = courseRun.VenueId,
                Cost = courseRun.Cost,
                CostDescription = courseRun.CostDescription,
                DurationUnit = courseRun.DurationUnit,
                DurationValue = courseRun.DurationValue,
                ProviderCourseID = courseRun.ProviderCourseId,
                DeliveryMode = courseRun.DeliveryMode,
                National = courseRun.DeliveryMode == CourseDeliveryMode.WorkBased & !courseRun.National.HasValue ||
                           courseRun.National.GetValueOrDefault(),
                FlexibleStartDate = courseRun.FlexibleStartDate,
                StartDate = courseRun.StartDate,
                StudyMode = courseRun.StudyMode,
                AttendancePattern = courseRun.AttendancePattern,
                CreatedDate = courseRun.CreatedOn,

            };

            //Determine newer edited date
            if (course.UpdatedOn > courseRun.UpdatedOn)
            {
                vm.UpdatedDate = course.UpdatedOn;
            }
            else
            {
                vm.UpdatedDate = courseRun.UpdatedOn;
            }

            if (vm.VenueId != null)
            {
                if (vm.VenueId != Guid.Empty)
                {
                    var venue = await _sqlQueryDispatcher.ExecuteQuery(new GetVenue() { VenueId = courseRun.VenueId.Value });
                    vm.VenueName = venue?.VenueName;
                }
            }

            if(!string.IsNullOrEmpty(courseRun.CourseWebsite))
            {
                if (courseRun.CourseWebsite.Contains("http") || courseRun.CourseWebsite.Contains("https"))
                {
                    vm.CourseURL = courseRun.CourseWebsite;
                }
                else
                {
                    vm.CourseURL = "http://" + courseRun.CourseWebsite;
                }
            }

            if(courseRun.SubRegionIds?.Count > 0)
            {
                var allRegions = _courseService.GetRegions().RegionItems;
                var regions = GetRegions().RegionItems.Select(x => x.Id);
                vm.Regions = FormattedRegionsByIds(allRegions, courseRun.SubRegionIds);
            }

            //Generate Live service URL accordingly based on current host
            string host = HttpContext.Request.Host.ToString();
            ViewBag.LiveServiceURL = LiveServiceURLHelper.GetLiveServiceURLFromHost(host) +
                "find-a-course/course-details?CourseId=" + vm.CourseId + "&r=" + vm.CourseInstanceId;

            return View(vm);
        }
        internal IEnumerable<string> FormattedRegionsByIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            if (list == null) list = new List<RegionItemModel>();
            if (ids == null) ids = new List<string>();

            var regionNames = (from regionItemModel in list
                               from subRegionItemModel in regionItemModel.SubRegion
                               where ids.Contains(subRegionItemModel.Id) || ids.Contains(regionItemModel.Id)
                               select regionItemModel.RegionName).Distinct().OrderBy(x => x);

            return regionNames;
        }
        private SelectRegionModel GetRegions()
        {
            var selectRegion = new SelectRegionModel
            {
                LabelText = "Where in England can you deliver this course?",
                HintText = "Select all regions and areas that apply.",
                AriaDescribedBy = "Select all that apply."
            };

            if (selectRegion.RegionItems != null && selectRegion.RegionItems.Any())
            {
                selectRegion.RegionItems = selectRegion.RegionItems.OrderBy(x => x.RegionName);
                foreach (var selectRegionRegionItem in selectRegion.RegionItems)
                {
                    selectRegionRegionItem.SubRegion = selectRegionRegionItem.SubRegion.OrderBy(x => x.SubRegionName).ToList();
                }
            }

            return selectRegion;
        }
    }
}
