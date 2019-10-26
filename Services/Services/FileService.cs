using Services.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace YMParseWeb.Services
{
    public class FileService
    {
        /// <summary>
        /// Создает Excel файл.
        /// </summary>
        public string CreateExcelFile(List<ProductModel> items, string searchQuery)
        {
            // Замена пробелов на знак "_"
            searchQuery = searchQuery.Replace(" ", "_");

            //string fileName = searchQuery + "-" + Guid.NewGuid() + ".xlsx",
            //    path = Path.Combine(@"C:\Users\yasaviev.ir\Desktop\YMParseFiles", fileName);

            string fileName = searchQuery + "-" + Guid.NewGuid() + ".xlsx",
                path = Path.Combine(@"C:\Users\Administrator\Desktop\YMParseFiles", fileName);

            // Открыть Excel
            Excel.Application ex = new Excel.Application();

            // Открытие книги
            Excel.Workbook workBook = ex.Workbooks.Add();

            // Получение рабочего листа (1)
            Excel.Worksheet sheet = (Excel.Worksheet)workBook.Sheets[1];

            // Название листа(вкладки снизу)
            sheet.Name = "Результат парсинга";

            // Захват диапозона ячеек для заголовков и установка font.bold
            Excel.Range headersRange = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1, 4]];
            headersRange.Font.Bold = true;
            sheet.Cells[1, 1] = "Название";
            sheet.Cells[1, 2] = "Цена";
            sheet.Cells[1, 3] = "Описание";

            // Задает ширину для столбцов
            sheet.Columns[1].ColumnWidth = 42;
            sheet.Columns[2].ColumnWidth = 10;
            sheet.Columns[3].ColumnWidth = 57;

            // Включает перенос текста
            sheet.Columns[1].WrapText = true;
            sheet.Columns[3].WrapText = true;

            // Запись данных в ячейки
            int i = 1;
            foreach (var product in items)
            {
                sheet.Hyperlinks.Add(
                    sheet.Cells[i + 1, 1],
                    product.Link.ToString(),
                    string.Empty,
                    string.Empty,
                    product.Name.ToString()
                );
                sheet.Cells[i + 1, 2] = product.Price;
                //sheet.Cells[i + 1, 3] = product.Description;
                i++;
            }

            // Сохранение книги
            ex.Application.ActiveWorkbook.SaveAs(path, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            // Закрытие сервера Excel.
            ex.Quit();

            return fileName;
        }
    }
}