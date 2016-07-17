using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IParserPlugin
{
    public interface IParserPlugin
    {
        IEnumerable<UnitFromList> GetLinksFromListPage(string html, string webSiteUrl, string OriginalURL);
        UnitDetails GetUnitDetailsFromHtml(UnitFromList listUnit, string html, ref DateTime lastRequest, TimeSpan requestFrequency);
        string GetDomainName();
    }
}
