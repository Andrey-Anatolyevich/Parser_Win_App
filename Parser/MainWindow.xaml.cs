using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using IParserPlugin;
using System.Threading;

namespace Parser {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // Taskbar icon
        private System.Windows.Forms.NotifyIcon MyNotifyIcon;
        // main parser task
        Task T = null;
        // task cancellation token
        CancellationTokenSource TokSor = null;
        CancellationToken TaskToken;
        // parser unit 
        Code.ParserProcess ParserUnit = null;

        public MainWindow() {
            // задали иконку для окна
            this.Icon = Imaging.CreateBitmapSourceFromHIcon(
                Properties.Resources.bak.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            // инициализация 
            MyNotifyIcon = new System.Windows.Forms.NotifyIcon();
            // задали иконку для значка в области уведомлений
            MyNotifyIcon.Icon = Properties.Resources.bak;
            
            MyNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MyNotifyIcon_MouseDoubleClick);
            this.StateChanged += Window_StateChanged;
            
            InitializeComponent();
        }
        //private void OnBalloonShown(object sender, EventArgs e) {
        //    MyNotifyIcon.Click -= OnClickNotifyIcon;
        //}
        //private void OnBalloonClosed(object sender, EventArgs e) {
        //    MyNotifyIcon.Click += OnClickNotifyIcon;
        //}
        //private void OnClickNotifyIcon(object sender, EventArgs e) {
        //    MyNotifyIcon.ShowBalloonTip(1000, "Unshipped items", 
        //        labelTotalUnshippedCounter.Content.ToString(), System.Windows.Forms.ToolTipIcon.None);
        //}
        // on notifyicondoubleclick
        void MyNotifyIcon_MouseDoubleClick(object sender,
            System.Windows.Forms.MouseEventArgs e) {
            this.WindowState = WindowState.Normal;
        }
        private void Window_StateChanged(object sender, EventArgs e) {
            if (this.WindowState == WindowState.Minimized) {
                this.ShowInTaskbar = false;
                MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                MyNotifyIcon.BalloonTipText = "Minimized the app ";
                MyNotifyIcon.ShowBalloonTip(400);
                MyNotifyIcon.Visible = true;
                //MyNotifyIcon.Click += OnClickNotifyIcon;
                //MyNotifyIcon.BalloonTipShown += OnBalloonShown;
                //MyNotifyIcon.BalloonTipClosed += OnBalloonClosed;
            }
            else if (this.WindowState == WindowState.Normal) {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void Launch_Click(object sender, RoutedEventArgs e) {
            if (T != null && !T.IsCompleted) {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (Action)(() => {
                        MessageBox.Show("There is one task running");
                    }));
            }
            else {
                InitNewParserUnit();
                T = new Task(ParserUnit.Launch, TaskToken, TaskCreationOptions.LongRunning);
                T.Start();
                // активируем кнопку отмены таска.
                buttonCancel.IsEnabled = true;
                // активируем кнопку паузы
                MenuItemUpdateSettings.IsEnabled = true;
                Launch.IsEnabled = false;
            }
        }

        private void InitNewParserUnit() {
            CancellationToken TaskToken = GetNewCancelToken();
            // запустим процесс
            ParserUnit = new Code.ParserProcess(TaskToken);
            ParserUnit.NewRequestEvent += TaskNewRequest;
            ParserUnit.NewEMailEvent += TaskNewEMail;
            ParserUnit.NewUnshippedUnits += NewUnshippedUnits;
            ParserUnit.NewIdleEvent += TaskNewDelay;
            ParserUnit.NewAllUnitsUpdate += ParserUnit_NewAllUnitsUpdate;
            ParserUnit.NewCancelledByToken += ParserUnit_NewCancelledByToken;
            ParserUnit.NewSettingsLoaded += ParserUnit_NewSettingsLoaded;
            ParserUnit.NewException += ParserUnit_NewException;
        }

        private void ParserUnit_NewException(Exception ex) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                (Action)(() => {
                    MessageBox.Show("Exception happened: \r\n" + ex.ToString());
                }));
            Launch.IsEnabled = true;
            buttonCancel.IsEnabled = false;
            MenuItemUpdateSettings.IsEnabled = false;
        }

        private void ParserUnit_NewSettingsLoaded() {
            this.Dispatcher.Invoke(() => {
                SettingsLoadedDTLabel.Text = DateTime.Now.ToString("HH:mm:ss");
                MenuItemUpdateSettings.IsEnabled = true;
            });
        }

        private void ParserUnit_NewCancelledByToken() {
            // таск отменён по токену.
            Launch.IsEnabled = true;
        }

        private void ParserUnit_NewAllUnitsUpdate(IEnumerable<UnitDetails> listOfUnits) {
            this.Dispatcher.Invoke(() => {
                dataGrid.IsEnabled = false;
                dataGrid.ItemsSource = null;
                dataGrid.Items.Refresh();
                dataGrid.ItemsSource = listOfUnits;
                dataGrid.Items.Refresh();
                dataGrid.IsEnabled = true;
            });
        }

        private void TaskNewRequest(DateTime dt) {
            this.Dispatcher.Invoke(() => {
                this.labelLastOpDT.Content = dt.ToLongTimeString();
                this.labelTotalRequestsCounter.Content = int.Parse(labelTotalRequestsCounter.Content.ToString()) + 1;
            });
        }
		/// <summary>
		/// Обновить время и счётчик e-mail
		/// </summary>
		/// <param name="dt"></param>
		private void TaskNewEMail(DateTime dt, int number) {
			this.Dispatcher.Invoke(() => {
				this.labelLastEmailOpDT.Content = dt.ToLongTimeString();
				this.labelTotalEMailsCounter.Content = int.Parse(labelTotalEMailsCounter.Content.ToString()) + 1;
                MyNotifyIcon.ShowBalloonTip(3000, "New E-MAIL", "Just sent E-Mail with " + number + " units.", System.Windows.Forms.ToolTipIcon.Info);
            });
		}
        /// <summary>
		/// Обновить время сна
		/// </summary>
		/// <param name="dt"></param>
		private void TaskNewDelay(DateTime dt) {
            this.Dispatcher.Invoke(() => {
                this.labelSleepUntilData.Content = dt.ToLongTimeString();
            });
        }
        /// <summary>
        /// Обновить кол-во неотправленных объяв
        /// </summary>
        /// <param name="dt"></param>
        private void NewUnshippedUnits(int number, int shipped) {
			this.Dispatcher.Invoke(() => {
				this.labelTotalUnshippedCounter.Content = number;
				this.labelTotalShippedCounter.Content = shipped;
			});
		}

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (sender != null) {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1) {
                    DataGridRow dgr = grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem) as DataGridRow;
                    if(dgr != null) {
                        UnitDetails Unit =  dgr.Item as UnitDetails;
                        if(Unit != null) {
                            System.Diagnostics.Process.Start(Unit.Url);
                        }
                    }
                }
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            if (T != null)
                if (T.Status == TaskStatus.Running) {
                    TokSor.Cancel();
                    buttonCancel.IsEnabled = false;
                    MenuItemUpdateSettings.IsEnabled = false;
                    SettingsLoadedDTLabel.Text = "";
                }
        }

        private void buttonUpdateSettings_Click(object sender, RoutedEventArgs e) {
            if (ParserUnit != null) {
                ParserUnit.RequestUpdateSettings();
                MenuItemUpdateSettings.IsEnabled = false;
            }
        }

        private void MenuItemOpenSettingsWindow_Click(object sender, RoutedEventArgs e) {
            Code.Set.SettGreatUnit Settings = new Code.Set.SettGreatUnit();
            try {
                Settings.LoadSettings();
                SettingsEditor SE = new SettingsEditor(Settings);
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (Action)(() => {
                        SE.ShowDialog();
                    }));
            } catch {
                MessageBox.Show("Error on loading settings from file.");
            }
        }
        private CancellationToken GetNewCancelToken() {
            // создали токен
            TokSor = new CancellationTokenSource();
            return TokSor.Token;
        }
        /// <summary>
        /// Перехватываем закрытие окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            MessageBoxResult UserAnswer = MessageBoxResult.Cancel;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() => {
                UserAnswer = MessageBox.Show(
                    "Выходим?"
                    , "Подтверждение завершения программы"
                    , MessageBoxButton.YesNoCancel
                    , MessageBoxImage.Question
                    );
                if(UserAnswer == MessageBoxResult.Yes) {
                    ProceedExit();
                }
            }));
            e.Cancel = true;
        }
        /// <summary>
        /// Завершение выполнения программы
        /// </summary>
        private void ProceedExit() {
            Environment.Exit(0);
        }
    }
}
