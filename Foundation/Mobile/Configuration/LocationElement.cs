/* *********************************************************************
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
 *     Thomas Holmes <thomasholmes_5@hotmail.com>
 * 
 * ********************************************************************* */

#region Usings

using System.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Contains information for all location specified in the <c>web.config</c> file.
    /// </summary>
    public sealed class LocationElement : ConfigurationElementCollection
    {
        # region Properties

        /// <summary>
        /// Gets the url of the webpage
        /// </summary>
        [ConfigurationProperty("url", IsRequired = true, IsKey = true)]
        public string Url
        {
            get
            {
                return (string)this["url"];
            }
        }

        /// <summary>
        /// A regular expression used to find macthing elements of the 
        /// original URL associated with the request.
        /// </summary>
        [ConfigurationProperty("matchExpression", IsRequired = false)]
        public string MatchExpression
        {
            get
            {
                return (string)this["matchExpression"];
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this homepage should be used.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
        }
        
        #endregion

        #region ConfigurationElementCollection Members

        /// <summary>
        /// Creates a new instance of <see cref="FilterElement"/>.
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new FilterElement();
        }

        /// <summary>
        /// Add element to the base collection.
        /// </summary>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FilterElement)element).Property;
        }

        #endregion
    }
}
