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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

//Libre office has a convert route which can perform merges
public class MergeOfficeRequest : BuildRequestBase
{
    private readonly IResolveContentType _resolver = new ResolveContentTypeImplementation();

    protected override string ApiPath
        => Constants.Gotenberg.LibreOffice.ApiPaths.MergeOffice;

    public bool PrintAsLandscape { get; set; }

    public string? PageRanges { get; set; }

    /// <summary>
    ///     Tells gotenberg to perform the conversion with unoconv.
    ///     If you specify this with a Format the API has unoconv convert it to that.
    ///     Note: the documentation says you can't use both together but that regards request headers.
    ///     When true and Format is not set, the client falls back to PDF/A-1a.
    /// </summary>
    public bool UseNativePdfFormat { get; set; }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        var validItems = (this.Assets?.FindValidOfficeMergeItems(this._resolver)).IfNullEmpty()
            .ToList();

        if (validItems.Count < 1)
            throw new
                ArgumentException(
                    $"No Valid Office Documents to Convert. Valid extensions: {string.Join(", ", MergeOfficeConstants.AllowedExtensions)}");

        yield return CreateFormDataItem(
            "true",
            Constants.Gotenberg.LibreOffice.Routes.Convert.Merge);

        foreach (var item in validItems.ToHttpContent())
            yield return item;

        foreach (var item in this.Config.IfNullEmptyContent())
            yield return item;

        foreach (var item in this.PropertiesToHttpContent())
            yield return item;
    }
}