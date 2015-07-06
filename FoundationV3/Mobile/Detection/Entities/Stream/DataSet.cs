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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// A data set returned from the stream factory which includes a pool of
    /// data readers that are used to fetch data from the source when
    /// the data set is used to retrieve data not already in memory.
    /// </summary>
    public class DataSet : Entities.DataSet
    {
        #region Fields

        /// <summary>
        /// Pool of readers connected the underlying data file.
        /// </summary>
        internal readonly Pool Pool;

        #endregion 

        #region Properties

        /// <summary>
        /// The number of readers that have been created in the pool
        /// that connects the data set to the data source.
        /// </summary>
        public int ReadersCreated
        {
            get { return Pool.ReadersCreated; }
        }

        /// <summary>
        /// The number of readers in the queue ready to be used.
        /// </summary>
        public int ReadersQueued
        {
            get { return Pool.ReadersQueued; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// /Creates a new stream data set connected to the data
        /// file provided.
        /// </summary>
        /// <param name="fileName">Valid path to the uncompressed data set file</param>
        /// <param name="lastModified">Date and time the source data was last modified.</param>
        /// <param name="mode">
        /// The mode of operation the data set will be using.
        /// </param>
        /// <param name="isTempFile">True if the file should be deleted when the source is disposed</param>
        internal DataSet(string fileName, DateTime lastModified, Modes mode, bool isTempFile)
            : base(lastModified, mode)
        {
            Pool = new Pool(new SourceFile(fileName, isTempFile));
        }

        /// <summary>
        /// Creates a new stream data set connected to the byte array
        /// data source provided.
        /// </summary>
        /// <param name="data">Byte array containing uncompressed data set</param>
        /// <param name="mode">
        /// The mode of operation the data set will be using.
        /// </param>
        internal DataSet(byte[] data, Modes mode)
            : base(DateTime.MinValue, mode)
        {
            Pool = new Pool(new SourceMemory(data));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the cache for the data set.
        /// </summary>
        public override void ResetCache()
        {
            base.ResetCache();
            ((ICacheList)Signatures).ResetCache();
            ((ICacheList)Nodes).ResetCache();
            ((ICacheList)Strings).ResetCache();
            ((ICacheList)Profiles).ResetCache();
            ((ICacheList)Values).ResetCache();
            ((ICacheList)RankedSignatureIndexes).ResetCache();
        }

        /// <summary>
        /// Disposes of the data set closing all readers and streams in
        /// the pool. If a temporary data file is used then the file
        /// is also deleted if it's not being used by other processes.
        /// </summary>
        public override void Dispose()
        {
            Pool.Dispose();
            base.Dispose();
        }

        #endregion
    }
}