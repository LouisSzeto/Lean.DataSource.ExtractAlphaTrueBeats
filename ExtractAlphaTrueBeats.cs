﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using NodaTime;
using System.IO;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Util;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// A collection of Extra Alpha True Beats for a Symbol and date
    /// </summary>
    public class ExtractAlphaTrueBeats : BaseDataCollection
    {
        private static readonly ExtractAlphaTrueBeat _factory = new();

        /// <summary>
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object
        /// each time it is called. The returned object is assumed to be time stamped in the config.ExchangeTimeZone.
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Line of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            return _factory.Reader(config, line, date, isLiveMode);
        }

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    "alternative",
                    "extractalpha",
                    "truebeats",
                    $"{config.Symbol.Value.ToLowerInvariant()}.csv"),
                SubscriptionTransportMedium.LocalFile,
                FileFormat.FoldingCollection);
        }

        /// <summary>
        /// Return a new instance clone of this object, used in fill forward
        /// </summary>
        /// <returns>A clone of the current object</returns>
        public override BaseData Clone()
        {
            return new ExtractAlphaTrueBeats
            {
                Time = Time,
                Symbol = Symbol,
                EndTime = EndTime,
                Data = Data?.ToList(point => point.Clone())
            };
        }

        /// <summary>Indicates if there is support for mapping</summary>
        /// <remarks>Relies on the <see cref="P:QuantConnect.Data.BaseData.Symbol" /> property value</remarks>
        /// <returns>True indicates mapping should be used</returns>
        public override bool RequiresMapping()
        {
            return _factory.RequiresMapping();
        }

        /// <summary>Indicates that the data set is expected to be sparse</summary>
        /// <remarks>Relies on the <see cref="P:QuantConnect.Data.BaseData.Symbol" /> property value</remarks>
        /// <remarks>This is a method and not a property so that python
        /// custom data types can override it</remarks>
        /// <returns>True if the data set represented by this type is expected to be sparse</returns>
        public override bool IsSparseData()
        {
            return _factory.IsSparseData();
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <remarks>Will throw <see cref="T:System.InvalidOperationException" /> for security types
        /// other than <see cref="F:QuantConnect.SecurityType.Base" /></remarks>
        /// <returns>The <see cref="T:NodaTime.DateTimeZone" /> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return _factory.DataTimeZone();
        }

        /// <summary>
        /// Formats a string with TrueBeat data
        /// </summary>
        /// <returns>string containing TrueBeat information</returns>
        public override string ToString()
        {
            return $"[{string.Join(",", Data.Select(data => data.ToString()))}]";
        }
    }
}