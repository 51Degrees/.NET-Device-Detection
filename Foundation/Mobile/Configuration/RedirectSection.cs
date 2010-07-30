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

using System;
using System.Configuration;
using System.Web;
using System.IO;
using System.Security.Permissions;

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Settings for automatic redirection of mobile devices.
    /// </summary>
    public class RedirectSection : ConfigurationSection
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of Section.
        /// </summary>
        public RedirectSection() { }

        #endregion
        
        #region Properties
        
        /// <summary>
        /// Returns true if the functionality of <see cref="RedirectSection"/> is enabled.
        /// </summary>
        internal bool Enabled
        {
            get { return ElementInformation.IsPresent; }
        }

        /// <summary>
        /// A file used to store the details of devices that have previously accessed the web
        /// site to determine if they're making a subsequent request. Needed to ensure 
        /// multiple worker processes have a consistent view of previous activity. 
        /// (Optional – random behaviour will be experienced if not specified on web sites
        /// with more than one worker processes)
        /// </summary>
        [ConfigurationProperty("devicesFile", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        public string DevicesFile
        {
            get
            {
                return (string)this["devicesFile"];
            }
        }

        /// <summary>
        /// The number of minutes of inactivity that should occur before the requesting
        /// device should be treated as making a new request to the web site for the
        /// purposes of redirection. If the session is available the session timeout
        /// will be used and override this value. (Optional - defaults to 20 minutes)
        /// </summary>
        [ConfigurationProperty("timeout", IsRequired = false, DefaultValue = "20")]
        public int Timeout
        {
            get
            {
                return (int)this["timeout"];
            }
        }
        
        /// <summary>
        /// If set to true only the first request received by the web site is redirected
        /// to the mobileUrl when the site is accessed from a mobile device.
        /// (Optional – defaults to true)
        /// </summary>
        [ConfigurationProperty("firstRequestOnly", IsRequired = false, DefaultValue = "true")]
        public bool FirstRequestOnly
        {
            get
            {
                return (bool)this["firstRequestOnly"];
            }
        }

        /// <summary>
        /// Previously mobileRedirectUrl under the mobile/toolkit element. A url to direct 
        /// mobile devices to instead of the normal web sites landing page. (Mandatory)
        /// </summary>
        [ConfigurationProperty("mobileHomePageUrl", IsRequired = true, DefaultValue = "")]
        [StringValidator(MaxLength = 255)]
        public string MobileHomePageUrl
        {
            get
            {
                return (string)this["mobileHomePageUrl"];
            }
        }

        /// <summary>
        /// A url to display an image in error page to mobile devices
        /// </summary>
        [ConfigurationProperty("errorImageUrl", DefaultValue = "")]
        [StringValidator(MaxLength = 255)]
        public string ErrorImageUrl
        {
            get
            {
                return (string)this["errorImageUrl"];
            }
        }

        /// <summary>
        /// A regular expression used to identify pages on the web site that are designed 
        /// for mobile phones. Any page that derives from System.Web.UI.MobileControls.MobilePage 
        /// will automatically be treated as a mobile page. Use this attribute to tell redirection
        /// about mobile pages derived from other base classes such as System.Web.UI.Page.
        /// Redirection needs to be aware of mobile pages so that requests to these pages can be
        /// ignored. (Optional)
        /// </summary>
        [ConfigurationProperty("mobilePagesRegex", IsRequired = false, DefaultValue = "")]
        [StringValidator(MaxLength = 2048)]
        public string MobilePagesRegex
        {
            get
            {
                return (string)this["mobilePagesRegex"];
            }
        }

        #endregion
    }
}