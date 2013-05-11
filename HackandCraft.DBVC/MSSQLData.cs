
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace DBVC
{
    public class MSSQLData : IDataSource
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

        public MSSQLData(string connStr)
        {
            this.connStr = connStr;
        }

        public MSSQLData()
        {
        }

        private SqlConnection getPooledConnection(SqlConnection sConn)
        {
            sConn.ConnectionString = this.connStr;
            sConn.Open();
            sConn.Close();
            return sConn;
        }

        private bool closeConnection(SqlConnection sConn)
        {
            sConn.Close();
            sConn.Dispose();
            return true;
        }

        public XmlDocument execStoredProc(string strProcName, XmlDocument strParameters)
        {
            var xmlDocument = new XmlDocument();
            var pooledConnection = this.getPooledConnection(new SqlConnection());
            try
            {
                pooledConnection.Open();
                var sqlCommand = new SqlCommand(strProcName, pooledConnection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = 180,
                    };
                sqlCommand.Parameters.Add("@p", SqlDbType.Xml).Value = strParameters == null ? null : strParameters.InnerXml;
                SqlParameter sqlParameter = sqlCommand.Parameters.Add("@r", SqlDbType.Xml);
                sqlParameter.Direction = ParameterDirection.Output;
                sqlCommand.ExecuteNonQuery();
                xmlDocument.LoadXml(sqlParameter.Value.ToString());
                pooledConnection.Close();
            }
            catch (Exception ex)
            {
                xmlDocument = errorXmlResult(ex.Message);
                pooledConnection.Close();
            }
            return xmlDocument;
        }

        public string execStoredProc(string strProcName, int jsonObjectId, string strParameters)
        {
            SqlConnection pooledConnection = this.getPooledConnection(new SqlConnection());
            try
            {
                pooledConnection.Open();
                var sqlCommand = new SqlCommand(strProcName, pooledConnection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = 180,
                    };
                sqlCommand.Parameters.Add("@p", SqlDbType.VarChar).Value = strParameters;
                sqlCommand.Parameters.Add("@o", SqlDbType.Int).Value = jsonObjectId;
                SqlParameter sqlParameter = sqlCommand.Parameters.Add("@r", SqlDbType.VarChar, -1);
                sqlParameter.Direction = ParameterDirection.Output;
                sqlCommand.ExecuteNonQuery();
                pooledConnection.Close();
                return sqlParameter.Value.ToString();
            }
            catch (Exception ex)
            {
                pooledConnection.Close();
                return ex.ToString();
            }
        }

        private XmlDocument errorXmlResult(string exp)
        {
            var xmlDocument = new XmlDocument();
            var xmlElement = (XmlElement)xmlDocument.AppendChild((XmlNode)xmlDocument.CreateElement("Result"));
            xmlElement.SetAttribute("errorMessage", exp);
            xmlElement.SetAttribute("status", "1");
            return xmlDocument;
        }
    }
}
