﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Abstraction to acquire tokens from a Managed Service Identity.
    /// </summary>
    public class ManagedIdentityAuthenticator : IAuthenticator
    {
        private readonly AzureServiceTokenProvider _tokenProvider;
        private readonly string _resource;
        private readonly string _tenantId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedIdentityAuthenticator"/> class.
        /// </summary>
        /// <param name="tenantId">Tenant id for the managed identity to be used for acquiring tokens.</param>
        /// <param name="managedIdentityClientId">Client id for the managed identity to be used for acquiring tokens.</param>
        /// <param name="resource">Resource for which to acquire the token.</param>
        public ManagedIdentityAuthenticator(string tenantId, string managedIdentityClientId, string resource)
        {
            if (string.IsNullOrEmpty(managedIdentityClientId))
            {
                throw new ArgumentNullException(nameof(managedIdentityClientId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }

            //https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet
            // "RunAs=App;AppId=<client-id-guid>" for user-assigned managed identities
            // Production TODOS: does AzureServiceTokenProvider cache? how does this behave under load?
            _tokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={managedIdentityClientId}");
            _resource = resource;
            _tenantId = tenantId;
        }

        /// <inheritdoc/>
        public async Task<AuthenticatorResult> GetTokenAsync(bool forceRefresh = false)
        {
            var authResult = await _tokenProvider.GetAuthenticationResultAsync(_resource, _tenantId, forceRefresh).ConfigureAwait(false);

            return new AuthenticatorResult()
            {
                AccessToken = authResult.AccessToken,
                ExpiresOn = authResult.ExpiresOn
            };
        }
    }
}
