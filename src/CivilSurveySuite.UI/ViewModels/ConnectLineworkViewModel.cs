﻿using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CivilSurveySuite.Common.Helpers;
using CivilSurveySuite.Common.Models;
using CivilSurveySuite.Common.Services.Interfaces;

namespace CivilSurveySuite.UI.ViewModels
{
    /// <summary>
    /// ViewModel for ConnectLineworkView.xaml
    /// </summary>
    public class ConnectLineworkViewModel : ObservableObject
    {
        private readonly IConnectLineworkService _connectLineworkService;
        private ObservableCollection<DescriptionKey> _descriptionKeys;

        public ObservableCollection<DescriptionKey> DescriptionKeys
        {
            get => _descriptionKeys;
            set
            {
                _descriptionKeys = value;
                NotifyPropertyChanged();
            }
        }

        public DescriptionKey SelectedKey { get; set; }

        public ICommand AddRowCommand => new RelayCommand(AddRow, () => true);

        public ICommand RemoveRowCommand => new RelayCommand(RemoveRow, () => true);

        public ICommand ConnectCommand => new AsyncRelayCommand(ConnectLinework);

        public ConnectLineworkViewModel(IConnectLineworkService connectLineworkService)
        {
            _connectLineworkService = connectLineworkService;

            // Check if the service already has a fileName assigned. If not we can use the settings property.
            if (string.IsNullOrEmpty(_connectLineworkService.DescriptionKeyFile))
            {
                _connectLineworkService.DescriptionKeyFile = Properties.Settings.Default.DescriptionKeyFileName;
            }

            LoadSettings(_connectLineworkService.DescriptionKeyFile);
        }

        private void AddRow()
        {
            DescriptionKeys.Add(new DescriptionKey());
        }

        private void RemoveRow()
        {
            if (SelectedKey != null)
            {
                DescriptionKeys.Remove(SelectedKey);
            }
        }

        private async Task ConnectLinework()
        {
            await _connectLineworkService.ConnectCogoPoints(DescriptionKeys);
        }

        /// <summary>
        /// Get the last xml file loaded from settings
        /// </summary>
        public bool LoadSettings(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (File.Exists(fileName))
                {
                    DescriptionKeys = XmlHelper.ReadFromXmlFile<ObservableCollection<DescriptionKey>>(fileName);
                    return true;
                }
            }

            DescriptionKeys = new ObservableCollection<DescriptionKey>();
            return false;
        }

        /// <summary>
        /// Save XML file
        /// </summary>
        /// <param name="fileName"></param>
        public bool SaveSettings(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || DescriptionKeys == null
                                               || DescriptionKeys.Count == 0)
            {
                return false;
            }

            XmlHelper.WriteToXmlFile(fileName, DescriptionKeys);
            Properties.Settings.Default.DescriptionKeyFileName = fileName;
            Properties.Settings.Default.Save();
            return true;
        }
    }
}
