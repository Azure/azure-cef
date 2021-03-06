// <auto-generated>
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.EngagementFabric.AdminClient
{
    using Microsoft.Rest;
    using Microsoft.Rest.Azure;
    using Models;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for EmailOperations.
    /// </summary>
    public static partial class EmailOperationsExtensions
    {
            /// <summary>
            /// Get account (Email)
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            public static EmailAccount GetAccount(this IEmailOperations operations, string apiVersion, string account)
            {
                return operations.GetAccountAsync(apiVersion, account).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get account (Email)
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<EmailAccount> GetAccountAsync(this IEmailOperations operations, string apiVersion, string account, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetAccountWithHttpMessagesAsync(apiVersion, account, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Create or Update account
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='update'>
            /// Account
            /// </param>
            public static EmailAccount CreateOrUpdateAccount(this IEmailOperations operations, string apiVersion, EmailAccount update)
            {
                return operations.CreateOrUpdateAccountAsync(apiVersion, update).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Create or Update account
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='update'>
            /// Account
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<EmailAccount> CreateOrUpdateAccountAsync(this IEmailOperations operations, string apiVersion, EmailAccount update, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CreateOrUpdateAccountWithHttpMessagesAsync(apiVersion, update, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get account credential assignments
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            public static IList<EmailCredentialAssignment> GetCredentialAssignments(this IEmailOperations operations, string apiVersion, string account)
            {
                return operations.GetCredentialAssignmentsAsync(apiVersion, account).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get account credential assignments
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<EmailCredentialAssignment>> GetCredentialAssignmentsAsync(this IEmailOperations operations, string apiVersion, string account, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetCredentialAssignmentsWithHttpMessagesAsync(apiVersion, account, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Create or update account credential assignment
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='credentialAssignment'>
            /// Account credential assignment
            /// </param>
            public static IList<EmailCredentialAssignment> CreateOrUpdateCredentialAssignment(this IEmailOperations operations, string apiVersion, string account, EmailCredentialAssignment credentialAssignment)
            {
                return operations.CreateOrUpdateCredentialAssignmentAsync(apiVersion, account, credentialAssignment).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Create or update account credential assignment
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='credentialAssignment'>
            /// Account credential assignment
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<EmailCredentialAssignment>> CreateOrUpdateCredentialAssignmentAsync(this IEmailOperations operations, string apiVersion, string account, EmailCredentialAssignment credentialAssignment, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CreateOrUpdateCredentialAssignmentWithHttpMessagesAsync(apiVersion, account, credentialAssignment, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Delete credential assignment
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='provider'>
            /// Third party Email provider
            /// </param>
            /// <param name='id'>
            /// AppId of third party Email provider
            /// </param>
            public static void DeleteCredentialAssignment(this IEmailOperations operations, string apiVersion, string account, string provider, string id)
            {
                operations.DeleteCredentialAssignmentAsync(apiVersion, account, provider, id).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Delete credential assignment
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='provider'>
            /// Third party Email provider
            /// </param>
            /// <param name='id'>
            /// AppId of third party Email provider
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task DeleteCredentialAssignmentAsync(this IEmailOperations operations, string apiVersion, string account, string provider, string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.DeleteCredentialAssignmentWithHttpMessagesAsync(apiVersion, account, provider, id, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Create or update provider credential
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='credential'>
            /// Credential
            /// </param>
            public static EmailCredential CreateOrUpdateCredential(this IEmailOperations operations, string apiVersion, EmailCredential credential)
            {
                return operations.CreateOrUpdateCredentialAsync(apiVersion, credential).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Create or update provider credential
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='credential'>
            /// Credential
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<EmailCredential> CreateOrUpdateCredentialAsync(this IEmailOperations operations, string apiVersion, EmailCredential credential, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CreateOrUpdateCredentialWithHttpMessagesAsync(apiVersion, credential, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get provider credential
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='provider'>
            /// Third party Email provider
            /// </param>
            /// <param name='id'>
            /// AppId of third party Email provider
            /// </param>
            public static EmailCredential GetCredential(this IEmailOperations operations, string apiVersion, string provider, string id)
            {
                return operations.GetCredentialAsync(apiVersion, provider, id).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get provider credential
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='provider'>
            /// Third party Email provider
            /// </param>
            /// <param name='id'>
            /// AppId of third party Email provider
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<EmailCredential> GetCredentialAsync(this IEmailOperations operations, string apiVersion, string provider, string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetCredentialWithHttpMessagesAsync(apiVersion, provider, id, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Delete provider credential
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='provider'>
            /// Third party Email provider
            /// </param>
            /// <param name='id'>
            /// AppId of third party Email provider
            /// </param>
            public static void DeleteCredential(this IEmailOperations operations, string apiVersion, string provider, string id)
            {
                operations.DeleteCredentialAsync(apiVersion, provider, id).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Delete provider credential
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='provider'>
            /// Third party Email provider
            /// </param>
            /// <param name='id'>
            /// AppId of third party Email provider
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task DeleteCredentialAsync(this IEmailOperations operations, string apiVersion, string provider, string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.DeleteCredentialWithHttpMessagesAsync(apiVersion, provider, id, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Create or update domain
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='domain'>
            /// Domain name
            /// </param>
            public static EmailDomain CreateOrUpdateDomain(this IEmailOperations operations, string apiVersion, string account, EmailDomain domain)
            {
                return operations.CreateOrUpdateDomainAsync(apiVersion, account, domain).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Create or update domain
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='domain'>
            /// Domain name
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<EmailDomain> CreateOrUpdateDomainAsync(this IEmailOperations operations, string apiVersion, string account, EmailDomain domain, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CreateOrUpdateDomainWithHttpMessagesAsync(apiVersion, account, domain, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Get Domain
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='domain'>
            /// Name of CEF domain
            /// </param>
            public static EmailDomain GetDomain(this IEmailOperations operations, string apiVersion, string account, string domain)
            {
                return operations.GetDomainAsync(apiVersion, account, domain).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get Domain
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='apiVersion'>
            /// </param>
            /// <param name='account'>
            /// Name of CEF account
            /// </param>
            /// <param name='domain'>
            /// Name of CEF domain
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<EmailDomain> GetDomainAsync(this IEmailOperations operations, string apiVersion, string account, string domain, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetDomainWithHttpMessagesAsync(apiVersion, account, domain, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}
