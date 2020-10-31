using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BadiStatusGraphsCreator
{
    public class BadiInfoScraper
    {
        public async Task<IEnumerable<BadiInfo>> GetBadiInfo()
        {
            var url = "https://www.stadt-zuerich.ch/ssd/de/index/sport/schwimmen/wassertemperaturen.html";
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);


            var headers = doc.GetElementbyId("baederinfossummary").Descendants("thead").Single().Descendants("th").Select(x => x.InnerText).ToList();
            var results = new List<BadiInfo>();
            foreach (var badiTableRow in doc.GetElementbyId("baederinfossummary").Descendants("tbody").Single().Descendants("tr"))
            {
                var badi = new BadiInfo();

                var fields = badiTableRow.Descendants("td").Select(x => x.InnerText.Trim()).ToList();

                badi.Name = fields[headers.IndexOf("Badeanlage")];
                badi.Status = fields[headers.IndexOf("Status")];
                badi.Temperature = int.Parse(fields[headers.IndexOf("Wasser")].Substring(0, 2));
                badi.LastUpdate = DateTime.ParseExact(fields[headers.IndexOf("aktualisiert")], "dddd dd. MMMM yyyy HH.mm", new CultureInfo("de-DE"));
                results.Add(badi);
            }

            return results;
        }
    }



    public class BadiInfo
    {
        public string Name { get; set; }
        public decimal Temperature { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
