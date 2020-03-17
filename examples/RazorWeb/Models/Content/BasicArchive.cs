/*
 * Copyright (c) 2020 Piranha CMS
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/coreweb
 *
 */

using Piranha.Extend;
using Piranha.Models;

namespace RazorWeb.Models.Content
{
    /// <summary>
    /// Basic page with block content
    /// </summary>
    [ContentType(Title = "Standard Archive", UseBlocks = false)]
    [ContentTypeRoute(Title = "Default Layout", Route = "/basicarchive")]
    [ContentTypeRoute(Title = "Fancy Layout", Route = "/fancyarchive")]
    public class BasicArchive : Archive
    {
    }
}
