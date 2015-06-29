﻿/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and 
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Provides an additional method to reduce the number of parameters
    /// passed when adding an item to the cache.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="BaseEntity"/> the cache will contain</typeparam>
    internal class Cache<T> : Cache<int, T> where T : BaseEntity 
    {
        /// <summary>
        /// Constructs a new instance of <see cref="Cache{T}"/> for
        /// use with entities.
        /// </summary>
        /// <param name="cacheSize"></param>
        internal Cache(int cacheSize) : base(cacheSize)
        {
        }

        /// <summary>
        /// Adds the item to the using it's index as the key.
        /// </summary>
        /// <param name="item"></param>
        internal void AddRecent(T item)
        {
            base.AddRecent(item.Index, item);
        }

        /// <summary>
        /// Resets the stats for the cache.
        /// </summary>
        internal void ResetCache()
        {
            base._itemsActive.Clear();
            base._itemsInactive.Clear();
            base.Misses = 0;
            base.Requests = 0;
            base.Switches = 0;
        }
    }
}
