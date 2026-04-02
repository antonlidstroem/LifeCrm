using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using LifeCrm.Core.Interfaces;

namespace LifeCrm.Infrastructure.Services
{
    public class CsvService : ICsvService
    {
        public async Task<byte[]> ExportAsync<T>(IEnumerable<T> rows)
        {
            await using var ms = new MemoryStream();
            await using var writer = new StreamWriter(ms);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
            await csv.WriteRecordsAsync(rows);
            await writer.FlushAsync();
            return ms.ToArray();
        }

        public async Task<(List<T> Rows, List<CsvParseError> Errors)> ImportAsync<T>(byte[] csvBytes)
        {
            var rows   = new List<T>();
            var errors = new List<CsvParseError>();
            var line   = 0;

            await using var ms = new MemoryStream(csvBytes);
            using var reader   = new StreamReader(ms);
            using var csv      = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null, MissingFieldFound = null
            });

            while (await csv.ReadAsync())
            {
                line++;
                if (line == 1 && csv.ReadHeader()) continue;
                try
                {
                    var record = csv.GetRecord<T>();
                    if (record is not null) rows.Add(record);
                }
                catch (Exception ex)
                {
                    errors.Add(new CsvParseError(line, ex.Message, csv.Parser.RawRecord.TrimEnd()));
                }
            }
            return (rows, errors);
        }
    }
}
