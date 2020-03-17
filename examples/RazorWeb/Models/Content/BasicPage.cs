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
using Piranha.Extend.Fields;
using Piranha.Models;

namespace RazorWeb.Models.Content
{
    public enum LayoutStyle
    {
        Black,
        White
    }

    /// <summary>
    /// Basic page with block content
    /// </summary>
    [ContentType(Title = "Standard Page")]
    [ContentTypeRoute(Title = "Default", Route = "/basicpage")]
    public class BasicPage : Page
    {
        /// <summary>
        /// Gets/sets the page header.
        /// </summary>
        [Region(Display = RegionDisplayMode.Setting)]
        public Regions.Hero Hero { get; set; }

        [Region]
        public SelectField<LayoutStyle> Layout { get; set; }
    }
}
