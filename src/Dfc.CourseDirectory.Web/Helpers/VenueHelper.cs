﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public static class VenueHelper
    {
        public static async Task<Dictionary<Guid, string>> GetVenueNames(IEnumerable<Course> courses, IVenueService venueService )
        {
            Dictionary<Guid, string> venueNames = new Dictionary<Guid, string>();
            foreach (var course in courses)
            {
                foreach (var courseRun in course.CourseRuns)
                {
                    if (courseRun.VenueId != Guid.Empty && courseRun.VenueId != null)
                    {
                        if (!venueNames.ContainsKey((Guid)courseRun.VenueId))
                        {
                            var result = await venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(courseRun.VenueId.ToString()));
                            if (result.IsSuccess && result.HasValue)
                            {
                                Guid.TryParse(result.Value.ID, out Guid venueId);
                                venueNames.Add(venueId, result.Value.VenueName);
                            }
                        }
                    }
                }
            }
            return venueNames;
        }
    }
}
