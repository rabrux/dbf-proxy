using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;

namespace dbf {
    class Program {
        static void Main(string[] args) {
            string 
                argName = null,
                path = null,
                sqlQuery = null;

            // validate number of params
            if (args.Length != 4)
                UsageMessage();

            // get arguments
            for (var i = 0; i < args.Length; i++) {
                Regex r1 = new Regex( @"^-{2}(.*)" );
                Match match = r1.Match( args[ i ] );
                if ( match.Success ) {
                    argName = match.Groups[1].ToString().ToLower();
                    if ( argName == "path" ) {
                        path = args[i + 1];
                        i++;
                    } else if ( argName == "query" ) {
                        sqlQuery = args[i + 1];
                        i++;
                    }
                }
            }

            // verify if arguments are correct
            if (path == null || sqlQuery == null)
                UsageMessage();

            // verify if base path is an existing directory
            if (!Directory.Exists(@path))
                ErrorMessage("the specified path isnt directory");

            // query execution
            DataTable rs = new DataTable();
            OleDbConnection db = new OleDbConnection(@"Provider=VFPOLEDB.1;Data Source=" + path );
            db.Open();
            if (db.State == ConnectionState.Open) {
                try
                {
                    OleDbCommand query = new OleDbCommand(sqlQuery, db);
                    OleDbDataAdapter DA = new OleDbDataAdapter(query);
                    DA.Fill(rs);
                    db.Close();
                    string result = DataTableToJSONWithStringBuilder(rs);
                    Console.WriteLine(result);
                }
                catch ( OleDbException ex ) {
                    Console.WriteLine(ex.Message);
                }
                
            }
            else
                ErrorMessage("database connection error");
        }

        /**
         * DataTable JSON encoder
         * Source http://www.c-sharpcorner.com/UploadFile/9bff34/3-ways-to-convert-datatable-to-json-string-in-Asp-Net-C-Sharp/
         * */
        public static string DataTableToJSONWithStringBuilder(DataTable table) {
            var JSONString = new StringBuilder();
            if (table.Rows.Count > 0) {
                JSONString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++) {
                    JSONString.Append("{");
                    for (int j = 0; j < table.Columns.Count; j++) {
                        if (j < table.Columns.Count - 1)
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString().Trim() + "\",");
                        else if (j == table.Columns.Count - 1)
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString().Trim() + "\"");
                    }
                    if (i == table.Rows.Count - 1)
                        JSONString.Append("}");
                    else
                        JSONString.Append("},");
                }
                JSONString.Append("]");
            }
            return JSONString.ToString();
        }

        public static void UsageMessage() {
            ErrorMessage("usage: dbf --path <dbf_base_path_files> --query <query_string>");
        }

        /**
         * error function
         * */
        public static void ErrorMessage(string message) {
            Console.Error.WriteLine( message );
            Environment.Exit(2);
        }
    }
}
