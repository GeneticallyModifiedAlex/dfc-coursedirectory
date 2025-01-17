﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class GetProviderByUkprnHandler : ICosmosDbQueryHandler<GetProviderByUkprn, Provider>
    {
        private readonly ILogger<GetProviderByUkprnHandler> _logger;

        public GetProviderByUkprnHandler(ILogger<GetProviderByUkprnHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Provider> Execute(DocumentClient client, Configuration configuration,
            GetProviderByUkprn request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName);

            var response = await client.CreateDocumentQuery<Provider>(collectionUri,
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true
                    })
                .Where(p => p.UnitedKingdomProviderReferenceNumber == request.Ukprn.ToString())
                .AsDocumentQuery()
                .ExecuteNextAsync();

            if (response != null && response.Count > 1)
                _logger.LogWarning("Multiple Providers found for {UKPRN}", request.Ukprn);

            return response.LastOrDefault();

        }
    }
}
