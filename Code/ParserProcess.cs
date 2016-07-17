using Code.Set;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IParserPlugin;

namespace Code {
    // delegates for events
    public delegate void NewDatetime(DateTime dt);
    public delegate void NewDatetimeNumber(DateTime dt, int number);
    public delegate void NewUnshippedUnits(int number, int totalShipped);
    public delegate void NewAllDetails(IEnumerable<UnitDetails> listOfUnits);
    public delegate void NewNoArguments();
    public delegate void NewErrorHandler(Exception ex);

    public class ParserProcess {
        // список найденных плагинов
        Dictionary<string, IParserPlugin.IParserPlugin> FoundPlugins;
        // наш эмулятор БД
        DBEmulator DB;
        // Settings
        Set.SettGreatUnit MainSettings;
        // Mailer
        Mailer Sender;
        DateTime LastRequest;
        // Класс, который парсит
        Parser ParserUnit;
        // Delivers us HTML of URL
        WebHelper WebHelp;
        // Just Random
        Random Rnd;
        // our Cancellation token
        CancellationToken InnerCancellationToken;
        // marker, that settings are to be updated
        bool NeedToUpdateSettings;

        public ParserProcess(CancellationToken TokenCancel) {
            // задали токен
            InnerCancellationToken = TokenCancel;
            InnerCancellationToken.Register(() => {
                FireNewCancelledByToken();
            });
            // инициализация словарика
            FoundPlugins = new Dictionary<string, IParserPlugin.IParserPlugin>();
            // get plugins collection from folder
            ICollection<IParserPlugin.IParserPlugin> plugins = UniversalPluginLoader<IParserPlugin.IParserPlugin>.LoadPlugins("Plugins");
            // Add each one to our dictionary
            foreach (var item in plugins) {
                FoundPlugins.Add(item.GetDomainName(), item);
            }
            // загрузили настройки из файла
            MainSettings = new Set.SettGreatUnit();
            try {
                MainSettings.LoadSettings();
            }
            catch {
                MainSettings.SaveSettingsTest();
                MessageBox.Show("Создан пример файла настроек. Пропишите в нём все поля и запусите прогу снова.");
                Environment.Exit(0);
            }
            var ShName = MainSettings.EmailSettings;
            // Initialize Sender guy
            Sender = new Mailer(ShName.SmtpHostName, ShName.SmtpPort, ShName.EnableSSL,
                ShName.Login, ShName.Password, ShName.TargetEmail, ShName.EmailTitle);
            LastRequest = new DateTime();
            // init parser
            ParserUnit = new Parser();///////////////////////////////////////////////////////////////////////////////////////////
            // DB Emulator
            DB = new DBEmulator();
            WebHelp = new WebHelper();
            Rnd = new Random();
        }

