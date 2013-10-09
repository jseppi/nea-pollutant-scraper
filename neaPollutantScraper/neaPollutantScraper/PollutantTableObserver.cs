using CsQuery;
using neaPollutantScraper.Models;
using ServiceStack.OrmLite;
using SkyScraper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace neaPollutantScraper
{
    public class PollutantTableObserver : IObserver<HtmlDoc>
    {
        private readonly Pollutant Pollutant;

        public PollutantTableObserver(string pollutantName)
        {
            Pollutant = Globals.PollutantDict[pollutantName];
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        public void OnNext(HtmlDoc htmlDoc)
        {
            CQ cq = htmlDoc.Html;

            //The pollutant table is always the second one on the page
            // Unfortunately it has no other markup to find it
            var pollutantTable = cq["table"][1];
            var tbody = new CQ(pollutantTable.InnerHTML)["tbody"].FirstOrDefault();
            if (tbody == null) { return; }

            var rows = new CQ(tbody.InnerHTML)["tr"];

            string regionName = null;

            var today = DateTime.Today;

            var isPm = false;

            IList<Record> newRecords = new List<Record>();

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                if (rowIndex == 6)
                {
                    isPm = true;
                }

                regionName = getRegionNameFromIndex(rowIndex);

                if (regionName == null) { continue; }

                Region region = Globals.RegionDict[regionName];

                var row = rows[rowIndex];

                var tds = new CQ(row.InnerHTML)["td"];

                //Skip(1) because the first one is the region label

                var currHour = isPm ? 12 : 0;

                foreach (var td in tds.Skip(1))
                {
                    ValueHolder v = GetValue(td);
                    double value = -1;
                    double subIndex = -1;
                    if (v != null)
                    {
                        //This is the issue -- GetValue is null
                        value = v.Value;
                        if (v.SubIndex.HasValue)
                        {
                            subIndex = v.SubIndex.Value;
                        }
                    }

                    var currReadingTime = new DateTime(today.Year, today.Month, today.Day,
                        currHour, 0, 1);

                    currHour++; //increment the time

                    Record rec = new Record
                    {
                        Created = DateTime.UtcNow,
                        Time = currReadingTime,
                        PollutantId = Pollutant.Id,
                        RegionId = region.Id,
                        Value = value,
                        SubIndex = subIndex
                    };

                    newRecords.Add(rec);
                }
            }

            SaveRecords(newRecords);
        }

        private void SaveRecords(IList<Record> newRecords)
        {
            
            var updates = new List<Record>();
            var inserts = new List<Record>();

            using (IDbConnection db = Globals.dbPath.OpenDbConnection())
            {
                foreach (var rec in newRecords)
                {
                    var existing = db.FirstOrDefault<Record>(r =>
                        r.PollutantId == rec.PollutantId
                        && r.RegionId == rec.RegionId
                        && r.Time == rec.Time);

                    if (existing != null)
                    {
                        existing.Value = rec.Value;
                        existing.SubIndex = rec.SubIndex;
                        updates.Add(existing);
                    }
                    else
                    {
                        inserts.Add(rec);
                    }
                }

                Console.WriteLine("    Updating " + updates.Count + " records");
                db.UpdateAll<Record>(updates);

                Console.WriteLine("    Inserting " + inserts.Count + " new records");
                db.InsertAll<Record>(inserts);
            }
        }

        private ValueHolder GetValue(IDomObject node)
        {
            CQ strong = new CQ(node.InnerHTML)["strong"];

            if (strong.Length > 0)
            {
                node = strong[0];
            }

            var trimmedInner = node.InnerText.ToString().Trim();
            Regex r = new Regex(@"(?<val>\d+)(\((?<sub>\d+)\))?");

            if (trimmedInner.Equals("-"))
            {
                return null;
            }

            var match = r.Match(trimmedInner);
            if (match.Success)
            {
                double val = Convert.ToDouble(match.Groups["val"].Value);

                if (match.Groups["sub"].Success)
                {
                    double sub = Convert.ToDouble(match.Groups["sub"].Value);
                    return new ValueHolder(val, sub);
                }

                return new ValueHolder(val);
            }

            return null;
        }

        private string getRegionNameFromIndex(int i)
        {
            string region = null;
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
                default: //0 or 6
                    region = null;
                    break;
            }

            return region;
        }

    }
}
