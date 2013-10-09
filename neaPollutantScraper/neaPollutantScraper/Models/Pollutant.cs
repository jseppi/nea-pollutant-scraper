using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neaPollutantScraper.Models
{
    class Pollutant : IHasId<int>
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Index(Unique = true)]
        public string Name { get; set; }

        public string Units { get; set; }
    }
}
