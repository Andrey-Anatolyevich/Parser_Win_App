using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Set {
    public class Interest {
        public string Name { get; set; }
        public int PriceMin { get; set; }
        public int PriceMax { get; set; }
        public bool OnlyWithPictures { get; set; }
        public List<string> ConstraintsList { get; set; }
        public List<string> KeyWords { get; set; }

        public Interest() {
            this.ConstraintsList = new List<string>();
            this.KeyWords = new List<string>();
        }
    }
}
