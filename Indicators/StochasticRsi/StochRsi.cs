﻿using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // STOCHASTIC RSI
        public static IEnumerable<StochRsiResult> GetStochRsi(IEnumerable<Quote> history, int lookbackPeriod = 14)
        {

            // clean quotes
            history = Cleaners.PrepareHistory(history);

            // validate parameters
            ValidateStochRsi(history, lookbackPeriod);

            // initialize
            List<StochRsiResult> results = new List<StochRsiResult>();
            IEnumerable<RsiResult> rsiResults = GetRsi(history, lookbackPeriod);

            // calculate
            foreach (Quote h in history)
            {

                StochRsiResult result = new StochRsiResult
                {
                    Index = (int)h.Index,
                    Date = h.Date,
                };

                if (h.Index >= 2 * lookbackPeriod)
                {
                    IEnumerable<RsiResult> period = rsiResults.Where(x => x.Index <= h.Index && x.Index > (h.Index - lookbackPeriod));
                    float? rsi = period.Where(x => x.Index == h.Index).FirstOrDefault().Rsi;
                    float? rsiHigh = period.Select(x => x.Rsi).Max();
                    float? rsiLow = period.Select(x => x.Rsi).Min();

                    result.StochRsi = (rsi - rsiLow) / (rsiHigh - rsiLow);
                }

                results.Add(result);
            }


            // add direction
            float? lastRSI = 0;
            bool? lastIsIncreasing = null;
            foreach (StochRsiResult r in results.Where(x => x.Index >= 2 * lookbackPeriod).OrderBy(d => d.Index))
            {
                if (r.Index >= 2 * lookbackPeriod + 1)
                {
                    if (r.StochRsi > lastRSI)
                    {
                        r.IsIncreasing = true;
                    }
                    else if (r.StochRsi < lastRSI)
                    {
                        r.IsIncreasing = false;
                    }
                    else
                    {
                        // no change, keep trend
                        r.IsIncreasing = lastIsIncreasing;
                    }
                }

                lastRSI = r.StochRsi;
                lastIsIncreasing = r.IsIncreasing;
            }

            return results;
        }


        private static void ValidateStochRsi(IEnumerable<Quote> history, int lookbackPeriod)
        {

            // check parameters
            if (lookbackPeriod <= 0)
            {
                throw new BadParameterException("Lookback period must be greater than 0 for Stochastic RSI.");
            }

            // check history
            int qtyHistory = history.Count();
            int minHistory = 2 * lookbackPeriod;
            if (qtyHistory < minHistory)
            {
                throw new BadHistoryException("Insufficient history provided for Stochastic RSI.  " +
                        string.Format("You provided {0} periods of history when at least {1} is required.", qtyHistory, minHistory));
            }
        }
    }

}
