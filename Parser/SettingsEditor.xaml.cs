using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Code;
using Code.Set;
using System.Collections.ObjectModel;

namespace Parser {
    /// <summary>
    /// Interaction logic for SettingsEditor.xaml
    /// </summary>
    public partial class SettingsEditor : Window {
        SettGreatUnit SelectedSettings;
        SettSection SelectedSection;
        Interest SelectedInterest;

        public SettingsEditor(SettGreatUnit settings) {
            SelectedSettings = settings;
            InitializeComponent();
            FillFields();
        }

        private void FillFields() {
            if (SelectedSettings != null) {
                textBoxSmtpHost.Text = SelectedSettings.EmailSettings.SmtpHostName;
                textBoxSmtpPort.Text = SelectedSettings.EmailSettings.SmtpPort.ToString();
                switch (SelectedSettings.EmailSettings.EnableSSL) {
                    case true: comboBoxSSLEnabled.SelectedItem = true; break;
                    case false: comboBoxSSLEnabled.SelectedItem = false; break;
                }
                textBoxLogin.Text = SelectedSettings.EmailSettings.Login;
                passwordBoxPassword.Password = SelectedSettings.EmailSettings.Password;
                textBoxAccountEmail.Text = SelectedSettings.EmailSettings.TargetEmailFrom;
                textBoxAdresser.Text = SelectedSettings.EmailSettings.TargetEmail;
                textBoxEmailsSubject.Text = SelectedSettings.EmailSettings.EmailTitle;
                var Names = (from x in SelectedSettings.SectionsList
                             select x.Name);
                foreach (string s in Names)
                    comboBoxNames.Items.Add(s);
                //if (comboBoxNames.Items.Count > 0) {
                //    comboBoxNames.SelectedIndex = 0;
                //}
            }
        }

        private void comboBoxNames_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string SelectedItem = e.AddedItems[0] as string;
            if (!string.IsNullOrEmpty(SelectedItem)) {
                SettSection SectionDetails = SelectedSettings.SectionsList.Where(x =>
                x.Name == SelectedItem).FirstOrDefault();
                SelectedSection = SectionDetails;
                if (SectionDetails != null) {
                    UpdateInfoFromSection(SectionDetails);
                }
            }
        }

        private void UpdateInfoFromSection(SettSection sectionDetails) {
            textBoxSectionUrl.Text = sectionDetails.Url;
            textBoxSectionUrlDomain.Text = sectionDetails.UrlDomain;
            textBoxSectionUrlDomainShort.Text = sectionDetails.UrlDomainShort;
            textBoxSectionRequestMinDelay.Text = sectionDetails.FrequencyWebRequestMSeconds.ToString();
            textBoxSectionEmailsDelay.Text = sectionDetails.FrequencyEmailSeconds.ToString();

            List<string> ListOfInterests = (from x in sectionDetails.InterestList
                                            select x.Name).ToList();
            if (ListOfInterests.Count > 0) {
                foreach (string s in ListOfInterests) {
                    comboBoxInterests.Items.Add(s);
                }
            }
        }

        private void comboBoxInterests_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                var SelectedString = e.AddedItems[0] as string;
                if (!string.IsNullOrEmpty(SelectedString)) {
                    var CurrentInterest = (from x in SelectedSection.InterestList
                                           where x.Name == SelectedString
                                           select x).FirstOrDefault();
                    if (CurrentInterest != null) {
                        UpdateInfoFromInterest(CurrentInterest);
                    }
                }
            }
        }

        private void UpdateInfoFromInterest(Interest currentInterest) {
            textBoxInterestMinPrice.Text = currentInterest.PriceMin.ToString();
            textBoxInterestMaxPrice.Text = currentInterest.PriceMax.ToString();
            textBoxInterestConstraints.Clear();
            foreach (string s in currentInterest.ConstraintsList) {
                textBoxInterestConstraints.Text += s + Environment.NewLine;
            }
            textBoxInterestKeyWords.Clear();
            foreach (string s in currentInterest.KeyWords) {
                textBoxInterestKeyWords.Text += s + Environment.NewLine;
            }
        }

        private void buttonSaveSettings_Click(object sender, RoutedEventArgs e) {
            int SmtpPort;
            int.TryParse(textBoxSmtpPort.Text, out SmtpPort);

            SettGreatUnit NewSet = new SettGreatUnit();
            NewSet.EmailSettings.SmtpHostName = textBoxSmtpHost.Text;
            NewSet.EmailSettings.SmtpPort = SmtpPort;
            NewSet.EmailSettings.EnableSSL = bool.Parse(comboBoxSSLEnabled.Text);
            NewSet.EmailSettings.Login = textBoxLogin.Text;
            NewSet.EmailSettings.Password = passwordBoxPassword.Password;
            NewSet.EmailSettings.TargetEmailFrom = textBoxAccountEmail.Text;
            NewSet.EmailSettings.TargetEmail = textBoxAdresser.Text;
            NewSet.EmailSettings.EmailTitle = textBoxEmailsSubject.Text;
        }

        private void buttonAddNewInterest_Click(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(textBoxNewInterestName.Text)) {
                SelectedSection.InterestList.Add(new Interest() {
                    Name = textBoxNewInterestName.Text
                });
                textBoxNewInterestName.Text = "";
            };

        }
    }
}

