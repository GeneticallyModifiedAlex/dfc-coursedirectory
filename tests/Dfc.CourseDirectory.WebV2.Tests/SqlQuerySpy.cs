﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Moq;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class SqlQuerySpy
    {
        private readonly Mock<ISqlQueryDispatcher> _dispatcherMock;

        public SqlQuerySpy()
        {
            _dispatcherMock = new Mock<ISqlQueryDispatcher>();
        }

        public void RegisterCall<T>(ISqlQuery<T> query)
        {
            _dispatcherMock.Object.ExecuteQuery(query);
        }

        public void Reset() => _dispatcherMock.Reset();

        public void VerifyQuery<TQuery, TResult>(Expression<Func<TQuery, bool>> match)
            where TQuery : ISqlQuery<TResult>
        {
            _dispatcherMock.Verify(d => d.ExecuteQuery(It.Is(match)));
        }
    }

    public class SqlQuerySpyDecorator : ISqlQueryDispatcher
    {
        private readonly ISqlQueryDispatcher _inner;
        private readonly SqlQuerySpy _spy;

        public SqlQuerySpyDecorator(ISqlQueryDispatcher inner, SqlQuerySpy sqlQuerySpy)
        {
            _inner = inner;
            _spy = sqlQuerySpy;
        }

        public Task<T> ExecuteQuery<T>(ISqlQuery<T> query)
        {
            _spy.RegisterCall(query);
            return _inner.ExecuteQuery(query);
        }
    }
}