        public void Launch() {
            try {
                string EmailMessage;
                bool SomethingToSend = false;

                while (!InnerCancellationToken.IsCancellationRequested) {
                    // parsing & monitoring ops
                    foreach (Set.SettSection Current in MainSettings.SectionsList) {
                        if (InnerCancellationToken.IsCancellationRequested) {
                            break;
                        }
                        // получаем html
                        string ListResponse = "";
                        // if we got good response
                        if (WebHelp.GetHtmlFromUrl(Current.Url, this, ref LastRequest,
                            new TimeSpan(0, 0, 0, 0, Current.FrequencyWebRequestMSeconds), out ListResponse)) {
                            // получим список элементов на странице ссылок
                            // get current plugin
                            if (FoundPlugins.ContainsKey(Current.UrlDomainShort)) {
                                IParserPlugin.IParserPlugin CurrentPlugin = FoundPlugins[Current.UrlDomainShort];
                                List<UnitFromList> ListUnits = CurrentPlugin.GetLinksFromListPage(ListResponse, Current.UrlDomain, Current.Url).ToList();
                                // Положим список в "базу"
                                DB.SaveUnitsList(ListUnits);
                                // Возьмём не запарсенный список
                                List<UnitFromList> Unparsed = new List<UnitFromList>();
                                foreach (var CurInter in Current.InterestList) {
                                    Unparsed.AddRange(
                                        DB.GetUnparsedListUnits(CurInter.PriceMin, CurInter.PriceMax));
                                }
                                Unparsed = Unparsed.Distinct().ToList();
                                // перемешаем элементы
                                var rnd = new Random();
                                var result = Unparsed.OrderBy(item => rnd.Next());
                                // скормим его парсеру объявлений
                                List<UnitDetails> ParsedUnits = ParserUnit
                                    .ParseUnitsFromList(Unparsed, this, ref LastRequest,
                                    new TimeSpan(0, 0, 0, 0, Current.FrequencyWebRequestMSeconds), CurrentPlugin, InnerCancellationToken);
                                // Сохраним в "базу"
                                DB.SaveUnitsDetailsList(ParsedUnits);
                            }
                        }
                    }
                    // email ops
                    EmailMessage = "";
                    int TotalInMail = 0;

                    foreach (Set.SettSection Current in MainSettings.SectionsList) {
                        List<UnitDetails> ForMai = new List<UnitDetails>();
                        foreach (Interest CInter in Current.InterestList) {
                            // получим неотправленные
                            ForMai.AddRange(DB.GetUnshipped(CInter.PriceMin,
                                CInter.PriceMax, Current.Url, CInter.ConstraintsList, CInter.KeyWords, CInter.OnlyWithPictures));
                        }
                        ForMai = ForMai.Distinct().ToList();
                        TotalInMail += ForMai.Count;
                        if (ForMai.Count > 0 && DateTime.Now > Current.LastEmailDT + new TimeSpan(0, 0, Current.FrequencyEmailSeconds)) {
                            // compose message string
                            EmailMessage += MailHelper.ComposeEmailString(ForMai);
                            //Mail the message
                            foreach (Interest CInter in Current.InterestList) {
                                DB.SetShipped(CInter, Current.Url);
                                Current.LastEmailDT = DateTime.Now;
                            }
                            SomethingToSend = true;
                        }
                        // вывод неотправленного кол-ва элементов
                        // получим неотправленное кол-во
                        List<UnitDetails> Unshipped = new List<UnitDetails>();
                        foreach (Interest CInter in Current.InterestList) {
                            List<UnitDetails> InnerUnshipped = DB.GetUnshipped(CInter.PriceMin, CInter.PriceMax, Current.Url,
                                CInter.ConstraintsList, CInter.KeyWords, CInter.OnlyWithPictures).ToList();
                            Unshipped.AddRange(InnerUnshipped);
                            Unshipped = Unshipped.Distinct().ToList();
                        }
                        int Shipped = DB.GetShippedTotal().Count;
                        // запустим событие
                        FireNewUnshipped(Unshipped.Count, Shipped);
                    }
                    if (SomethingToSend) {
                        Sender.SendString(EmailMessage, TotalInMail);
                        // fire new e-mail event
                        FireNewEMail(TotalInMail);
                        SomethingToSend = false;
                    }
                    if (InnerCancellationToken.IsCancellationRequested) break;
                    // Wait for some time
                    int Iwaitfor = Rnd.Next(125, 240);
                    TimeSpan IdleSpan = new TimeSpan(0, 0, Iwaitfor);
                    DateTime SleepUntil = DateTime.Now + IdleSpan;
                    FireNewIdleEvent(SleepUntil);
                    // запустим ивент обновления списка детализированных юнитов
                    FireAllUnitsUpdate(DB.GetAllUnitDetails());

                    while (DateTime.Now < SleepUntil && !InnerCancellationToken.IsCancellationRequested) {
                        Thread.Sleep(500);
                    }
                    if (NeedToUpdateSettings)
                        UpdateSettings();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString() + Environment.NewLine + ex.Message);
                FireNewException(ex);
            }
        }

        public void RequestUpdateSettings() {
            NeedToUpdateSettings = true;
        }
        void UpdateSettings() {
            MainSettings.LoadSettings();
            FireNewSettingsLoaded();
            // reset the trigger
            NeedToUpdateSettings = false;
        }
        public SettGreatUnit GetSettings() {
            return MainSettings;
        }

        #region Regarding events and FireAnEventMethods
        // событие нового запроса на сайт
        public event NewDatetime NewRequestEvent;
		// событие нового e-mail
		public event NewDatetimeNumber NewEMailEvent;
        // new delay event
        public event NewDatetime NewIdleEvent;
        // событие кол-ва неотправленных элементов
        public event NewUnshippedUnits NewUnshippedUnits;
        public event NewAllDetails NewAllUnitsUpdate;
        // событие, что парсер остановлен
        public event NewNoArguments NewPaused;
        // парсер вновь запущен
        public event NewNoArguments NewResumed;
        // парсер остановлен токеном
        public event NewNoArguments NewCancelledByToken;
        // настройки загружены
        public event NewNoArguments NewSettingsLoaded;
        // ивент ошибки в приложении
        public event NewErrorHandler NewException;

        internal void FireNewHttpRequest() {
			if (NewRequestEvent != null)
				NewRequestEvent(DateTime.Now);
        }
		void FireNewEMail(int emailed) {
			if (NewEMailEvent != null)
				NewEMailEvent(DateTime.Now, emailed);
		}
        void FireNewIdleEvent(DateTime SleepUntil) {
            if (NewIdleEvent != null) {
                NewIdleEvent(SleepUntil);
            }
        }
        void FireAllUnitsUpdate(IEnumerable<UnitDetails> intake) {
            if(intake != null && NewAllUnitsUpdate != null) {
                NewAllUnitsUpdate(intake);
            }
        }
		void FireNewUnshipped(int number, int shipped) {
			if (NewUnshippedUnits != null)
				NewUnshippedUnits(number, shipped);
		}
        void FireNewPaused() {
            if(NewResumed != null) {
                NewResumed();
            }
        }
        void FireNewCancelledByToken() {
            if (NewCancelledByToken != null) {
                NewCancelledByToken();
            }
        }
        void FireNewSettingsLoaded() {
            if (NewSettingsLoaded != null) {
                NewSettingsLoaded();
            }
        }
        void FireNewException(Exception ex) {
            if(NewException != null) {
                NewException(ex);
            }
        }

        #endregion
    }
}
