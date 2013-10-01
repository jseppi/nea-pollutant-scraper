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

                    //TODO: the most recent value is surrounded by <strong>
                    Console.WriteLine(tds.Length);
                    var values = tds.Skip(1).Select(x => {
                        var v = GetValue(x);
                        if (v != null) { return v.Value.ToString(); }
                        return (-1).ToString();
                    }).ToList();
                    Console.WriteLine(string.Join(", ", values));

                    Console.WriteLine();
                }
            }


            public ValueHolder GetValue(IDomObject node)
            {
                var trimmedInner = node.InnerText.ToString().Trim();

                if (trimmedInner != "")
                {
                    //TODO: Most value strings actually look like ##(##)
                    // where the number in parentheses is another value that should be recorded
                    //TODO: Parse out and fill in second argument of ValueHolder
                    double val;
                    if (double.TryParse(trimmedInner, out val))
                    {
                        return new ValueHolder(val, null);
                    }
                }
                else
                {
                    Console.Write("wrapped in <strong>: ");
                    CQ cq = node.InnerHTML;
                    Console.Write(cq["strong"][0].InnerText.ToString().Trim() + "\n");
                }

                return null;
            }
        }
    }
}