using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ToogoodChallenge
{
    sealed class TransformSystem
    {
        private static TransformSystem instance;
        private const int TargetHeaderLength = 5;   // length of the header (number of columns ... or rows for email format)
        private const int FileFormatTwoPseudoHeaderRows = 4;    // number of header rows in email format 2

        private TransformSystem() { }

        public static TransformSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TransformSystem();
                }

                return instance;
            }
        }

        /// <summary>
        /// Converts any type of file to the target desired, if given an ordered dictionary of conversions from source to target and the number of headers in the source.
        /// Assumptions: 
        /// 1. one VALUE per line
        /// 2. Missing values without a record are blank -- empty line (i.e. All records take up the same amount of lines.)
        /// 3. Every file except format 2 will have headers.
        /// 4. Newlines are not in unexpected places.
        /// </summary>
        /// <param name="file">The input file or source file as a string where each row is separated by a newline.</param>
        /// <param name="headerRows">The number of header rows present in the source.</param>
        /// <param name="conversions">A dictionary of mappings that convert a source row to a target row.
        /// The order for the dictionary is based on the target ordering. For this case that means the keys are ordered as such:
        /// AccountCode, Name, Type, Open Date, Currency.
        /// </param>
        /// <returns>The output as one string with each row separated by new lines.</returns>
        public static string TransformAnyFileFormat(string file, int headerRows, Dictionary<string, (string, Func<string, string>)> conversions)
        {
            

            // output
            string finalConversion = "";

            // add header for target
            foreach (string key in conversions.Keys)
            {
                finalConversion += key + "\n";
            }

            // split the file by newlines
            string[] fileContents =
                file.Split(
                    new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None
                );

            // header rows of source
            string[] headers = fileContents.Take(headerRows).ToArray();

            // record rows of source
            string[] records = fileContents.Skip(headerRows).ToArray();

            // number of records based on header rows; assume missing values are a blank line
            int recordCount = headerRows != 0 ? records.Length / TargetHeaderLength : (records.Length / headerRows) - 1;

            // length of record based on header rows
            int recordRowLength = headerRows == 0 ? FileFormatTwoPseudoHeaderRows : headerRows;

            // for each record
            for (int index = 0; index < recordCount; index++)
            {
                // the current record based on record length
                string[] currentRecord = records.Skip(index * recordRowLength).Take(recordRowLength).ToArray();

                // for each mapping
                foreach (KeyValuePair<string, (string, Func<string, string>)> c in conversions)
                {
                    // find the row in the current source record that converts to current target row mapping
                    int relevantRow = Array.IndexOf(headers, c.Value.Item1);
                    Func<string, string> fn = c.Value.Item2;

                    // append the record value to the return value
                    finalConversion += fn(currentRecord[relevantRow]) + "\n";
                }

            }

            return finalConversion;
        }



        /// <summary>
        /// Converts any type of csv file to the target desired, if given an ordered dictionary of conversions from source to target and whether there are headers in the source.
        /// Assumptions: 
        /// 1. one ROW per line
        /// 2. Missing values without a record are blank.
        /// 3. Commas do not exist within a column.
        /// </summary>
        /// <param name="file">The input file or source file as a string where each column within a row is split by commas and each row is separated by a newline.</param>
        /// <param name="hasHeaders">If the source has a header row</param>
        /// <param name="conversions">A dictionary of mappings that convert a source column (by zero based index) to a target column.
        /// The order for the dictionary is based on the target ordering. For this case that means the keys are ordered as such:
        /// AccountCode, Name, Type, Open Date, Currency.
        /// </param>
        /// <returns>The output as one string with each row separated by new lines in the target format.</returns>
        public static string TransformAnyCSVFormat(string file, bool hasHeader, Dictionary<string, (int, Func<string, string>)> conversions)
        {

            // output
            string finalConversion = "";

            // add header for target
            finalConversion += String.Join(",", conversions.Keys) + "\n";
           

            // split the file by newlines
            string[] fileContents =
                file.Split(
                    new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None
                );

            // record rows of source
            string[] records = fileContents.Skip(hasHeader ? 1 : 0).ToArray();

            // for each record
            foreach (string record in records)
            {
                // split record by commas -- assumes no commas within a column
                string[] recordColumns = record.Split(",").ToArray();
                string[] currentRecordConversion = new string[TargetHeaderLength];

                int recordIndex = 0;
                // for each mapping
                foreach (KeyValuePair<string, (int, Func<string, string>)> c in conversions)
                {
                    // find the column in the current source record that converts to current target row mapping
                    int relevantColumn = c.Value.Item1;
                    Func<string, string> fn = c.Value.Item2;

                    // append the record value to the return value
                    currentRecordConversion[recordIndex] = fn(recordColumns[relevantColumn]);
                    recordIndex++;
                }

                finalConversion += String.Join(",", currentRecordConversion) + "\n";
            }

            return finalConversion;
        }


        /// <summary>
        /// Ignore. Made for trying format 1 before improvements.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string TransformFileFormat1(string file)
        {
            /* Assumptions: Missing values without a record are blank -- empty line (i.e. will always be 5 rows)
             * 
             */

            string finalConversion = "";
            int headerRows = 5;

            Dictionary<string, (string, Func<string, string>)> conversions = new Dictionary<string, (string, Func<string, string>)>();

            conversions.Add("AccountCode", ("Identifier",
                x =>
                {
                    return x.Split("|")[1];
                }
            ));

            Func<string, string> typeConvert = x =>
            {
                if (x == "1")
                {
                    return "Trading";
                }
                else if (x == "2")
                {
                    return "RRSP";
                }
                else if (x == "3")
                {
                    return "RESP";
                }
                else if (x == "4")
                {
                    return "Fund";
                }
                else
                {
                    return "Error";
                }

            };

            Func<string, string> dateConvert = x =>
            {
                return DateTime.ParseExact(x, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            };

            conversions.Add("Name", ("Name", x => x));
            conversions.Add("Type", ("Type", typeConvert));
            conversions.Add("Open Date", ("Opened", dateConvert));
                
            
            
            conversions.Add("Currency", ("Currency", x =>
            {
                if (x == "CD")
                {
                    return "CAD";
                } else if (x == "US")
                {
                    return "USD";
                } else
                {
                    return "Error";
                }
            }));


                
            foreach (string key in conversions.Keys)
            {
                finalConversion += key + "\n";
            }           
                        


            string[] fileContents =
                file.Split(
                    new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None
                );

            string[] headers = fileContents.Take(headerRows).ToArray();
            string[] records = fileContents.Skip(headerRows).ToArray();

            int recordCount = headerRows != 0 ?  records.Length / TargetHeaderLength : (records.Length / headerRows) - 1;

            for (int index = 0; index < recordCount; index++)
            {
                string[] currentRecord = records.Skip(index * headerRows).Take(headerRows).ToArray();
                
                string[] convertedRecord = new string[TargetHeaderLength];
                int recordRowCounter = 0;

                foreach (KeyValuePair<string, (string, Func<string, string>)> c in conversions)
                {
                    int relevantRow = Array.IndexOf(headers, c.Value.Item1);
                    Func<string, string> fn = c.Value.Item2;

                    convertedRecord[recordRowCounter] = fn(currentRecord[relevantRow]);
                    finalConversion += fn(currentRecord[relevantRow]) + "\n";
                }
                

                

            }
            
            return finalConversion;
        }
    }
}
