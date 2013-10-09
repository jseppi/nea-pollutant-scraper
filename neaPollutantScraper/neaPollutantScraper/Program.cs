using neaPollutantScraper.Models;
using ServiceStack.OrmLite;
using SkyScraper;
using System;
using System.Collections.Generic;
using System.Data;

namespace neaPollutantScraper
{
    static class Globals
    {
        public static readonly string dbPath = "neaPollutantRecords.sqlite";
        public static readonly string baseUrl = "http://app2.nea.gov.sg/anti-pollution-radiation-protection/air-pollution/psi/pollutant-concentrations/type/";

        public static IDictionary<string, Region> RegionDict = new Dictionary<string, Region>();
        public static IDictionary<string, Pollutant> PollutantDict = new Dictionary<string, Pollutant>();
    }
    
    class Program
    {
        static void CreateTables()
        {
            using (IDbConnection db = Globals.dbPath.OpenDbConnection())
            {
                db.CreateTables(false, typeof(Pollutant), typeof(Region), typeof(Record));
            }
        }

        static void GetAndCreateRegions()
        {
            string[] regionNames = new string[] { "North", "South", "East", "West", "Central" };

            using (IDbConnection db = Globals.dbPath.OpenDbConnection())
            {
                foreach (var name in regionNames)
                {
                    var reg = db.FirstOrDefault<Region>(r => r.Name == name);
                    if (reg == null)
                    {
                        db.Insert<Region>(new Region { Name = name });
                        reg = db.First<Region>(r => r.Name == name);
                    }

                    Globals.RegionDict[name] = reg;
                }
            } 
        }

        static void GetAndCreatePollutants()
        {
            string[] pollutants = new string[] { "SulphurDioxide", "PM10", "NitrogenDioxide", "Ozone", "CarbonMonoxide", "PM25" };
            string[] units = new string[] { "ug/m3", "ug/m3", "ug/m3", "ug/m3", "mg/m3", "ug/m3" };

            using (IDbConnection db = Globals.dbPath.OpenDbConnection())
            {
                for (int i = 0; i < pollutants.Length; i++)
                {
                    var name = pollutants[i];
                    var unit = units[i];

                    var pollutant = db.FirstOrDefault<Pollutant>(p => p.Name == name);
                    if (pollutant == null)
                    {
                        db.Insert<Pollutant>(new Pollutant { Name = name, Units = unit });
                        pollutant = db.First<Pollutant>(p => p.Name == name);
                    }

                    Globals.PollutantDict[name] = pollutant;
                }
            } 
        }

        static void Main(string[] args)
        {
            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;

            CreateTables();
            GetAndCreateRegions();
            GetAndCreatePollutants();

            foreach (Pollutant pollutant in Globals.PollutantDict.Values)
            {
                Console.WriteLine("\nFetching values for Pollutant: " + pollutant.Name);

                var url = Globals.baseUrl + pollutant.Name;

                var httpClient = new HttpClient();
                var scraper = new Scraper(httpClient, new ScrapedUrisDictionary());

                scraper.Subscribe(new PollutantTableObserver(pollutant.Name));

                scraper.DisableRobotsProtocol = true;
                scraper.Scrape(new Uri(url)).Wait();
            }
        }

        
    }
}