using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IParserPlugin;
using HtmlAgilityPack;
using System.Web;

namespace PluginCraigslist
{
    public class Craigslist : IParserPlugin.IParserPlugin {
        public string GetDomainName() {
            return "craigslist.org";
        }

        public IEnumerable<UnitFromList> GetLinksFromListPage(string html, string webSiteUrl, string OriginalURL) {
            HtmlAgilityPack.HtmlDocument HtmlDocument = new HtmlAgilityPack.HtmlDocument();
            // отдадим на обработку обработчику
            HtmlDocument.LoadHtml(html);
            List<UnitFromList> Result = new List<UnitFromList>();
            HtmlNode trNodesHolder = HtmlDocument.DocumentNode.Descendants("div")
                .Where(x => x.GetAttributeValue("class", "") == "content").FirstOrDefault();
            if (trNodesHolder != null) {
                List<HtmlNode> Units = trNodesHolder.ChildNodes.Where(x => x.Name == "p" && x.GetAttributeValue("class", "") == "row").ToList();
                foreach (HtmlNode Nod in Units) {
                    // получим ID
                    string ID = Nod.GetAttributeValue("data-pid", "");
                    // получим ссылку
                    HtmlNode UrlHolderNode = Nod.ChildNodes.Where(x => x.Name == "a").FirstOrDefault();
                    string Url = UrlHolderNode.GetAttributeValue("href", "");
                    // get the price
                    int Price = 0;
                    HtmlNode PriceHolderNode = Nod.Descendants("span").Where(x => x.GetAttributeValue("class", "") == "price").FirstOrDefault();
                    if (PriceHolderNode != null) {
                        Array WithPrice = HttpUtility.HtmlDecode(PriceHolderNode.InnerText).Where(x => char.IsDigit(x)).ToArray();
                        string tmp = "";
                        foreach (char c in WithPrice)
                            tmp += c;
                        int.TryParse(tmp, out Price);
                    }
                    // записываем всё что нашли в результат
                    Result.Add(new UnitFromList() {
                        UrlParent = OriginalURL,
                        AddID = ID,
                        Url = webSiteUrl + Url,
                        Price = Price,
                        FoundDT = DateTime.Now
                    });
                }
            }
            return Result;
        }

        public UnitDetails GetUnitDetailsFromHtml(UnitFromList listUnit, string html, ref DateTime lastRequest, TimeSpan requestFrequency) {
            // создадим новый элемент для вставки
            UnitDetails Result = new UnitDetails();
            // зададим ссылку
            Result.UrlParent = listUnit.UrlParent;
            Result.Url = listUnit.Url;
            // дата. Примим текущую
            Result.TimeAdded = DateTime.Now;
            // задали ID
            Result.AddID = listUnit.AddID;
            // создадим htmldocument
            HtmlDocument Doc = new HtmlDocument();
            Doc.LoadHtml(html);
            // получим Title
            HtmlNode Ttl = Doc.DocumentNode.Descendants("span")
                .Where(x => x.GetAttributeValue("class", "") == "postingtitletext").FirstOrDefault();
            if (Ttl != null) {
                Result.Title = HttpUtility.HtmlDecode(Ttl.InnerText);
            }
            // получим описание 
            HtmlNode MainText = Doc.DocumentNode.Descendants("section")
                .Where(x => x.GetAttributeValue("id", "") == "postingbody").FirstOrDefault();
            if (MainText != null) {
                Result.AddText = MainText.InnerText;
            }
            // получим цену
            HtmlNode Price = Doc.DocumentNode.Descendants("span")
                .Where(x => x.GetAttributeValue("class", "") == "price").FirstOrDefault();
            if (Price != null) {
                int PriceNumber;
                Array Mix = HttpUtility.HtmlDecode(Price.InnerHtml).Where(x => char.IsDigit(x)).ToArray();
                string tmp = "";
                foreach (char c in Mix)
                    tmp += c;
                int.TryParse(tmp, out PriceNumber);
                if (PriceNumber != 0) {
                    Result.Price = PriceNumber;
                }
            }
            // ссылки на картинки
            HtmlNode PicturesNodes = Doc.DocumentNode.Descendants("div")
                .Where(x => x.GetAttributeValue("id", "") == "thumbs").FirstOrDefault();
            List<HtmlNode> ListOfNodes = null;
            if (PicturesNodes != null) {
                ListOfNodes = PicturesNodes.Descendants("a").ToList();
            }
            if (ListOfNodes!= null) {
                foreach (HtmlNode Pic in ListOfNodes) {
                    string item = Pic.GetAttributeValue("href", "");
                    Result.Pictures.Add(item);
                }
            }
            else { // картинка может быть одна. Тогда предидущая часть не сработает. 
                HtmlNode SinglePicNode = Doc.DocumentNode.Descendants("div")
                .Where(x => x.GetAttributeValue("class", "") == "tray").FirstOrDefault();
                if (SinglePicNode != null) {
                    HtmlNode SingleNode = SinglePicNode.Descendants("img").FirstOrDefault();
                    if (SingleNode != null) {
                        string SingleImg = SingleNode.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(SingleImg))
                            Result.Pictures.Add(SingleImg);
                    }
                }
            }
            return Result;
        }
    }
}
