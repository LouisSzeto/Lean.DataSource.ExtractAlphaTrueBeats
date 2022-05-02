/*
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
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuantConnect.Configuration;
using QuantConnect.Data.Auxiliary;
using QuantConnect.DataSource;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Logging;
using QuantConnect.Util;

namespace QuantConnect.DataProcessing
{
    /// <summary>
    /// ExtractAlphaTrueBeatUniverseConverter implementation.
    /// </summary>
    public class ExtractAlphaTrueBeatUniverseConverter
    {
        private readonly string _destinationFolder;
        private readonly string _universeFolder;
        private readonly string _dataFolder = Globals.DataFolder;
        private readonly bool _canCreateUniverseFiles;
        private ConcurrentDictionary<string, ConcurrentQueue<string>> _tempData = new();

        /// <summary>
        /// Creates a new instance of <see cref="ExtractAlphaTrueBeatUniverse"/>
        /// </summary>
        /// <param name="destinationFolder">The folder where the data will be saved</param>
        public ExtractAlphaTrueBeatUniverseConverter(string destinationFolder)
        {
            _destinationFolder = Path.Combine(destinationFolder, "alternative", "extractalpha", "truebeats");
            _universeFolder = Path.Combine(_destinationFolder, "universe");
            _canCreateUniverseFiles = Directory.Exists(Path.Combine(_dataFolder, "equity", "usa", "map_files"));

            Directory.CreateDirectory(_universeFolder);
        }

        /// <summary>
        /// Runs the instance of the object.
        /// </summary>
        /// <returns>True if process all downloads successfully</returns>
        public void Run()
        {
            var mapFileProvider = new LocalZipMapFileProvider();
            mapFileProvider.Initialize(new DefaultDataProvider());

            var trueBeatCsvs = Directory.GetFiles(_destinationFolder, "*.csv", SearchOption.TopDirectoryOnly);

            Parallel.ForEach(trueBeatCsvs, filePath =>
            {
                var lines = File.ReadAllLines(filePath).Select(x => x.Split(",")).ToList();
                var ticker = filePath.Split("\\").Last().Split(".").First().ToUpper();

                var csvContents = new List<string>();

                foreach (var csv in lines)
                {
                    var time = Parse.DateTimeExact(csv[0], "yyyyMMdd", DateTimeStyles.None);
                    var date = $"{time:yyyyMMdd}";

                    if (!_canCreateUniverseFiles)
                        continue;

                    var sid = SecurityIdentifier.GenerateEquity(ticker, Market.USA, true, mapFileProvider, time);

                    var universeCsvContents = $"{sid},{ticker},{String.Join(",", csv.Skip(2).Take(5))}";

                    var queue = _tempData.GetOrAdd(date, new ConcurrentQueue<string>()); 
                    queue.Enqueue(universeCsvContents);
                }
            });

            Parallel.ForEach(_tempData, kvp =>
            {
                var finalPath = Path.Combine(_universeFolder, $"{kvp.Key}.csv");
                var finalFileExists = File.Exists(finalPath);

                var lines = new HashSet<string>(kvp.Value);
                if (finalFileExists)
                {
                    foreach (var line in File.ReadAllLines(finalPath))
                    {
                        lines.Add(line);
                    }
                }
                var finalLines = lines.OrderBy(x => x.Split(',').First()).ToList();

                var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.tmp");
                File.WriteAllLines(tempPath, finalLines);
                var tempFilePath = new FileInfo(tempPath);
                tempFilePath.MoveTo(finalPath, true);
            });
        }
    }
}