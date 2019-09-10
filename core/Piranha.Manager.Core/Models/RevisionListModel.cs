/*
 * Copyright (c) 2019 Håkan Edling
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System;
using System.Collections.Generic;

namespace Piranha.Manager.Models
{
    /// <summary>
    /// Revision list model.
    /// </summary>
    public class RevisionListModel
    {
        public class RevisionItem
        {
            public Guid Id { get; set; }
            public DateTime Created { get; set; }
            public string Date { get { return Created.ToString("yyyy-MM-dd"); } }
            public string Time { get { return Created.ToString("HH:mm"); } }
        }

        public IList<RevisionItem> Items { get; set; } = new List<RevisionItem>();
    }
}