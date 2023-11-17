using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace myRaiHelper.Task
{
    public class QueryCommand : BaseTask
    {
        public enum DatabaseEnum
        {
            None = 0,
            Cezanne = 1,
            Talentia = 2,
            Custom = 99
        }

        public enum OperationType
        {
            None = 0,
            NonQuery = 1,
            Dataset = 2
        }

        public string Command { get; set; }
        public string ConnectionString { get; set; }
        public DatabaseEnum Database { get; set; }
        public OperationType Operation { get; set; }

        public override bool CheckParam(out string errore)
        {
            bool result = true;
            errore = null;

            if (Database == DatabaseEnum.None)
            {
                result = false;
                errore += "Modello non indicato\r\n";
            }
            else if (Database == DatabaseEnum.Custom && String.IsNullOrWhiteSpace(ConnectionString))
            {
                result = false;
                errore += "String di connession non indicata";
            }

            if (Operation == OperationType.None)
            {
                result = false;
                errore += "Operazione non indicata";
            }

            if (String.IsNullOrWhiteSpace(Command))
            {
                result = false;
                errore += "Comando non indicato";
            }

            return result;
        }

        public override bool Esegui(out string output, out string errore)
        {
            bool result = false;
            output = null;
            errore = null;

            DbContext context = null;
            string connString = "";
            switch (Database)
            {
                case DatabaseEnum.None:
                    break;
                case DatabaseEnum.Cezanne:
                    context = new myRaiData.Incentivi.IncentiviEntities();
                    connString = context.Database.Connection.ConnectionString;
                    break;
                case DatabaseEnum.Talentia:
                    context = new myRaiDataTalentia.TalentiaEntities();
                    connString = context.Database.Connection.ConnectionString;
                    break;
                case DatabaseEnum.Custom:
                    connString = ConnectionString;
                    break;
                default:

                    break;
            }

            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                using (SqlConnection sqlConnection = new SqlConnection(connString))
                {
                    switch (Operation)
                    {
                        case OperationType.None:
                            break;
                        case OperationType.NonQuery:
                            sqlConnection.Open();
                            sqlCommand.Connection = sqlConnection;
                            sqlCommand.CommandText = Command;
                            int rows = sqlCommand.ExecuteNonQuery();
                            if (sqlConnection.State == ConnectionState.Open)
                                sqlConnection.Close();

                            output = String.Format("{0} rows affected", rows);
                            result = true;
                            break;
                        case OperationType.Dataset:
                            DataSet ds = new DataSet();
                            sqlCommand.Connection = sqlConnection;
                            sqlCommand.CommandType = CommandType.Text;
                            sqlCommand.CommandText = Command;
                            da.Fill(ds);

                            output = ds.GetXml();
                            result = true;

                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                errore = ex.Message;
            }

            return result;
        }
    }
}
