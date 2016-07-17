using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IParserPlugin {
    public class UnitDetails {
        public string UrlParent { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public DateTime TimeAdded { get; set; }
        public List<string> Pictures { get; set; }
        public int Price { get; set; }
        public string AddText { get; set; }
        public string AddID { get; set; }
        public bool EMailed { get; set; }

        public UnitDetails() {
            this.Pictures = new List<string>();
        }
    }
}
