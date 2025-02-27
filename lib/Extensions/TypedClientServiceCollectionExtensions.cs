﻿//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Net;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gotenberg.Sharp.API.Client.Extensions;

public static class TypedClientServiceCollectionExtensions
{
    [PublicAPI]
    public static IHttpClientBuilder AddGotenbergSharpClient(
        this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        return services.AddGotenbergSharpClient(
            (sp, client) =>
            {
                var ops = GetOptions(sp);
                client.Timeout = ops.TimeOut;
                client.BaseAddress = ops.ServiceUrl;
            });
    }

    [PublicAPI]
    public static IHttpClientBuilder AddGotenbergSharpClient(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
    {
        if (configureClient == null) throw new ArgumentNullException(nameof(configureClient));

        return services
            .AddHttpClient(nameof(GotenbergSharpClient), configureClient)
            .AddTypedClient<GotenbergSharpClient>()
            .ConfigurePrimaryHttpMessageHandler(
                () => new TimeoutHandler(
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip
                                                 | DecompressionMethods.Deflate
                    }))
            .AddPolicyHandler(PolicyFactory.CreatePolicyFromSettings)
            .SetHandlerLifetime(TimeSpan.FromMinutes(6));
    }

    private static GotenbergSharpClientOptions GetOptions(IServiceProvider sp)
    {
        return sp.GetRequiredService<IOptions<GotenbergSharpClientOptions>>().Value;
    }
}