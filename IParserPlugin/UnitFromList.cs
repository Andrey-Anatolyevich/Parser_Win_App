using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IParserPlugin {
    public class UnitFromList {
        public string UrlParent { get; set; }
        public string Url { get; set; }
        public string AddID { get; set; }
        public bool IsParsed { get; set; }
        public int Price { get; set; }
        public DateTime FoundDT { get; set; }
    }
}
