﻿/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees 
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region

using System.Collections;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    internal static class Factory
    {
        #region Fields

        /// <summary>
        /// Cache for mobile capabilities. Items are removed approx. 60 minutes after the last
        /// time they were used.
        /// </summary>
        private static readonly Cache<IDictionary> _cache = new Cache<IDictionary>(60);

        /// <summary>
        /// Lock used when 
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Used to obtain the mobile capabilities for the request or user agent string
        /// from the device data source provided.
        /// </summary>
        private static MobileCapabilities _mobileCapabilities;

        #endregion

        #region Private Properties

        /// <summary>
        /// Returns a single instance of the MobileCapabilities class used to provide
        /// capabilities to enhance the request.
        /// </summary>
        private static MobileCapabilities MobileCapabilities
        {
            get
            {
                if (_mobileCapabilities == null)
                {
                    lock (_lock)
                    {
                        if (_mobileCapabilities == null)
                        {
                            if (Manager.Enabled)
                                _mobileCapabilities = new Wurfl.MobileCapabilities();
                        }
                    }
                }
                return _mobileCapabilities;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Checks to determine if the device is one that should be redirected
        /// by making a direct call to the device detection layer.
        /// </summary>
        /// <param name="context">The context of the request.</param>
        /// <returns>True if the device is a device that should be redirected.</returns>
        internal static bool IsRedirectDevice(HttpContext context)
        {
            return MobileCapabilities.IsRedirectDevice(context);
        }

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the useragent
        /// string provided.
        /// </summary>
        /// <param name="userAgent">The useragent for the device.</param>
        /// <returns></returns>
        internal static IDictionary Create(string userAgent)
        {
            IDictionary caps;

            // We can't do anything with empty user agent strings.
            if (userAgent == null)
                return null;
            
            if (_cache.GetTryParse(userAgent, out caps))
            {
                // Return these capabilities for adding to the existing ones.
                return caps;
            }

            // Create the new mobile capabilities and record the collection of
            // capabilities for quick creation in future requests.
            caps = MobileCapabilities.Create(userAgent);
            _cache[userAgent] = caps;
            return caps;
        }

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the context
        /// of the requesting device.
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> of the requesting device.</param>
        /// <returns>A new mobile capabilities</returns>
        internal static IDictionary Create(HttpContext context)
        {
            IDictionary caps;

            // We can't do anything with empty user agent strings.
            if (context.Request.UserAgent == null)
                return null;

            if (_cache.GetTryParse(context.Request.UserAgent, out caps))
            {
                // Return these capabilities for adding to the existing ones.
                return caps;
            }

            // Create the new mobile capabilities and record the collection of
            // capabilities for quick creation in future requests.
            caps = MobileCapabilities.Create(context);
            _cache[context.Request.UserAgent] = caps;
            return caps;
        }

        #endregion
    }
}