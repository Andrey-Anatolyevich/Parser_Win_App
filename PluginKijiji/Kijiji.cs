using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IParserPlugin;
using HtmlAgilityPack;
using System.Web;


namespace PluginKijiji {
    public class Kijiji : IParserPlugin.IParserPlugin {
        public string GetDomainName() {
            return "kijiji";
        }

        public IEnumerable<UnitFromList> GetLinksFromListPage(string html, string webSiteUrl, string OriginalURL) {
            HtmlDocument HtmlDocument = new HtmlDocument();
            // отдадим на обработку обработчику
            HtmlDocument.LoadHtml(html);
            List<UnitFromList> Result = new List<UnitFromList>();
            HtmlNode trNodesHolder = HtmlDocument.DocumentNode
                .Descendants("div")
                .Where(x => x.GetAttributeValue("class", "") == "container-results")
                .FirstOrDefault();
            if (trNodesHolder != null) {
                List<HtmlNode> Units = trNodesHolder
                    .ChildNodes
                    .Where(x => x.Name == "table" && x.GetAttributeValue("class", "")
                    .Contains("regular-ad"))
                    .ToList();
                foreach (HtmlNode Nod in Units) {
                    // получим ссылку
                    string Url = Nod.GetAttributeValue("data-vip-url", "");
                    // получим ID
                    // Пусть для Kijiji будет ID типа: /v-cars-trucks/vancouver/2007-mini-cooper-priced-to-sell/1114050511
                    // те. ссылка до знака '?'
                    string ID = Url.Contains("?") ? Url.Substring(0, Url.IndexOf("?")) : Url;
                    // get the price
                    int Price = 0;
                    HtmlNode PriceHolderNode = Nod.Descendants("td")
                        .Where(x => x.GetAttributeValue("class", "") == "price")
                        .FirstOrDefault();
                    if (PriceHolderNode != null) {
                        string TmpPrice = PriceHolderNode.InnerText;
                        if(!string.IsNullOrWhiteSpace(TmpPrice) && TmpPrice.Contains('.')) {
                            // отрежем всё после точки
                            TmpPrice = TmpPrice.Substring(0, TmpPrice.IndexOf('.'));
                        }
                        Array WithPrice = HttpUtility
                            .HtmlDecode(TmpPrice)
                            .Where(x => char.IsDigit(x))
                            .ToArray();
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
        /// <summary>
        /// Парсит страницу с объявлением в нужный нам класс
        /// </summary>
        /// <param name="listUnit">Класс со ссылкой на страницу</param>
        /// <param name="html">HTML код страницы</param>
        /// <param name="lastRequest">Время последнего запроса</param>
        /// <param name="requestFrequency">Частота запросов к серверу</param>
        /// <returns></returns>
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
                .Where(x => x.GetAttributeValue("itemprop", "") == "name")
                .FirstOrDefault();
            if (Ttl != null) {
                // получим блок h1 в котором и хранится тайтл
                var TheTitleNodes = Ttl
                    .ChildNodes
                    .Where(x => x.Name == "h1");
                if (TheTitleNodes.Any()) {
                    Result.Title = HttpUtility.HtmlDecode(TheTitleNodes.First().InnerHtml);
                }
            }
            // получим описание 
            HtmlNode MainText = Doc.DocumentNode
                .Descendants("span")
                .Where(x => x.GetAttributeValue("itemprop", "") == "description")
                .FirstOrDefault();
            if (MainText != null) {
                Result.AddText = MainText.InnerText;
            }
            // получим цену
            HtmlNode Price = Doc.DocumentNode
                .Descendants("span")
                .Where(x => x.GetAttributeValue("itemprop", "") == "price")
                .FirstOrDefault();
            if (Price != null) {
                int PriceNumber;
                string TmpString = Price.InnerHtml;
                if (TmpString.Contains('.')) {
                    TmpString = TmpString.Substring(0, TmpString.IndexOf('.'));
                }
                Array Mix = HttpUtility
                    .HtmlDecode(TmpString)
                    .Where(x => char.IsDigit(x))
                    .ToArray();
                string tmp = "";
                foreach (char c in Mix)
                    tmp += c;
                int.TryParse(tmp, out PriceNumber);
                if (PriceNumber != 0) {
                    Result.Price = PriceNumber;
                }
            }
            // ссылки на картинки

            HtmlNode PicturesNodesHolder = Doc.DocumentNode
                .Descendants("ul")
                .Where(x => x.GetAttributeValue("id", "") == "ShownImage")
                .FirstOrDefault();
            List<HtmlNode> ThePicsBlocks = null;
            if(PicturesNodesHolder != null) {
                ThePicsBlocks = PicturesNodesHolder
                    .ChildNodes
                    .Where(x => x.Name == "li")
                    .ToList();
            }
            if(ThePicsBlocks != null) {
                // нашлись блоки с картинками
                foreach(var i in ThePicsBlocks) {
                    // получим ссылку на картинку внутри каждого блока
                    var Img = i.ChildNodes
                        .Where(x => x.Name == "img");
                    if(Img != null && Img.Count()>0) {
                        string S = Img.First().GetAttributeValue("src", "");
                        if (!string.IsNullOrWhiteSpace(S)) {
                            Result.Pictures.Add(S);
                        }
                    }
                }
            }
            return Result;
        }
    }
}
