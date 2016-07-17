using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Set {
    public class SettEmail {
        public string SmtpHostName { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSSL { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string TargetEmailFrom { get; set; }
        public string TargetEmail { get; set; }
        public string EmailTitle { get; set; }
    }
}
