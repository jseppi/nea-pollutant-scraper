using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neaPollutantScraper.Models
{
    class Record : IHasId<int>
    {
        [AutoIncrement]
        public int Id { get; set; }

        public DateTime Created { get; set; }

        [References(typeof(Pollutant))]
        public int PollutantId { get; set; }

        [References(typeof(Region))]
        public int RegionId { get; set; }

        public DateTime Time { get; set; }

        public double Value { get; set; }

        public double SubIndex { get; set; }
    }
}
