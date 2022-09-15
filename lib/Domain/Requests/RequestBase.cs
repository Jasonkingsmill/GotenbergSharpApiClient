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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public abstract class RequestBase : IApiRequest
{
    private const string DispositionType = Constants.HttpContent.Disposition.Types.FormData;

    public RequestConfig Config { get; set; }

    public AssetDictionary Assets { get; set; }

    public PdfFormats Format { [UsedImplicitly] get; set; }

    /// <summary>
    ///     Only meant for internal use. Scoped as public b/c the interface defines it.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract string ApiPath { get; }

    public bool IsWebhookRequest => this.Config.Webhook.IsConfigured();

    public virtual ILookup<string, string> Headers => this.Config.Webhook.GetHeaders().ToLookup(s => s.Key, s => s.Value);

    public abstract IEnumerable<HttpContent> ToHttpContent();

    internal static StringContent CreateFormDataItem<T>(T value, string fieldName)
    {
        var item = new StringContent(value!.ToString()!);

        item.Headers.ContentDisposition = new ContentDispositionHeaderValue(DispositionType)
            { Name = fieldName };

        return item;
    }

    public virtual void Validate()
    {
        this.Config.Webhook.Validate();
    }
}