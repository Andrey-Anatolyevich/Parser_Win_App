using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Code.Set {
    public class SettGreatUnit {
        string SettingsFileName = "Settings.xml";
        public SettEmail EmailSettings { get; set; }
        public List<SettSection> SectionsList { get; set; }

        public SettGreatUnit() {
            EmailSettings = new SettEmail();
            SectionsList = new List<SettSection>();
        }
        public void SaveSettings() {
            XmlSerializer Ser = new XmlSerializer(typeof(SettGreatUnit));
            TextWriter Writer = new StreamWriter(SettingsFileName);
            Ser.Serialize(Writer, this);
            Writer.Close();
        }
        public void SaveSettingsTest() {
            EmailSettings.EmailTitle = "Some title for emails";
            EmailSettings.EnableSSL = true;
            EmailSettings.Login = "your login for and e-mail you want to use for sending mail";
            EmailSettings.Password = "password";
            EmailSettings.SmtpHostName = "gmtp.gmail.com";
            EmailSettings.SmtpPort = 587;
            EmailSettings.TargetEmail = "And email you want to send results to";
            EmailSettings.TargetEmailFrom = "Your login & password email address";

            SettSection Section = new SettSection();
            Section.Name = "Test name";
            Section.FrequencyEmailSeconds = 900;
            Section.FrequencyWebRequestMSeconds = 2000;
            Section.Url = "https://www.avito.ru/moskva/noutbuki?user=1";
            Section.UrlDomain = "https://avito.ru";
            Section.UrlDomainShort = "avito.ru";
            Section.InterestList.Add(new Interest() {
                PriceMin = 20000,
                PriceMax = 45000,
                OnlyWithPictures = true,
                ConstraintsList = new List<string>() {
                    "apple",
                    "macbook"
                },
                KeyWords = new List<string>() {
                    "words your require to be in add",
                    "one",
                    "per",
                    "line"
                }
            });
            SectionsList.Add(Section);
            SaveSettings();
        }
        public void LoadSettings() {
            SettGreatUnit Result;
            XmlSerializer ResultSerializer = new XmlSerializer(typeof(SettGreatUnit));
            FileStream ResFileStream = new FileStream(SettingsFileName, FileMode.Open);
            Result = (SettGreatUnit)ResultSerializer.Deserialize(ResFileStream);
            // Вот тут и восстанавливаем настройки
            EmailSettings = Result.EmailSettings;
            SectionsList = Result.SectionsList;
        }
    }
}