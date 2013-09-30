using SkyScraper;
using SkyScraper.Observers.ImageScraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkyScraper.Observers.ConsoleWriter;
using System.Threading.Tasks;
using CsQuery;

namespace neaPollutantScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            const string baseUrl = "http://app2.nea.gov.sg/anti-pollution-radiation-protection/air-pollution/psi/pollutant-concentrations/type/";
            var pollutantSuffixes = new string[] { "SulphurDioxide", "PM10", "NitrogenDioxide", "Ozone", "CarbonMonoxide", "PM25" };

            foreach (var pollutant in pollutantSuffixes)
            {
                Console.WriteLine(pollutant);

                var url = baseUrl + pollutant;
                var httpClient = new HttpClient();
                var scraper = new Scraper(httpClient, new ScrapedUrisDictionary());

                scraper.Subscribe(new PollutantTableObserver());

                scraper.DisableRobotsProtocol = true;
                scraper.Scrape(new Uri(url)).Wait();
            }

       
        }

        public class PollutantTableObserver : IObserver<HtmlDoc>
        {

            public void OnCompleted()
            {
                
            }

            public void OnError(Exception error)
            {
                
            }

            public void OnNext(HtmlDoc value)
            {
                CQ cq = value.Html;

                //The pollutant tabe is always the second one on the page
                // Unfortunately it has no other markup to find it
                var pollutantTable = cq["table"][1];
                var tbody = new CQ(pollutantTable.InnerHTML)["tbody"].FirstOrDefault();
                if (tbody == null) { return; }

                var rows = new CQ(tbody.InnerHTML)["tr"];

                string region = null;
                for (int i = 0; i < rows.Length; i++)
                {
                    switch (i)
                    {
                        case 1:
                        case 7:
                            region = "North";
                            break;
                        case 2:
                        case 8:
                            region = "South";
                            break;
                        case 3:
                        case 9:
                            region = "East";
                            break;
                        case 4:
                        case 10:
                            region = "West";
                            break;
                        case 5:
                        case 11:
                            region = "Central";
                            break;
                        default:
                            region = null;
                            break;
                    }

                    if (region == null) { continue; }

                    var row = rows[i];

                    Console.Write(region + ": ");

                    var tds = new CQ(row.InnerHTML)["td"];

                    //skip the first, it is the direction label

                    //TODO: the 11am and 11pm values can be in a <strong> tag - account for this
                    var values = tds.Skip(1).Select(x => x.InnerText.ToString().Trim());
                    Console.WriteLine(string.Join(", ", values));

                    Console.WriteLine();
                }
            }
        }
    }
}
