﻿/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using FiftyOne.Foundation.Mobile.Configuration;
using FiftyOne.Foundation.Mobile.Detection.Entities;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Module used to enhance mobile browser information and detect and redirect
    /// mobile devices accessing non-mobile web pages.
    /// </summary>
    public class DetectorModule : Redirection.RedirectModule
    {
        #region Fields

        /// <summary>
        /// Set to true if eTags aren't supported due to the version of IIS.
        /// </summary>
        private static bool _eTagNotSupported = false;

        /// <summary>
        /// Used to lock the initialisation of static fields.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Timer used to check for new versions of the data file.
        /// </summary>
        private static Timer _autoUpdateDownloadTimer;

        /// <summary>
        /// Timer used to check for new versions of the file locally.
        /// </summary>
        private static Timer _fileCheckTimer;

#if !VER4

        /// <summary>
        /// Indicates if static initialisation has been completed.
        /// </summary>
        private static bool _initialised = false;

        /// <summary>
        /// Collection of client targets used by the application with the key
        /// field representing the alias and the value the useragent.
        /// </summary>
        private static SortedList<string, string> _clientTargets;

#endif

        #endregion

        #region Constructors

        static DetectorModule()
        {
            if (Detection.Configuration.Manager.Enabled)
            {
                // If there are licence keys available with which to fetch the 
                // binary data files.
                if (Detection.Configuration.Manager.AutoUpdate &&
                    LicenceKey.Keys.Length > 0)
                {
                    // Start the auto update thread to check for new data files.
                    _autoUpdateDownloadTimer = new Timer(
                        new TimerCallback(AutoUpdate.CheckForUpdate),
                        null,
                        Constants.AutoUpdateDelayedStart,
                        Constants.AutoUpdateSleep);
                }

                // Check the master file more frequently incase it's changed.
                _fileCheckTimer = new Timer(
                    new TimerCallback(WebProvider.CheckDataFileRefresh),
                    null,
                    Constants.FileUpdateDelayedStart,
                    Constants.FileUpdateSleep);
            }
        }

        #endregion

        #region Initialisers

        /// <summary>
        /// Initialises the device detection module.
        /// </summary>
        /// <param name="application">HttpApplication object for the web application.</param>
        public override void Init(HttpApplication application)
        {
            if (Detection.Configuration.Manager.Enabled)
            {
                Initialise(application);
            }
            base.Init(application);
        }

        /// <summary>
        /// Initiliases the HttpModule registering this modules interest in
        /// all new requests and handler mappings.
        /// </summary>
        /// <param name="application">HttpApplication object for the web application.</param>
        protected void Initialise(HttpApplication application)
        {
            // Configure the bandwidth monitoring component.
            Feature.Bandwidth.Init(application.Application);

            // Configure the image optimiser component.
            Feature.ImageOptimiser.Init(application.Application);

            // Configure the profile override component.
            Feature.ProfileOverride.Init(application.Application);

            // Register for an event to capture javascript requests
            // for static resources and record bandwidth information.
            application.BeginRequest += OnBeginRequestJavascript;

            // Register for the event after the request is authorised so that
            // images can be optimised.
            application.PostAuthorizeRequest += OnPostAuthorizeRequest;

            // Used to check for the presence of the handler so that the header
            // can be altered to include the extra javascript.
            application.PostMapRequestHandler += OnPostMapRequestHandler;

            // Used to set the bandwidth monitoring information.
            application.PostRequestHandlerExecute += OnPostRequestHandlerExecute;
            application.PostAcquireRequestState += OnPostAcquireRequestState;

#if VER4
            // Replace the .NET v4 BrowserCapabilitiesProvider if it's not ours.
            if (HttpCapabilitiesBase.BrowserCapabilitiesProvider is MobileCapabilitiesProvider == false)
            {
                lock (_lock)
                {
                    if (HttpCapabilitiesBase.BrowserCapabilitiesProvider is MobileCapabilitiesProvider == false)
                    {
                        // Use the existing provider as the parent if it's not null. 
                        if (HttpCapabilitiesBase.BrowserCapabilitiesProvider != null)
                            HttpCapabilitiesBase.BrowserCapabilitiesProvider =
                                new MobileCapabilitiesProvider((HttpCapabilitiesDefaultProvider)HttpCapabilitiesBase.BrowserCapabilitiesProvider);
                        else
                            HttpCapabilitiesBase.BrowserCapabilitiesProvider =
                                new MobileCapabilitiesProvider();
                    }
                }
            }

            // The new BrowserCapabilitiesProvider functionality in .NET v4 negates the need to 
            // override browser properties in the following way. The rest of the module is redundent
            // in .NET v4.
#else
            EventLog.Debug("Initialising Detector Module");

            StaticFieldInit();

            // Intercept the beginning of the request to override the capabilities.
            application.BeginRequest += OnBeginRequest;

            // Check for a MobilePage handler being used.
            application.PreRequestHandlerExecute += SetPreferredRenderingType;

            // If client targets are specified then check to see if one is being used
            // and override the requesting device information.
            if (_clientTargets != null)
                application.PreRequestHandlerExecute += SetPagePreIntClientTargets;
#endif
        }

        /// <summary>
        /// Called after the session has been initialised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPostAcquireRequestState(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            if (context != null)
            {
                Feature.Bandwidth.PostAcquireRequestState(context);
            }
        }

        /// <summary>
        /// Called before the response is send to the browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPostRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            if (context != null)
            {
                Feature.Bandwidth.PostRequestHandlerExecute(context);
            }
        }

