﻿using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using _3DS_CivilSurveySuite.UI.Helpers;
using _3DS_CivilSurveySuite.UI.Models;
using _3DS_CivilSurveySuite.UI.Services.Interfaces;

namespace _3DS_CivilSurveySuite.UI.ViewModels
{
    public class CogoPointSurfaceReportViewModel : ObservableObject
    {
        private readonly ISaveFileDialogService _saveFileService;
        private ColumnHeader _selectedColumnHeader;

        public ICommand WriteToFileCommand { get; private set; }

        public ICommand GenerateReportCommand { get; private set; }

        public ICommand GenerateColumnsCommand { get; private set; }

        public ICommand MoveColumnUpCommand { get; private set; }

        public ICommand MoveColumnDownCommand { get; private set; }

        public ICogoPointSurfaceReportService ReportService { get; }

        public ColumnHeader SelectedColumnHeader
        {
            get => _selectedColumnHeader;
            set => SetProperty(ref _selectedColumnHeader, value);
        }

        public ObservableCollection<SortColumnHeader> SortingHeaders { get; }
            = new ObservableCollection<SortColumnHeader>();

        public CogoPointSurfaceReportViewModel(ICogoPointSurfaceReportService cogoPointSurfaceReportService,
            ISaveFileDialogService saveFileDialogService)
        {
            ReportService = cogoPointSurfaceReportService;
            _saveFileService = saveFileDialogService;

            InitCommands();
        }

        private void InitCommands()
        {
            GenerateReportCommand  = new AsyncRelayCommand(UpdateReportData);
            WriteToFileCommand     = new RelayCommand(WriteFile, () => true);
            GenerateColumnsCommand = new RelayCommand(ReportService.BuildColumnHeaders, () => true);
            MoveColumnUpCommand    = new RelayCommand<ColumnHeader>(ReportService.ColumnProperties.MoveUp);
            MoveColumnDownCommand  = new RelayCommand<ColumnHeader>(ReportService.ColumnProperties.MoveDown);
        }

        private void WriteFile()
        {
            _saveFileService.DefaultExt = ".csv";
            _saveFileService.Filter = "CSV Files (*.csv)|*.csv";

            if (_saveFileService.ShowDialog() != true)
            {
                return;
            }

            //var data = ReportService.WriteDataTable();
            //TODO: Add way to select delimiter.
            var data = DataView.ToCsv();
            var fileName = _saveFileService.FileName;

            FileHelpers.WriteFile(fileName, true, data);
        }

        public DataView DataView => ReportService.DataTable?.DefaultView;

        private async Task UpdateReportData()
        {
            await Task.Run(() => ReportService.GenerateReport());
            NotifyPropertyChanged(nameof(DataView));
            GenerateSortString();
        }

        //TODO: Should I move this to the sort control?
        private void GenerateSortString()
        {
            // Build a sorting string.
            var sb = new StringBuilder();

            for (var i = 0; i < SortingHeaders.Count; i++)
            {
                SortColumnHeader header = SortingHeaders[i];
                sb.Append($"{header.ColumnHeader.HeaderText} {header.SortDirection}");

                if (i != SortingHeaders.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            DataView.Sort = sb.ToString();
        }
    }
}
