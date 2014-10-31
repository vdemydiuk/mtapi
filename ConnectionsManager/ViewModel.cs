using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MTApiService;
using System.ComponentModel;
using System.Windows;

namespace ConnectionsManager
{
    class ViewModel : INotifyPropertyChanged
    {
        #region Properties

        private ObservableCollection<MtConnectionProfile> _ConnectionProfiles = new ObservableCollection<MtConnectionProfile>();
        public ObservableCollection<MtConnectionProfile> ConnectionProfiles
        {
            get { return _ConnectionProfiles; }
        }

        public DelegateCommand AddProfileCommand { get; private set; }
        public DelegateCommand DeleteProfileCommand { get; private set; }
        public DelegateCommand ShowAboutCommand { get; private set; }

        private MtConnectionProfile _SelectedProfile;
        public MtConnectionProfile SelectedProfile 
        {
            get { return _SelectedProfile; }
            set
            {
                _SelectedProfile = value;
                OnPropertyChanged("SelectedProfile");
                DeleteProfileCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Public Methods
        public ViewModel()
        {
            AddProfileCommand = new DelegateCommand(ExecuteAddProfile);
            DeleteProfileCommand = new DelegateCommand(ExecuteDeleteProfile, CanExecuteDeleteProfile);
            ShowAboutCommand = new DelegateCommand(ExecuteShowAbout);
        }

        public void Initialize()
        {
            var profiles = MtRegistryManager.LoadConnectionProfiles();
            if (profiles != null)
            {
                foreach(var prof in profiles)
                {
                    ConnectionProfiles.Add(prof);
                }
            }
        }
        #endregion

        #region Private Methods
        private void ExecuteAddProfile(object o)
        {
            var dlg = new AddProfileDialog(App.Current.MainWindow);
            var result = dlg.ShowDialog();

            if (result == true)
            {
                var profile = new MtConnectionProfile(dlg.ProfileName);
                profile.Host = dlg.Host;
                profile.Port = int.Parse(dlg.Port);

                MtRegistryManager.AddConnectionProfile(profile);
                ConnectionProfiles.Add(profile);
            }
        }

        private bool CanExecuteDeleteProfile(object o)
        {
            return SelectedProfile != null;
        }

        private void ExecuteDeleteProfile(object o)
        {
            var result = MessageBox.Show("Do you want to delete connection profile '" + SelectedProfile.Name + "' ?"
                , "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MtRegistryManager.RemoveConnectionProfile(SelectedProfile.Name);
                ConnectionProfiles.Remove(SelectedProfile);
            }
        }

        private void ExecuteShowAbout(object o)
        {
            var dlg = new AboutWindow(App.Current.MainWindow);
            dlg.ShowDialog();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Fields
        #endregion
    }
}
