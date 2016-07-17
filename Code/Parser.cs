using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Net;
using IParserPlugin;
using System.Threading;

namespace Code {
    class Parser {
        // Парсит Список объявлений авито на ссылки на эти объявления
        //public List<UnitFromList> AvitoGetLinksFromList(string html,string webSite, string OriginalURL) {
        //    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        //    // отдадим на обработку обработчику
        //    Doc.LoadHtml(html);
        //    List<UnitFromList> Result = new List<UnitFromList>();
        //    HtmlNode trNodesHolder = doc.DocumentNode.Descendants("div")
        //        .Where(x => x.GetAttributeValue("class", "") == "js-catalog_after-ads").FirstOrDefault();
        //    List<HtmlNode> Units = trNodesHolder.ChildNodes.Where(x => x.Name == "div").ToList();
        //    foreach(HtmlNode Nod in Units) {
        //        // получим ID
        //        string ID = Nod.GetAttributeValue("id", "").Remove(0, 1);
        //        // получим ссылку
        //        HtmlNode UrlHolderNode = Nod.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "description").FirstOrDefault();
        //        string Url = UrlHolderNode.Descendants("a")
        //            .Where(x => x.ParentNode.Name == "h3" && x.ParentNode.GetAttributeValue("class", "") == "title")
        //            .FirstOrDefault().GetAttributeValue("href","");
        //        // get the price
        //        HtmlNode PriceHolderNode = Nod.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "description").FirstOrDefault();
        //        Array WithPrice = PriceHolderNode.Descendants("div")
        //            .Where(x => x.GetAttributeValue("class", "") == "about").FirstOrDefault().InnerText.Where(x => char.IsDigit(x)).ToArray();
        //        int Price;
        //        string tmp = "";
        //        foreach (char c in WithPrice)
        //            tmp += c;
        //        int.TryParse(tmp, out Price);
        //        // записываем всё что нашли в результат
        //        Result.Add(new UnitFromList() {
        //            UrlParent = OriginalURL,
        //            AddID = ID,
        //            Url = webSite + Url,
        //            Price = Price,
        //            FoundDT = DateTime.Now
        //        });
        //    }
        //    return Result;
        //}
        // парсит страницы с деталями объявления
        public List<UnitDetails> ParseUnitsFromList(List<UnitFromList> input, ParserProcess Proc, 
            ref DateTime lastRequest, TimeSpan requestFrequency, IParserPlugin.IParserPlugin plugin,
            CancellationToken Token) {

            List<UnitDetails> Result = new List<UnitDetails>();
            WebHelper www = new WebHelper();
            foreach (UnitFromList U in input) {
                try {
                    if (Token.IsCancellationRequested) {
                        break;
                    }
                    // получим код страницы
                    string Html = "";
                    if (www.GetHtmlFromUrl(U.Url, Proc, ref lastRequest, requestFrequency, out Html)) {
                        // скормим его парсеру
                        UnitDetails UD = plugin.GetUnitDetailsFromHtml(U, Html, ref lastRequest, requestFrequency);
                        // добавили в результаты
                        Result.Add(UD);
                    }
                } catch (WebException ex) {
                    Result.Add(new UnitDetails {
                        TimeAdded = DateTime.Now,
                        AddID = U.AddID,
                        Url = U.Url,
                        UrlParent = U.UrlParent,
                        Title = ex.Message
                    });
                }
            }
            return Result;
        }
        //UnitDetails AvitoGetUnitDetails(UnitFromList ListUnit, string Html, ref DateTime lastRequest, TimeSpan requestFrequency) {
        //    // создадим новый элемент для вставки
        //    UnitDetails Result = new UnitDetails();
        //    // зададим ссылку
        //    Result.UrlParent = ListUnit.UrlParent;
        //    Result.Url = ListUnit.Url;
        //    // дата. Примим текущую
        //    Result.TimeAdded = DateTime.Now;
        //    // задали ID
        //    Result.AddID = ListUnit.AddID;
        //    // создадим htmldocument
        //    HtmlDocument Doc = new HtmlDocument();
        //    Doc.LoadHtml(Html);
        //    // получим Title
        //    HtmlNode Ttl = Doc.DocumentNode.Descendants("h1")
        //        .Where(x => x.GetAttributeValue("itemprop", "") == "name").FirstOrDefault();
        //    if (Ttl != null) {
        //        Result.Title = HttpUtility.HtmlDecode(Ttl.InnerHtml);
        //    }
        //    // получим описание 
        //    HtmlNode MainText = Doc.DocumentNode.Descendants("div")
        //        .Where(x => x.GetAttributeValue("itemprop", "") == "description").FirstOrDefault();
        //    if (MainText != null) {
        //        Result.AddText = "";
        //        List<HtmlNode> MainTextInner = MainText.Descendants("p").ToList();
        //        foreach (HtmlNode N in MainTextInner) {
        //            Result.AddText += HttpUtility.HtmlDecode(N.InnerHtml);//.Replace("<br>", "\r\n"));
        //        }
        //    }
        //    // получим цену
        //    HtmlNode Price = Doc.DocumentNode.Descendants("span")
        //        .Where(x => x.GetAttributeValue("itemprop", "") == "price").FirstOrDefault();
        //    if (Price != null) {
        //        int PriceNumber;
        //        Array Mix = HttpUtility.HtmlDecode(Price.InnerHtml).Where(x => char.IsDigit(x)).ToArray();
        //        string tmp = "";
        //        foreach (char c in Mix)
        //            tmp += c;
        //        int.TryParse(tmp, out PriceNumber);
        //        if (PriceNumber != 0) {
        //            Result.Price = PriceNumber;
        //        }
        //    }
        //    // ссылки на картинки
        //    List<HtmlNode> PicturesNodes = Doc.DocumentNode.Descendants("div")
        //        .Where(x => x.GetAttributeValue("data-type", "") == "photo").ToList();
        //    if (PicturesNodes.Count > 0) {
        //        foreach (HtmlNode Pic in PicturesNodes) {
        //            string item = Pic.Descendants("a").Where(x => x.GetAttributeValue("class", "") == "gallery-link").FirstOrDefault()
        //                .GetAttributeValue("href", "");
        //            Result.Pictures.Add("http:" + item);
        //        }
        //    }
        //    else { // картинка может быть одна. Тогда предидущая часть не сработает. 
        //        HtmlNode SinglePicNode = Doc.DocumentNode.Descendants("div")
        //        .Where(x => x.GetAttributeValue("class", "").IndexOf("picture-aligner") >= 0).FirstOrDefault();
        //        if (SinglePicNode != null) {
        //            HtmlNode SingleNode = SinglePicNode.Descendants("img")
        //                .Where(x => x.GetAttributeValue("itemprop", "") == "image").FirstOrDefault();
        //            string SingleImg = SingleNode.GetAttributeValue("src", "");
        //            if (!string.IsNullOrEmpty(SingleImg))
        //                Result.Pictures.Add("http:" + SingleImg);
        //        }
        //    }
        //    return Result;
        //}
    }
}