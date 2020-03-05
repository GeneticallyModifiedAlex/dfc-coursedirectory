﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Query = Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries.CreateApprenticeshipQASubmission;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public Task<int> CreateApprenticeshipQASubmission(
            Guid providerId,
            DateTime submittedOn,
            string submittedByUserId,
            string providerBriefOverview,
            IEnumerable<Guid> apprenticeshipIds)
        {
            return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new Query()
            {
                ApprenticeshipIds = apprenticeshipIds,
                ProviderBriefOverview = providerBriefOverview,
                ProviderId = providerId,
                SubmittedByUserId = submittedByUserId,
                SubmittedOn = submittedOn
            }));
        }
    }
}