using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GameRentalManagement.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace GameRentalManagement.UserControls
{
    public partial class ReportPage : UserControl
    {
        GameRentalDbContext con = new GameRentalDbContext();

        public ReportPage()
        {
            InitializeComponent();
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (cbMonth.SelectedItem == null || string.IsNullOrWhiteSpace(txtYear.Text))
            {
                MessageBox.Show("Please select month and enter year.", "Missing Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtYear.Text.Trim(), out int year))
            {
                MessageBox.Show("Invalid year format.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int month = int.Parse(((ComboBoxItem)cbMonth.SelectedItem).Content.ToString());

            var rentals = con.Rentals
                .Where(r => r.RentalDate.Month == month && r.RentalDate.Year == year)
                .Include(r => r.Customer)
                .Include(r => r.ProcessedByNavigation)
                .Include(r => r.RentalDetails)
                .ThenInclude(rd => rd.Game)
                .ToList();

            if (rentals.Count == 0)
            {
                MessageBox.Show("No rentals found for the selected month and year.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Report_Borrowed-Returned_GameRental.xlsx");
            string exportDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Reports");
            Directory.CreateDirectory(exportDir);
            string exportPath = Path.Combine(exportDir, $"Report_Borrowed-Returned_GameRental_{month}_{year}.xlsx");


            FileInfo templateFile = new FileInfo(templatePath);
            FileInfo newFile = new FileInfo(exportPath);

            if (!templateFile.Exists)
            {
                MessageBox.Show("Template file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newFile.Exists) newFile.Delete();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(newFile, templateFile))
                {
                    var ws = package.Workbook.Worksheets[0];

                    ws.Cells["C2"].Value = $"{month}/{year}";
                    ws.Cells["A6:N300"].Clear();

                    int startRow = 6;
                    int no = 1;
                    int totalRentals = rentals.Count;
                    int totalReturns = 0;
                    decimal totalNetRevenue = 0;

                    foreach (var rental in rentals)
                    {
                        bool isReturned = rental.ReturnDate != null;
                        if (isReturned) totalReturns++;

                        decimal rentalTotal = 0;
                        decimal overdueFee = 0;

                        foreach (var detail in rental.RentalDetails)
                        {
                            decimal lineTotal = detail.Quantity * detail.PriceAtRent;
                            rentalTotal += lineTotal;

                            decimal vat = lineTotal * 0.1m;
                            decimal netRevenue = lineTotal - vat;

                            ws.Cells[startRow, 1].Value = no++; // No.
                            ws.Cells[startRow, 2].Value = rental.RentalDate.ToString("dd/MM/yyyy"); // Rental Date
                            ws.Cells[startRow, 3].Value = rental.DueDate.ToString("dd/MM/yyyy");     // ✅ Due Date
                            ws.Cells[startRow, 4].Value = rental.ReturnDate?.ToString("dd/MM/yyyy") ?? ""; // Return Date
                            ws.Cells[startRow, 5].Value = rental.Status;            // Status
                            ws.Cells[startRow, 6].Value = rental.RentalId;          // Rental ID
                            ws.Cells[startRow, 7].Value = detail.Game.GameName;     // Game Title
                            ws.Cells[startRow, 8].Value = rental.Customer.FullName; // Customer
                            ws.Cells[startRow, 9].Value = detail.Quantity;          // Quantity
                            ws.Cells[startRow, 10].Value = detail.PriceAtRent;      // Unit Price
                            ws.Cells[startRow, 11].Value = lineTotal;               // Total
                            ws.Cells[startRow, 12].Value = vat;                     // VAT
                            ws.Cells[startRow, 13].Value = netRevenue;              // Net Revenue
                            ws.Cells[startRow, 14].Value = rental.ProcessedByNavigation?.Username; // Processed By

                            startRow++;
                        }

                        // Tính doanh thu
                        if (isReturned)
                        {
                            if (rental.TotalPrice.HasValue)
                                totalNetRevenue += rental.TotalPrice.Value;
                            else
                                totalNetRevenue += rentalTotal;
                        }
                        else
                        {
                            // Nếu chưa trả, kiểm tra quá hạn chưa
                            if (rental.DueDate < DateOnly.FromDateTime(DateTime.Now))
                            {
                                int overdueDays = DateOnly.FromDateTime(DateTime.Now).DayNumber - rental.DueDate.DayNumber;
                                overdueFee = overdueDays * 5000;
                                totalNetRevenue += rentalTotal + overdueFee;
                            }
                            else
                            {
                                // Chưa quá hạn thì không cộng thêm
                                totalNetRevenue += rentalTotal;
                            }
                        }
                    }

                    // Format các số tiền
                    ws.Cells[$"I6:L{startRow - 1}"].Style.Numberformat.Format = "#,##0";

                    // Tổng kết
                    ws.Cells[startRow + 1, 16].Value = "Total Rentals:";
                    ws.Cells[startRow + 1, 17].Value = totalRentals;

                    ws.Cells[startRow + 2, 16].Value = "Total Returns:";
                    ws.Cells[startRow + 2, 17].Value = totalReturns;

                    ws.Cells[startRow + 3, 16].Value = "Total Revenue (VND):";
                    ws.Cells[startRow + 3, 17].Value = totalNetRevenue;
                    ws.Cells[startRow + 3, 17].Style.Numberformat.Format = "#,##0";

                    package.Save();
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = exportPath,
                    UseShellExecute = true
                });

                txtStatus.Text = $"Exported to: {exportPath}";
                MessageBox.Show("Report exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExportMonthlyReportWithSummary_Click(object sender, RoutedEventArgs e)
        {
            if (cbMonth.SelectedItem == null || string.IsNullOrWhiteSpace(txtYear.Text))
            {
                MessageBox.Show("Please select month and enter year.", "Missing Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtYear.Text.Trim(), out int year))
            {
                MessageBox.Show("Invalid year format.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int month = int.Parse(((ComboBoxItem)cbMonth.SelectedItem).Content.ToString());

            var rentals = con.Rentals
                .Where(r => r.RentalDate.Month == month && r.RentalDate.Year == year)
                .Include(r => r.RentalDetails)
                .ThenInclude(d => d.Game)
                .ToList();

            if (rentals.Count == 0)
            {
                MessageBox.Show("No rentals found for the selected month and year.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Report_Borrowed-Returned_GameRental.xlsx");
            string exportDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Reports");
            Directory.CreateDirectory(exportDir);
            string exportPath = Path.Combine(exportDir, $"Report_Borrowed-Returned_GameRental_{month}_{year}.xlsx");

            FileInfo templateFile = new FileInfo(templatePath);
            FileInfo newFile = new FileInfo(exportPath);

            if (!templateFile.Exists)
            {
                MessageBox.Show("Template file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newFile.Exists) newFile.Delete();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(newFile, templateFile))
                {
                    var ws = package.Workbook.Worksheets[0];

                    int startRow = 6;
                    int currentRow = startRow;
                    int no = 1;

                    var groupedGames = rentals
                        .SelectMany(r => r.RentalDetails)
                        .GroupBy(d => d.Game)
                        .Select(g => new
                        {
                            Game = g.Key,
                            Borrowed = g.Sum(x => x.Quantity),
                            Returned = g.Sum(x => x.Rental.ReturnDate != null ? x.Quantity : 0)
                        })
                        .ToList();

                    int totalBorrowed = 0;
                    int totalReturned = 0;

                    foreach (var item in groupedGames)
                    {
                        ws.Cells[currentRow, 1].Value = no++; // No.
                        ws.Cells[currentRow, 2].Value = item.Game.GameId;
                        ws.Cells[currentRow, 3].Value = item.Game.GameName;
                        ws.Cells[currentRow, 4].Value = item.Game.Genre;
                        ws.Cells[currentRow, 5].Value = item.Game.Status ? "Available" : "Not Available";
                        ws.Cells[currentRow, 6].Value = item.Game.Quantity;
                        ws.Cells[currentRow, 7].Value = item.Borrowed;
                        ws.Cells[currentRow, 8].Value = item.Returned;

                        totalBorrowed += item.Borrowed;
                        totalReturned += item.Returned;

                        currentRow++;
                    }

                    // Ghi tổng cộng vào cột K (cột bên phải của "Total Borrowed:" tại cột J)
                    for (int row = 1; row <= 100; row++)
                    {
                        var label = ws.Cells[row, 10].Text; // Column J
                        if (label == "Total Borrowed:")
                        {
                            ws.Cells[row, 11].Value = totalBorrowed; // Column K
                        }
                        else if (label == "Total Returned:")
                        {
                            ws.Cells[row, 11].Value = totalReturned; // Column K
                        }
                    }

                    package.Save();
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = exportPath,
                    UseShellExecute = true
                });

                txtStatus.Text = $"Exported to: {exportPath}";
                MessageBox.Show("Monthly report exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


}