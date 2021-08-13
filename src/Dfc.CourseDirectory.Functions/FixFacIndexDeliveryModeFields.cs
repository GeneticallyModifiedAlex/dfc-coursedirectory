using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class FixFacIndexDeliveryModeFields
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        public FixFacIndexDeliveryModeFields(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        [FunctionName(nameof(FixFacIndexDeliveryModeFields))]
        [NoAutomaticTrigger]
        public async Task Execute(string input)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var sql = @"
UPDATE Pttcd.FindACourseIndex SET [National] = 1 WHERE DeliveryMode = 2

UPDATE Pttcd.FindACourseIndex SET StudyMode = NULL WHERE DeliveryMode <> 1";

            await dispatcher.Transaction.Connection.ExecuteAsync(sql, transaction: dispatcher.Transaction);

            await dispatcher.Commit();
        }
    }
}