#if !VER4

        /// <summary>
        /// Initialises the static fields.
        /// </summary>
        private static void StaticFieldInit()
        {
            if (_initialised == false)
            {
                lock (_lock)
                {
                    if (_initialised == false)
                    {
                        // Get a list of the client target names.
                        _clientTargets = GetClientTargets();

                        // Indicate initialisation is complete.
                        _initialised = true;
                    }
                }
            }
        }

#endif

        #endregion

#if !VER4

        #region Events


        /// <summary>
        /// If the handler is a MobilePage then ensure a compaitable textwriter will be used.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        private static void SetPreferredRenderingType(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;

                // Check to see if the handler is a mobile page. If so then the preferred markup
                // needs to change as the legacy mobile controls do not work with html4 specified.
                // If these lines are removed a "No mobile controls device configuration was registered 
                // for the requesting device" exception is likely to occur.
                if (context.Handler != null &&
                    IsMobileType(context.Handler.GetType()) &&
                    "html4".Equals(context.Request.Browser.PreferredRenderingType, 
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    context.Request.Browser.Capabilities["preferredRenderingType"] = "html32";
                }
            }
        }

        /// <summary>
        /// Ensures the PreInit event of the page is processed by the module
        /// to override capabilities associated with a client target specification.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        private void SetPagePreIntClientTargets(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;

                // If this handler relates to a page use the preinit event of the page
                // to set the capabilities providing time for the page to set the 
                // clienttarget property if required.
                if (context != null &&
                    context.Handler is Page)
                    ((Page)context.Handler).PreInit += OnPreInitPage;
            }
        }

        /// <summary>
        /// Override the capabilities assigned to the request.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        private void OnBeginRequest(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;
                if (context != null)
                {
                    try
                    {
                        // Override the capabilities incase the developers page needs them in the 
                        // preinit event of the page.                
                        OverrideCapabilities(context);
                    }

                    // Some issues of null exceptions have been reported and this is temporary code to trap 
                    // them recording headers to help further diagnosis.
                    catch (NullReferenceException ex)
                    {
                        StringBuilder builder = new StringBuilder("A Null Exception occured which has the following header info:");
                        foreach (string key in context.Request.Headers.Keys)
                            builder.Append(Environment.NewLine).Append(key).Append("=").Append(
                                context.Request.Headers[key]);

                        // Create new exception with the additional information and record to the log file.
                        EventLog.Fatal(new MobileException(builder.ToString(), ex));
                    }
                }
            }
        }

        /// <summary>
        /// Before the page initialises make sure the latest browser capabilities
        /// are available if a client target has been specified.
        /// </summary>
        /// <param name="sender">Page associated with the request.</param>
        /// <param name="e">Event arguements.</param>
        private void OnPreInitPage(object sender, EventArgs e)
        {
            Page page = sender as Page;

            // Check to see if a client target has been specified and if it has
            // use the associated useragent string to assign the capabilities.
            string userAgent = null;
            if (page != null && _clientTargets.TryGetValue(page.ClientTarget, out userAgent))
                OverrideCapabilities(HttpContext.Current, userAgent);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the client target section from the configuration if the security level
        /// allows this method to be used. Will fail in medium trust environments.
        /// </summary>
        /// <returns></returns>
        private static ClientTargetSection GetClientTargetsSection()
        {
            return WebConfigurationManager.GetWebApplicationSection(
                       "system.web/clientTarget") as ClientTargetSection;
        }

        /// <summary>
        /// Gets a list of the client target names configured for the application. Will return null
        /// if either target names are not defined or unrestricted security access is not available.
        /// </summary>
        /// <returns>A list of client target names.</returns>
        private static SortedList<string, string> GetClientTargets()
        {
            try
            {
                ClientTargetSection targets = GetClientTargetsSection();
                if (targets != null)
                {
                    // Client targets have been defined so set the sorted list to include
                    // these details.
                    SortedList<string, string> clientNames = new SortedList<string, string>();
                    for (int index = 0; index < targets.ClientTargets.Count; index++)
                    {
                        clientNames.Add(
                            targets.ClientTargets[index].Alias,
                            targets.ClientTargets[index].UserAgent);
                    }
                    return clientNames;
                }
            }
            catch (System.Security.SecurityException)
            {
                // There is nothing we can do so return null.
                return null;
            }
            return null;
        }

        /// <summary>
        /// Adds the capabilities provided into the existing dictionary of capabilities
        /// already assigned by Microsoft and returns a new capabilities provider.
        /// </summary>
        /// <param name="currentCapabilities">Current properties assigned by Microsoft</param>
        /// <param name="fiftyOneCapabilities">Properties and values returned from 51Degrees</param>
        protected static HttpBrowserCapabilities CreateNewCapabilitiesProvider(
            SortedList<string, string[]> fiftyOneCapabilities,
            HttpBrowserCapabilities currentCapabilities)
        {
            // Enhance the current capabilities.
            var enhancedCapabilities = MobileCapabilitiesOverride.EnhancedCapabilities(
                fiftyOneCapabilities,
                currentCapabilities);

            // Use our own capabilities object.
            return new FiftyOneBrowserCapabilities(
                fiftyOneCapabilities,
                currentCapabilities,
                enhancedCapabilities);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// If this device is not already using our mobile capabilities
        /// then check to see if it's a mobile. If we think it's a mobile
        /// or .NET thinks it's not the same type of device then override
        /// the current capabilities.
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> to be tested and overridden.</param>
        protected virtual void OverrideCapabilities(HttpContext context)
        {
            context.Request.Browser = CreateNewCapabilitiesProvider(
                WebProvider.GetResults(context),
                context.Request.Browser);
        }

        /// <summary>
        /// Adds new capabilities for the useragent provided rather than the request details
        /// provided in the request paramters.
        /// </summary>
        /// <param name="context">The context who's capabilities collection should be updated.</param>
        /// <param name="userAgent">The useragent string of the device requiring capabilities to be added.</param>
        protected virtual void OverrideCapabilities(HttpContext context, string userAgent)
        {
            context.Request.Browser = CreateNewCapabilitiesProvider(
                WebProvider.ActiveProvider.Match(userAgent).Results,
                context.Request.Browser);
        }

        #endregion

#endif

        #region Methods

        private void OnPostMapRequestHandler(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            if (context != null)
            {
                if (context.Handler is System.Web.UI.Page)
                {
                    ((System.Web.UI.Page)context.Handler).Load += OnLoad;
                }
            }
        }

        private void OnLoad(object page, EventArgs e)
        {
            // Indicates if any script calls were added to the page.
            var addedScript = false;

            // Register the image optimising script to initiate optimisation.
            addedScript |= Feature.ImageOptimiser.AddScript((System.Web.UI.Page)page);

            // Determine if the bandwidth script will be called.
            addedScript |= Feature.Bandwidth.GetIsEnabled(HttpContext.Current);

            // Register the profile override script.
            addedScript |= Feature.ProfileOverride.AddScript((System.Web.UI.Page)page);

            // Register the core javascript for 51Degrees if any scripts 
            // were added to the page.
            if (addedScript)
            {
                ((System.Web.UI.Page)page).ClientScript.RegisterClientScriptBlock(
                    GetType(),
                    ((Page)page).ResolveUrl("~/51Degrees.core.js"),
                    String.Format(
                        "<script src=\"{0}\" type=\"text/javascript\"></script>", 
                         ((Page)page).ResolveUrl("~/51Degrees.core.js")));

                // Has to occur after the core javascript has been loaded.
                Feature.Bandwidth.AddScript((System.Web.UI.Page)page);
            }
        }
        
        private void OnBeginRequestJavascript(object sender, EventArgs e)
        {
            HttpContext context = ((HttpApplication)sender).Context;
            if (context != null)
            {
                // Record the start time of the request for network
                // conditions monitoring.
                Feature.Bandwidth.BeginRequest(context);
                var invalidChars = Path.GetInvalidPathChars();
                if (context.Request.CurrentExecutionFilePath.Any(c => invalidChars.Contains(c)) == false)
                {
                    var fileName = Path.GetFileName(context.Request.CurrentExecutionFilePath);
                    if (fileName == "51Degrees.features.js" ||
                        fileName == "51Degrees.core.js")
                    {
                        context.Response.Clear();
                        context.Response.ClearHeaders();

                        // Get the hash code without performing a match.
                        var hash = GetHashCode(context);

                        if (hash == context.Request.Headers["If-None-Match"])
                        {
                            // The response hasn't changed so respond with a 304.
                            context.Response.StatusCode = 304;
                        }
                        else
                        {
                            // Response is different so send with all the cache details set.
                            DateTime expires = DateTime.MinValue;
                            var content = new StringBuilder();
                            content.AppendLine(Constants.ClientSidePropertyCopyright);
                            switch (fileName)
                            {
                                case "51Degrees.features.js":
                                    content.Append(GetFeatureJavaScript(context));
                                    expires = DateTime.UtcNow.AddMinutes(
                                        context.Session != null ? context.Session.Timeout : _redirectTimeout);
                                    break;
                                case "51Degrees.core.js":
                                    AppendCoreJavaScript(context, content);
                                    expires = WebProvider.ActiveProvider.DataSet.NextUpdate;
                                    break;
                            }
                            SendJavaScript(context, hash, content.ToString(), expires, WebProvider.ActiveProvider.DataSet.Published);
                        }

                        context.Response.End();
                    }
                }
            }
        }

        /// <summary>
        /// Provides the core javascript for the device detected by the context.
        /// </summary>
        /// <param name="context">Context of the request</param>
        /// <param name="sb">String build to append javascript to</param>
        /// <returns></returns>
        private static void AppendCoreJavaScript(HttpContext context, StringBuilder sb)
        {
            AppendJavascript(sb, Feature.ImageOptimiser.GetJavascript(context));
            AppendJavascript(sb, Feature.Bandwidth.GetJavascript(context));
            AppendJavascript(sb, Feature.ProfileOverride.GetJavascript(context));
        }

        /// <summary>
        /// If the javascript is not empty adds it to the string builder.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="content"></param>
        internal static void AppendJavascript(StringBuilder sb, string content)
        {
            if (String.IsNullOrEmpty(content) == false)
                sb.AppendLine(content);
        }

        /// <summary>
        /// Sends the content string to the response a javascript.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="hash"></param>
        /// <param name="content"></param>
        /// <param name="expires"></param>
        /// <param name="lastModified"></param>
        private static void SendJavaScript(HttpContext context, string hash, string content, DateTime expires, DateTime lastModified)
        {
            var provider = WebProvider.ActiveProvider;
            var utf8 = Encoding.UTF8.GetBytes(content);
            context.Response.ContentType = "application/x-javascript";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.StatusCode = 200;
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(expires);
            context.Response.Cache.SetLastModified(lastModified);
            context.Response.Cache.VaryByHeaders.UserAgent = true;
            context.Response.Cache.VaryByParams.IgnoreParams = false;
            if (_eTagNotSupported == false)
            {
                try
                {
                    context.Response.Headers["ETag"] = hash;
                }
                catch (PlatformNotSupportedException)
                {
                    _eTagNotSupported = true;
                    EventLog.Write("Warn", "The Detector cannot send an ETag header, probably " +
                        "because IIS is running in classic mode. For optimum operation we recommend " +
                        "using the latest IIS in integrated mode.");
                }
            }

            context.Response.AppendHeader("Content-Length", utf8.Length.ToString());
            context.Response.OutputStream.Write(utf8, 0, utf8.Length);
        }

        /// <summary>
        /// Uses the user agent of the device, the query string of the request and
        /// the published data of the detection data set to work out an MD5 hash
        /// for the response which will be used to avoid requests to find the
        /// matching device. This is quicker than hashing the result as it avoids
        /// executing the match.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string GetHashCode(HttpContext context)
        {
            var provider = WebProvider.ActiveProvider;
            var hashBuffer = new List<byte>();
            hashBuffer.AddRange(BitConverter.GetBytes(provider.DataSet.Published.Year));
            hashBuffer.AddRange(BitConverter.GetBytes(provider.DataSet.Published.Month));
            hashBuffer.AddRange(BitConverter.GetBytes(provider.DataSet.Published.Day));
            hashBuffer.AddRange(Encoding.ASCII.GetBytes(context.Request.UserAgent));
            hashBuffer.AddRange(Encoding.ASCII.GetBytes(context.Request.QueryString.ToString()));
            return Convert.ToBase64String(MD5.Create().ComputeHash(hashBuffer.ToArray()));
        }

        /// <summary>
        /// Gets the JavaScript to send to the specific client device based on the
        /// request context provided.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string GetFeatureJavaScript(HttpContext context)
        {
            var queryString = context.Request.QueryString.ToString();
            var results = WebProvider.GetResults(context);
            var features = new List<string>();
            if (String.IsNullOrEmpty(queryString))
            {
                foreach (var property in WebProvider.ActiveProvider.DataSet.Properties.Where(i =>
                    i._valueType != Entities.Property.PropertyValueType.JavaScript))
                {
                    GetFeatureJavaScript(results, features, property);
                }
            }
            else
            {
                foreach (var propertyName in HttpUtility.UrlDecode(queryString).Split(
                    new char[] { ' ', ',', '&', '|' }))
                {
                    var property = WebProvider.ActiveProvider.DataSet.Properties.FirstOrDefault(i => 
                        i.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
                    if (property != null)
                        GetFeatureJavaScript(results, features, property);
                }
            }
            return String.Format("var FODF={{{0}}};", String.Join(",", features.ToArray()));
        }

        /// <summary>
        /// Adds the value for the property provided to the list of features.
        /// </summary>
        /// <param name="results">Properties returned from a previous match</param>
        /// <param name="features">List of features to return in the javascript</param>
        /// <param name="property">Property to be added to the list of features</param>
        private static void GetFeatureJavaScript(SortedList<string, string[]> results, List<string> features, Property property)
        {
            var values = results[property.Name];
            if (values != null)
            {
                switch(property._valueType)
                {
                    case Property.PropertyValueType.Bool:
                        bool valueBool;
                        if (bool.TryParse(values[0], out valueBool))
                        {
                            features.Add(String.Format(
                                "{0}:{1}",
                                property.JavaScriptName,
                                valueBool ? "true" : "false"));
                        }
                        break;
                    case Property.PropertyValueType.Int:
                        int valueInteger;
                        if (int.TryParse(values[0], out valueInteger))
                        {
                            features.Add(String.Format(
                                "{0}:{1}",
                                property.JavaScriptName,
                                valueInteger));
                        }
                        break;
                    case Property.PropertyValueType.Double:
                        double valueDouble;
                        if (double.TryParse(values[0], out valueDouble))
                        {
                            features.Add(String.Format(
                                "{0}:{1}",
                                property.JavaScriptName,
                                valueDouble));
                        }
                        break;
                    default:
                        features.Add(String.Format(
                            "{0}:\"{1}\"",
                            property.JavaScriptName,
#if VER4
                            HttpUtility.JavaScriptStringEncode(String.Join(Constants.ValueSeperator, values))));
#else
                            JavaScriptStringEncode(String.Join(Constants.ValueSeperator, values))));
#endif
                        break;
                }
            }
        }

#if !VER4
        private static string JavaScriptStringEncode(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in Encoding.ASCII.GetBytes(value))
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (c < 32 || c > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", (int)c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
#endif
        
        /// <summary>
        /// Checks for a png or jpg being requested with a w or h query string
        /// paramter and if there is one present will check to determine
        /// if a cached version is available. If one isn't in the disk
        /// cache then one will be created. The request will be redirected
        /// back to the static file handler to server the image. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPostAuthorizeRequest(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;
                if (context != null &&
                    Feature.ImageOptimiser.GetIsEnabled(context))
                {
                    if (context.Request.CurrentExecutionFilePath.EndsWith("Empty.gif"))
                    {
                        EventLog.Debug(String.Format(
                            "Image processor detected request for empty image",
                            context.Request.CurrentExecutionFilePath));

                        Feature.ImageOptimiser.EmptyImageResponse(context);
                    }
                    else if (context.Request.CurrentExecutionFilePath.EndsWith(
                        ".png", StringComparison.InvariantCultureIgnoreCase) ||
                             context.Request.CurrentExecutionFilePath.EndsWith(
                        ".jpg", StringComparison.InvariantCultureIgnoreCase))
                    {
                        EventLog.Debug(String.Format(
                            "Image processor detected request for file path '{0}'",
                            context.Request.CurrentExecutionFilePath));

                        // Get the width and height query string properties.
                        var width = context.Request.QueryString[Manager.ImageOptimisation.WidthParam];
                        var height = context.Request.QueryString[Manager.ImageOptimisation.HeightParam];

                        if (((width != null && width.Equals("auto", StringComparison.InvariantCultureIgnoreCase)) ||
                            (height != null && height.Equals("auto", StringComparison.InvariantCultureIgnoreCase))) &&
                            context.Request.UrlReferrer != null)
                        {
                            // This means a second request will be made 
                            // for the image with the correct width and height
                            // respond with an empty gif so that the sizes
                            // can be obtained.
                            Feature.ImageOptimiser.EmptyImageResponse(context);
                        }
                        else
                        {
                            // Auto isn't present so return the optimised
                            // image based on the width and height of the device
                            // or parameters.
                            Feature.ImageOptimiser.OptimisedImageResponse(context);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Records the module being disposed if debug enabled.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            EventLog.Debug("Disposing Detector Module");
        }

        #endregion
    }
}