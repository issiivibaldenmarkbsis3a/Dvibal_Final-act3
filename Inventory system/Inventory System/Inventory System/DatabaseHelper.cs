using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace Inventory_System
{
    public static class DatabaseHelper
    {
        private static string _connectionString = null;

        #region Database Configuration
        public static string GetDatabasePath()
        {
            string projectDb = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\inventorydb.accdb"));
            if (File.Exists(projectDb))
            {
                return projectDb;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventorydb.accdb");
        }

        public static string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(_connectionString))
                return _connectionString;

            string dbPath = GetDatabasePath();

            string[] providers = new string[]
            {
                "Microsoft.ACE.OLEDB.12.0",
                "Microsoft.ACE.OLEDB.16.0"
            };

            foreach (string provider in providers)
            {
                string testCs = string.Format("Provider={0};Data Source={1};Persist Security Info=False;", provider, dbPath);
                try
                {
                    using (OleDbConnection conn = new OleDbConnection(testCs))
                    {
                        conn.Open();
                        _connectionString = testCs;
                        return _connectionString;
                    }
                }
                catch
                {
                }
            }

            _connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Persist Security Info=False;", dbPath);
            return _connectionString;
        }

        public static OleDbConnection GetConnection()
        {
            return new OleDbConnection(GetConnectionString());
        }

        private static HashSet<string> GetUserTableColumns(OleDbConnection conn)
        {
            HashSet<string> columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM Users WHERE 1=0", conn))
                using (OleDbDataReader reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    DataTable schemaTable = reader.GetSchemaTable();
                    if (schemaTable != null)
                    {
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            columns.Add(row["ColumnName"].ToString());
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    DataTable dt = conn.GetSchema("Columns", new string[] { null, null, "Users" });
                    foreach (DataRow row in dt.Rows)
                    {
                        columns.Add(row["COLUMN_NAME"].ToString());
                    }
                }
                catch { }
            }

            return columns;
        }
        #endregion

        #region Database Setup
        public static void InitializeDatabase()
        {
            using (OleDbConnection conn = GetConnection())
            {
                conn.Open();

                string[] tablesToRemove = new string[] { "Products", "StockTransac", "StockTransactions" };
                foreach (string tbl in tablesToRemove)
                {
                    try
                    {
                        using (OleDbCommand dropCmd = new OleDbCommand("DROP TABLE [" + tbl + "]", conn))
                        {
                            dropCmd.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                    }
                }

                HashSet<string> columns = GetUserTableColumns(conn);
                if (columns.Count == 0)
                {
                    string createUsersSql = "CREATE TABLE Users (" +
                                            "UserID AUTOINCREMENT PRIMARY KEY, " +
                                            "Username VARCHAR(50), " +
                                            "PasswordHarsh VARCHAR(100), " +
                                            "FullName VARCHAR(100), " +
                                            "UserRole VARCHAR(50), " +
                                            "IsActive YESNO" +
                                            ");";
                    using (OleDbCommand createCmd = new OleDbCommand(createUsersSql, conn))
                    {
                        createCmd.ExecuteNonQuery();
                    }
                }

                using (OleDbCommand countCmd = new OleDbCommand("SELECT COUNT(*) FROM Users", conn))
                {
                    int userCount = Convert.ToInt32(countCmd.ExecuteScalar());
                    if (userCount == 0)
                    {
                        string dummy;
                        RegisterUser("Administrator", "admin", "admin123", out dummy);
                    }
                }
            }
        }
        #endregion

        #region User Authentication
        public static bool AuthenticateUser(string username, string password, out string fullName)
        {
            fullName = string.Empty;

            using (OleDbConnection conn = GetConnection())
            {
                conn.Open();

                HashSet<string> columns = GetUserTableColumns(conn);

                string pwdCol = "PasswordHarsh";
                if (columns.Contains("PasswordHarsh")) pwdCol = "PasswordHarsh";
                else if (columns.Contains("Password")) pwdCol = "[Password]";
                else if (columns.Contains("PasswordHash")) pwdCol = "PasswordHash";
                else if (columns.Contains("Pass")) pwdCol = "Pass";

                string nameCol = "FullName";
                if (columns.Contains("FullName")) nameCol = "FullName";
                else if (columns.Contains("Name")) nameCol = "Name";

                string query = string.Format("SELECT {0}, {1} FROM Users WHERE Username = ?", nameCol, pwdCol);
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.Add("?", OleDbType.VarWChar).Value = username.Trim();

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string dbPassword = reader[1] != DBNull.Value ? reader[1].ToString() : string.Empty;

                            if (dbPassword == password)
                            {
                                string dbName = reader[0] != DBNull.Value ? reader[0].ToString() : string.Empty;
                                fullName = !string.IsNullOrWhiteSpace(dbName) ? dbName : username.Trim();
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region User Registration
        public static bool RegisterUser(string fullName, string username, string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                errorMessage = "Full Name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Username is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Password is required.";
                return false;
            }

            username = username.Trim();
            fullName = fullName.Trim();

            using (OleDbConnection conn = GetConnection())
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = ?";
                using (OleDbCommand checkCmd = new OleDbCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.Add("?", OleDbType.VarWChar).Value = username;
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        errorMessage = "Username already exists. Please choose another username.";
                        return false;
                    }
                }

                HashSet<string> columns = GetUserTableColumns(conn);

                string pwdCol = "PasswordHarsh";
                if (columns.Contains("PasswordHarsh")) pwdCol = "PasswordHarsh";
                else if (columns.Contains("Password")) pwdCol = "[Password]";
                else if (columns.Contains("PasswordHash")) pwdCol = "PasswordHash";
                else if (columns.Contains("Pass")) pwdCol = "Pass";

                string nameCol = columns.Contains("FullName") ? "FullName" : (columns.Contains("Name") ? "Name" : null);
                string roleCol = columns.Contains("UserRole") ? "UserRole" : (columns.Contains("Role") ? "Role" : null);
                bool hasIsActive = columns.Contains("IsActive");
                bool hasCreatedAt = columns.Contains("CreatedAt");
                bool hasDateCreated = columns.Contains("DateCreated");

                List<string> colList = new List<string> { "Username", pwdCol };
                List<string> valPlaceholders = new List<string> { "?", "?" };
                List<OleDbParameter> paramList = new List<OleDbParameter>
                {
                    new OleDbParameter("?", OleDbType.VarWChar) { Value = username },
                    new OleDbParameter("?", OleDbType.VarWChar) { Value = password }
                };

                if (nameCol != null)
                {
                    colList.Add(nameCol);
                    valPlaceholders.Add("?");
                    paramList.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = fullName });
                }

                if (roleCol != null)
                {
                    colList.Add(roleCol);
                    valPlaceholders.Add("?");
                    paramList.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = "User" });
                }

                if (hasIsActive)
                {
                    colList.Add("IsActive");
                    valPlaceholders.Add("True");
                }

                if (hasCreatedAt)
                {
                    colList.Add("CreatedAt");
                    valPlaceholders.Add("NOW()");
                }
                else if (hasDateCreated)
                {
                    colList.Add("DateCreated");
                    valPlaceholders.Add("NOW()");
                }

                string insertSql = string.Format(
                    "INSERT INTO Users ({0}) VALUES ({1})",
                    string.Join(", ", colList),
                    string.Join(", ", valPlaceholders)
                );

                using (OleDbCommand insertCmd = new OleDbCommand(insertSql, conn))
                {
                    foreach (var param in paramList)
                    {
                        insertCmd.Parameters.Add(param);
                    }
                    insertCmd.ExecuteNonQuery();
                }

                return true;
            }
        }
        #endregion
    }
}
