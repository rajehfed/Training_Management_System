using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrainningCenter_Entities;

namespace TrainningCenter.Views.Windows
{
    /// <summary>
    /// Interaction logic for ExportConfigWindow.xaml
    /// </summary>
    public partial class ExportConfigWindow : Window
    {
        #region Fields

        private readonly object _dataToExport;
        private readonly int _recordCount;
        private string _selectedFormat = "Excel";

        #endregion

        #region Properties

        public string ExportFilePath { get; private set; }
        public string ExportFormat { get; private set; }
        public string SheetName => txtSheetName.Text;
        public string DocumentTitle => txtDocumentTitle.Text;
        public bool IncludeHeaders => chkIncludeHeaders.IsChecked ?? true;
        public bool AutoFitColumns => chkAutoFit.IsChecked ?? true;
        public bool IncludePageNumbers => chkPageNumbers.IsChecked ?? true;
        public bool IncludeDatetime => chkDateTime.IsChecked ?? true;

        #endregion

        #region Constructor
        public ExportConfigWindow(object dataToExport, int recordCount = 0)
        {
            InitializeComponent();

            _dataToExport = dataToExport;
            _recordCount = recordCount;

            InitializeWindow();
        }
        #endregion

        #region Initialization

        private void InitializeWindow()
        {
            // Set default file path to Desktop/Exports
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string exportsFolder = Path.Combine(desktopPath, "Exports");

            if (!Directory.Exists(exportsFolder))
            {
                Directory.CreateDirectory(exportsFolder);
            }

            txtFilePath.Text = exportsFolder;

            // Set record count
            tbRecordCount.Text = $"Ready to export {_recordCount:N0} record{(_recordCount != 1 ? "s" : "")}";

            // Set default selected format
            UpdateFormatSelection("Excel");
        }

        #endregion

        #region Format Selection

        private void UpdateFormatSelection(string format)
        {
            _selectedFormat = format;

            // Reset card borders
            excelCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            pdfCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

            // Reset card backgrounds
            excelCard.Background = Brushes.White;
            pdfCard.Background = Brushes.White;

            if (format == "Excel")
            {
                // Highlight Excel card
                excelCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8147E7"));
                excelCard.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0EDFF"));

                // Show Excel options, hide PDF options
                excelOptions.Visibility = Visibility.Visible;
                pdfOptions.Visibility = Visibility.Collapsed;
            }
            else if (format == "PDF")
            {
                // Highlight PDF card
                pdfCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8147E7"));
                pdfCard.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0EDFF"));

                // Show PDF options, hide Excel options
                excelOptions.Visibility = Visibility.Collapsed;
                pdfOptions.Visibility = Visibility.Visible;
            }
        }

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void excelCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rbExcel.IsChecked = true;
            UpdateFormatSelection("Excel");
        }

        private void pdfCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rbPdf.IsChecked = true;
            UpdateFormatSelection("PDF");
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();

            if (_selectedFormat == "Excel")
            {
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveDialog.DefaultExt = ".xlsx";
                saveDialog.FileName = $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }
            else if (_selectedFormat == "PDF")
            {
                saveDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDialog.DefaultExt = ".pdf";
                saveDialog.FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            }

            saveDialog.InitialDirectory = txtFilePath.Text;

            if (saveDialog.ShowDialog() == true)
            {
                txtFilePath.Text = Path.GetDirectoryName(saveDialog.FileName);
                ExportFilePath = saveDialog.FileName;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (!ValidateInputs())
                    return;

                // Generate file path if not already set by browse dialog
                if (string.IsNullOrEmpty(ExportFilePath))
                {
                    string fileName = _selectedFormat == "Excel"
                        ? $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                        : $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                    ExportFilePath = Path.Combine(txtFilePath.Text, fileName);
                }

                ExportFormat = _selectedFormat;

                // Set DialogResult to true and close
                this.DialogResult = true;
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(
                   $"An error occurred: {ex.Message}",
                   "Export Error",
                   MessageBoxButton.OK,
                   MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            // Validate sheet name for Excel
            if (_selectedFormat == "Excel" && string.IsNullOrWhiteSpace(txtSheetName.Text))
            {
                MessageBox.Show(
                    "Please enter a sheet name.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtSheetName.Focus();
                return false;
            }

            // Validate document title for PDF
            if (_selectedFormat == "PDF" && string.IsNullOrWhiteSpace(txtDocumentTitle.Text))
            {
                MessageBox.Show(
                    "Please enter a document title.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtDocumentTitle.Focus();
                return false;
            }

            // Validate file path
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show(
                    "Please select a save location.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            // Check if directory exists, create if not
            if (!Directory.Exists(txtFilePath.Text))
            {
                try
                {
                    Directory.CreateDirectory(txtFilePath.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Could not create directory: {ex.Message}",
                        "Directory Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }
    }
}
