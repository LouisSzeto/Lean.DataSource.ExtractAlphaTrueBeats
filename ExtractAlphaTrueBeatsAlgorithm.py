# QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
# Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from AlgorithmImports import *

### <summary>
### Example algorithm using the ExtractAlphaTrueBeats type as a source of alpha
### </summary>
class ExtractAlphaTrueBeatsAlgorithm(QCAlgorithm):
    ### <summary>
    ### Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
    ### </summary>
    def Initialize(self):
        self.SetStartDate(2013, 10, 07)  #Set Start Date
        self.SetEndDate(2013, 10, 11)    #Set End Date

        equity_symbol = self.AddEquity("SPY").Symbol
        self.AddData(ExtractAlphaTrueBeats, equity_symbol)

    ### <summary>
    ### OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
    ### </summary>
    ### <param name="slice">Slice object keyed by symbol containing the stock data</param>
    def OnData(self, slice):
        data = slice.Get(ExtractAlphaTrueBeats)

        if data is not None:
            for true_beats in data.Values:
                self.Log(f"{self.Time} {true_beats}")

                for true_beat in true_beats:
                    if true_beat.FiscalPeriod.Annual and true_beat.TrueBeat > 0.05:
                        self.SetHoldings("AAPL", 1)
                    else:
                        self.Liquidate()

    ### <summary>
    ### Order fill event handler. On an order fill update the resulting information is passed to this method.
    ### </summary>
    ### <param name="orderEvent">Order event details containing details of the events</param>
    def OnOrderEvent(self, orderEvent):
        if orderEvent.Status.IsFill():
            self.Debug(f"Purchased Stock: {orderEvent.Symbol}")
