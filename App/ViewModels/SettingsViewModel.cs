using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private bool _isFullViewMode = true;

        public bool IsFullViewMode
        {
            get => _isFullViewMode;
            set
            {
                if (_isFullViewMode != value)
                {
                    _isFullViewMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCompactViewMode));
                    SaveSettings();
                }
            }
        }

        public bool IsCompactViewMode
        {
            get => !_isFullViewMode;
            set
            {
                IsFullViewMode = !value;
            }
        }

        public SettingsViewModel()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            IsFullViewMode = Preferences.Get("IsFullViewMode", true);
        }

        private void SaveSettings()
        {
            Preferences.Set("IsFullViewMode", IsFullViewMode);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
