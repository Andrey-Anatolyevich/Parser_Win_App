using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Set {
    public class SettSection {
        public string Name { get; set; }
        public string Url { get; set; }
        public string UrlDomain { get; set; }
        public string UrlDomainShort { get; set; }
        public int FrequencyWebRequestMSeconds { get; set; }
        public int FrequencyEmailSeconds { get; set; }
        public DateTime LastEmailDT { get; set; }
        public List<Interest> InterestList { get; set; }

        public SettSection() {
            InterestList = new List<Interest>();
            LastEmailDT = new DateTime();
        }
    }
}
