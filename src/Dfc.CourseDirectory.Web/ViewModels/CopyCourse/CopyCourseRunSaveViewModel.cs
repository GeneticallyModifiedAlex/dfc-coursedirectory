﻿using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models;
using Dfc.CourseDirectory.Models.Models.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewModels.CopyCourse
{
    public class CopyCourseRunSaveViewModel
    {
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public string StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string DurationLength { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }

        public Guid VenueId { get; set; }
        public string[] SelectedRegions { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public string CourseProviderReference { get; set; }
        public string Url { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendanceMode { get; set; }
        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }

        public string QualificationType { get; set; }

        public bool FlexibleStartDate { get; set; }

    }

}
