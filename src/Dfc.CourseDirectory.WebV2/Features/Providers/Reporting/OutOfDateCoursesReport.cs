using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.Reporting.OutOfDateCoursesReport
{
    public class Query : IRequest<IAsyncEnumerable<Csv>>
    {
    }

    public class Csv
    {
        [Name("UKPRN")]
        public string ProviderUKPRN { get; set; }

        [Name("ProviderName")]
        public string ProviderName { get; set; }

        [Name("CourseId")]
        public string CourseID { get; set; }

        [Name("CourseRunId")]
        public string CourseRunID { get; set; }

        [Name("CourseName")]
        public string CourseRunName { get; set; }

        [Name("StartDate")]
        public string StartDate { get; set; }
    }

    public class Handler : IRequestHandler<Query, IAsyncEnumerable<Csv>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public Task<IAsyncEnumerable<Csv>> Handle(Query request, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
