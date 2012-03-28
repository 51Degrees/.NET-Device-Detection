﻿/* *********************************************************************
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

using System.Collections.Generic;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers
{
    internal class Results : List<Result>
    {
        internal Results()
        {
        }

        internal Results(BaseDeviceInfo device)
        {
            Add(new Result(device));
        }

        internal void AddRange(BaseDeviceInfo[] devices)
        {
            foreach (BaseDeviceInfo device in devices)
                Add(device);
        }

        internal void Add(BaseDeviceInfo device)
        {
            Add(new Result(device));
        }
    }
}