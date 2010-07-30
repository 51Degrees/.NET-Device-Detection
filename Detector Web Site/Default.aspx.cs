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
using FiftyOne.Foundation.Mobile.Detection;

namespace Detector
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Add headers for use by the CheckUA.aspx page and accuracy tester.
            Response.AddHeader("IsMobileDevice", Request.Browser.IsMobileDevice.ToString());
            Response.AddHeader("MobileDeviceManufacturer", Request.Browser.MobileDeviceManufacturer);
            Response.AddHeader("MobileDeviceModel", Request.Browser.MobileDeviceModel);
            Response.AddHeader("ScreenPixelsHeight", Request.Browser.ScreenPixelsHeight.ToString());
            Response.AddHeader("ScreenPixelsWidth", Request.Browser.ScreenPixelsWidth.ToString());

            if (Request.Browser is FiftyOne.Foundation.Mobile.Detection.Wurfl.MobileCapabilities)
            {
                string deviceId =((FiftyOne.Foundation.Mobile.Detection.Wurfl.MobileCapabilities)Request.Browser).DeviceId;
                if (deviceId != null)
                    Response.AddHeader("deviceid", deviceId);
                Response.AddHeader("ActualDeviceRoot", ((FiftyOne.Foundation.Mobile.Detection.Wurfl.MobileCapabilities)Request.Browser).ActualDeviceRoot.ToString());
            }

            if (Request.Browser is MobileCapabilities)
            {
                Response.AddHeader("PointingMethod", ((MobileCapabilities) Request.Browser).PointingMethod.ToString());
            }

            // Ensure the page is never cached.
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
        }
    }
}
