using System;
using System.Collections.Generic;
using System.Globalization;

namespace ToogoodChallenge
{
    class Program
    {
        static void Main(string[] args)
        {
            // I assumed one record per row instead of one value per row (one record in many adjacent rows).
            string valuePerRow = "Identifier\nName\nType\nOpened\nCurrency\n123|AbcCode\nMy Account\n2\n01-01-2018\nCD";
            string recordPerRow1 = "Identifier,Name,Type,Opened,Currency\n123|AbcCode,My Account,2,01-01-2018,CD";
            string recordPerRow2 = "My Account,2,C,";

            Dictionary<string, (string, Func<string, string>)> maps = new Dictionary<string, (string, Func<string, string>)>();

            maps.Add("AccountCode", ("Identifier",
                x =>
                {
                    return x.Split("|")[1];
                }
            ));

            static string typeConvert(string x)
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

            }

            static string dateConvert(string x)
            {
                return DateTime.ParseExact(x, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            }

            maps.Add("Name", ("Name", x => x));
            maps.Add("Type", ("Type", (Func<string, string>)typeConvert));
            maps.Add("Open Date", ("Opened", (Func<string, string>)dateConvert));



            maps.Add("Currency", ("Currency", x =>
            {
                if (x == "CD")
                {
                    return "CAD";
                }
                else if (x == "US")
                {
                    return "USD";
                }
                else
                {
                    return "Error";
                }
            }
            ));

            Dictionary<string, (int, Func<string, string>)> format1Conversions = new Dictionary<string, (int, Func<string, string>)>();

            format1Conversions.Add("AccountCode", (0,
                x =>
                {
                    return x.Split("|")[1];
                }
            ));


            format1Conversions.Add("Name", (1, x => x));
            format1Conversions.Add("Type", (2, (Func<string, string>)typeConvert));
            format1Conversions.Add("Open Date", (3, (Func<string, string>)dateConvert));



            format1Conversions.Add("Currency", (4, x =>
            {
                if (x == "CD")
                {
                    return "CAD";
                }
                else if (x == "US")
                {
                    return "USD";
                }
                else
                {
                    return "Error";
                }
            }
            ));



            Console.WriteLine("Format given in email:");
            Console.WriteLine(TransformSystem.TransformAnyFileFormat(valuePerRow, 5, maps));


            // Format 1
            Console.WriteLine("--------------------------------\nCSV format 1:");
            Console.WriteLine(TransformSystem.TransformAnyCSVFormat(recordPerRow1, true, format1Conversions));

            Dictionary<string, (int, Func<string, string>)> format2Conversions = new Dictionary<string, (int, Func<string, string>)>();
            format2Conversions.Add("AccountCode", (3, x => x));
            format2Conversions.Add("Name", (0, x => x));
            format2Conversions.Add("Type", (1, (Func<string, string>)typeConvert));
            format2Conversions.Add("Open Date", (0, x => ""));
            format2Conversions.Add("Currency", (2, x =>
            {
                if (x == "C")
                {
                    return "CAD";
                } else if (x == "U")
                {
                    return "USD";
                }
                else
                {
                    return "Err";
                }
            }
            ));

            // Format 2
            Console.WriteLine("--------------------------------\nCSV format 2:");
            Console.WriteLine(TransformSystem.TransformAnyCSVFormat(recordPerRow2, false, format2Conversions));


            string example1 = "Title,Type,Opened Date,Name,Currency\nAbcCode,RRSP,2018-01-01,MyAccount,Canadian";
            Dictionary<string, (int, Func<string, string>)> example1Conversions = new Dictionary<string, (int, Func<string, string>)>();
            example1Conversions.Add("AccountCode", (0, x => x));
            example1Conversions.Add("Name", (3, x => x));
            example1Conversions.Add("Type", (1, x => x));
            example1Conversions.Add("Open Date", (2, x => x));
            example1Conversions.Add("Currency", (4, x =>
            {
                if (x == "Canadian")
                {
                    return "CAD";
                } else
                {
                    return "USD";
                }
            }
            ));

            // example 1
            Console.WriteLine("--------------------------------\nCSV format example 1:");
            Console.WriteLine(example1);
            Console.WriteLine("Output:");
            Console.WriteLine(TransformSystem.TransformAnyCSVFormat(example1, true, example1Conversions));


            // example 2
            string example2 = "Abc|1,2019-01-01,RRSP,CAD,Account 1\nAbc|2,,RESP,USD,Account 2\nDef|1,2019-01-02,Trading,USD,Account 3";
            Dictionary<string, (int, Func<string, string>)> example2Conversions = new Dictionary<string, (int, Func<string, string>)>();
            example2Conversions.Add("AccountCode", (0, x => x.Replace("|", "-")));
            example2Conversions.Add("Name", (4, x => x));
            example2Conversions.Add("Type", (2, x => x));
            example2Conversions.Add("Open Date", (1, x => x));
            example2Conversions.Add("Currency", (3, x => x));


            Console.WriteLine("--------------------------------\nCSV format example 2:");
            Console.WriteLine(example2);
            Console.WriteLine("Output:");
            Console.WriteLine(TransformSystem.TransformAnyCSVFormat(example2, false, example2Conversions));
        }

    }

    
}
