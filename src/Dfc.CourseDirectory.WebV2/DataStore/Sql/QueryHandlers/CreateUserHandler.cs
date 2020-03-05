﻿using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.QueryHandlers
{
    public class CreateUserHandler : ISqlQueryHandler<CreateUser, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, CreateUser query)
        {
            var sql = @"
INSERT INTO Pttcd.Users
(UserId, Email, FirstName, LastName)
VALUES (@UserId, @Email, @FirstName, @LastName)";

            var paramz = new
            {
                query.UserId,
                query.Email,
                query.FirstName,
                query.LastName
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new None();
        }
    }
}