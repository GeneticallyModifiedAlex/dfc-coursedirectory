﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Auth;

namespace Dfc.CourseDirectory.Services.Interfaces.BulkUploadService
{
    public interface IApprenticeshipBulkUploadService
    {
        int CountCsvLines(Stream stream);
        Task<List<string>> ValidateAndUploadCSV(Stream stream, AuthUserDetails userDetails, bool updateApprenticeships);
    }
}
