/* *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is �Incompatible With Secondary Licenses�, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Web.Configuration;
using System.Web.Hosting;
using FiftyOne.Foundation.Mobile.Configuration;

#endregion

#if VER4 || VER35

using System.Linq;

#endif

namespace FiftyOne.Foundation.Mobile.Detection.Configuration
{
    /// <summary>
    /// Returns all the settings from the Web.Config for this component.
    /// </summary>
    internal static class Manager
    {
        #region Fields

        private static DetectionSection _configurationSection;

        #endregion

        #region Constructors

        static Manager()
        {
            _configurationSection = Support.GetWebApplicationSection("fiftyOne/detection", false) as DetectionSection;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the path to the binary file if provided.
        /// </summary>
        internal static string BinaryFilePath
        {
            get
            {
                if (_configurationSection == null)
                    return null;
                return Mobile.Configuration.Support.GetFilePath(_configurationSection.BinaryFilePath);
            }
        }

        /// <summary>
        /// Gets a list containing the path of the xml files to be applied.
        /// </summary>
        internal static string[] XmlFiles
        {
            get
            {
                if (_configurationSection == null)
                    return null;
#if VER4 || VER35
                return  (from FileConfigElement patch in _configurationSection.XmlFiles
                         where patch.Enabled
                         select Mobile.Configuration.Support.GetFilePath(patch.FilePath)).ToArray();
#else
                List<string> patchFiles = new List<string>();
                foreach (FileConfigElement patch in _configurationSection.XmlFiles)
                    if (patch.Enabled)
                        patchFiles.Add(Support.GetFilePath(patch.FilePath));
                return patchFiles.ToArray();
#endif
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new configuration instance checking for
        /// fresh data.
        /// </summary>
        internal static void Refresh()
        {
            // Ensure the managers detection section is refreshed in case the
            // process is not going to restart as a result of the change.
            ConfigurationManager.RefreshSection("fiftyOne/detection");

            _configurationSection = Support.GetWebApplicationSection("fiftyOne/detection", false) as DetectionSection;
        }

        #endregion
    }
}