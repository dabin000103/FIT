using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AirWebService
{
    public delegate Int64 dgDevDBSave(int SNM, string GUID, string DEV, string ReqInfo, Int64 S3Idx, string ResXml);

    public class SearchSave
    {
        public Int64 GetSearchIdx()
        {
            SqlConnection SqlCon = null;

            try
            {
                SqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DEV"].ConnectionString);

                using (SqlCommand sqlCommand = new SqlCommand("WSVLOG.DBO.WSV_S_해외항공검색3_항공검색번호", SqlCon) { CommandTimeout = 3, CommandType = CommandType.StoredProcedure })
                {
                    sqlCommand.Parameters.Add("@항공검색번호", SqlDbType.Int, 0);
                    sqlCommand.Parameters.Add("@결과", SqlDbType.Char, 1);
                    sqlCommand.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                    sqlCommand.Parameters["@항공검색번호"].Direction = ParameterDirection.Output;
                    sqlCommand.Parameters["@결과"].Direction = ParameterDirection.Output;
                    sqlCommand.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                    if (SqlCon.State.Equals(ConnectionState.Closed))
                        SqlCon.Open();

                    sqlCommand.ExecuteNonQuery();

                    if (sqlCommand.Parameters["@결과"].Value.ToString().Equals("F"))
                        throw new Exception(sqlCommand.Parameters["@에러메시지"].Value.ToString());

                    return Convert.ToInt64(sqlCommand.Parameters["@항공검색번호"].Value);
                }
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                if (SqlCon.State.Equals(ConnectionState.Open) || SqlCon.State != ConnectionState.Closed)
                    SqlCon.Close();

                SqlCon.Dispose();
            }
        }

        public Int64 DevDBSave(int SNM, string GUID, string DEV, string ReqInfo, Int64 S3Idx, string ResXml)
        {
            SqlConnection SqlCon = null;

            try
            {
                SqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DEV"].ConnectionString);

                using (SqlCommand sqlCommand = new SqlCommand("WSVLOG.DBO.WSV_T_해외항공검색3_저장_00", SqlCon) { CommandTimeout = 10, CommandType = CommandType.StoredProcedure })
                {
                    sqlCommand.Parameters.Add("@항공검색번호", SqlDbType.BigInt, 0);
                    sqlCommand.Parameters.Add("@사이트번호", SqlDbType.Int, 0);
                    sqlCommand.Parameters.Add("@요청정보", SqlDbType.VarChar, 200);
                    sqlCommand.Parameters.Add("@XMLDATA", SqlDbType.Xml, -1);
                    sqlCommand.Parameters.Add("@GUID", SqlDbType.VarChar, 30);
                    sqlCommand.Parameters.Add("@개발용도", SqlDbType.Char, 1);
                    sqlCommand.Parameters.Add("@결과", SqlDbType.Char, 1);
                    sqlCommand.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                    sqlCommand.Parameters["@항공검색번호"].Value = S3Idx;
                    sqlCommand.Parameters["@사이트번호"].Value = SNM;
                    sqlCommand.Parameters["@요청정보"].Value = ReqInfo;
                    sqlCommand.Parameters["@XMLDATA"].Value = ResXml;
                    sqlCommand.Parameters["@GUID"].Value = GUID;
                    sqlCommand.Parameters["@개발용도"].Value = DEV;
                    sqlCommand.Parameters["@결과"].Direction = ParameterDirection.Output;
                    sqlCommand.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                    if (SqlCon.State.Equals(ConnectionState.Closed))
                        SqlCon.Open();

                    sqlCommand.ExecuteNonQuery();

                    if (sqlCommand.Parameters["@결과"].Value.ToString().Equals("F"))
                        throw new Exception(sqlCommand.Parameters["@에러메시지"].Value.ToString());

                    return Convert.ToInt64(sqlCommand.Parameters["@항공검색번호"].Value);
                }
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                if (SqlCon.State.Equals(ConnectionState.Open) || SqlCon.State != ConnectionState.Closed)
                    SqlCon.Close();

                SqlCon.Dispose();
            }
        }
    }
}