using ClosedXML.Excel;

namespace TimeManager.Backend.Services
{
    public interface IExcelService
    {
        public (List<Dictionary<string, string>>, string? error) ParseExcelFileToList(IFormFile excelFile);
    }

    public class ExcelService : IExcelService
    {
        public (List<Dictionary<string, string>>, string? error) ParseExcelFileToList(IFormFile excelFile)
        {
            string? error;

            var importedDatas = new List<Dictionary<string, string>>();

            using (var stream = excelFile.OpenReadStream())
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        Console.WriteLine("no worksheet");
                        error = "The Excel file doesnot contain any worksheet";
                        return ([], error);
                    }

                    var lastRow = worksheet.LastRowUsed().RowNumber();

                    var headerRow = worksheet.Row(1);
                    int colNum = 1;

                    for (int rowNum = 2; rowNum <= lastRow; rowNum++)
                    {
                        var row = worksheet.Row(rowNum);
                        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        foreach (var cell in headerRow.CellsUsed())
                        {
                            string headerText = cell.GetValue<string>().Trim();
                            data[headerText] = row.Cell(colNum).GetValue<string>().Trim();
                            colNum++;
                        }

                        colNum = 1;
                        importedDatas.Add(data);
                    }
                }
            }

            Console.WriteLine(importedDatas);

            return (importedDatas, null);
        }
    }
}
