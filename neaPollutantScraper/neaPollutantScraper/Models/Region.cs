using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neaPollutantScraper.Models
{
    class Region : IHasId<int>
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Index(Unique = true)]
        public string Name { get; set; }
    }
}
