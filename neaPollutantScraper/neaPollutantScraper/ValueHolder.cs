using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neaPollutantScraper
{
    public class ValueHolder
    {
        public double Value;
        public double? SubIndex;

        public ValueHolder(double value, double? subIndex)
        {
            Value = value;
            SubIndex = subIndex;
        }

        public ValueHolder(double value)
        {
            Value = value;
        }
    }
}
