using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader
{
    public class AnalysisCategory
    {
        public List<List<double>> entries;
        public List<double> aggregate;

        public AnalysisCategory()
        {
            entries = new List<List<double>>();
            aggregate = new List<double>();
        }

        public AnalysisCategory(List<double> data)
        {
            entries = new List<List<double>>();
            aggregate = new List<double>();

            entries.Add(new List<double>());

            foreach (var entry in data)
            {
                entries[0].Add(entry);
                aggregate.Add(entry);
            }
        }

        public double ComputeSimilarity(List<double> data)
        {
            double similarityMax = 0;
            double similaritySum = 0;
            double percentDifference;

            // compare to the aggregate
            for (int iii = 0; iii < aggregate.Count() - 1; ++iii)    // dont include the last data point because it is the return percent
            {
                percentDifference = (data[iii] - aggregate[iii]) / aggregate[iii];      // data and aggregate values will be [0, 1]
                if (percentDifference < 0) percentDifference *= -1;                     // swap the sign of the difference

                similaritySum += (1 - percentDifference);

                // sum += (difference * difference);            // difference can be [-1, 1] -> sum can be [0, data.Count()]
            }

            // get an average difference-squared value
            return similaritySum / data.Count();                      // this will make sum between [0, 1]

            /*
            foreach (var entry in entries)
            {
                // sum the differences at each point
                for (int iii = 0; iii < data.Count() - 1; ++iii)    // dont include the last data point because it is the return percent
                {
                    percentDifference = (data[iii] - entry[iii]) / entry[iii];     // data and entry values will be [0, 1]
                    if (percentDifference < 0) percentDifference *= -1;            // swap the sign of the difference

                    similaritySum += (1 - percentDifference);

                    // sum += (difference * difference);            // difference can be [-1, 1] -> sum can be [0, data.Count()]
                }

                // get an average difference-squared value
                similaritySum /= data.Count();                      // this will make sum between [0, 1]

                if (similaritySum > similarityMax)
                    similarityMax = similaritySum;
            }
            
            return similarityMax;
            */
        }

        public void Add(List<double> data)
        {
            entries.Add(new List<double>());
            int lastEntryIndex = entries.Count() - 1;

            foreach (var entry in data)
                entries[lastEntryIndex].Add(entry);

            aggregate.Clear();

            for (int iii = 0; iii < entries[0].Count(); ++iii)
            {
                double sum = 0;
                int numberOfEntries = entries.Count();

                foreach (var list in entries)
                    sum += list[iii];

                aggregate.Add(sum / numberOfEntries);   // add the avg value to the aggregate list
            }
        }
    }
}