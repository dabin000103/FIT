using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AirWebService
{
    public class LogSave
    {
        public Int64 LogDBSave(SqlParameter[] Params)
        {
            SqlConnection SqlCon = null;

			try
			{
                //            SqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVNSYNC"].ConnectionString);

                //            using (SqlCommand sqlCommand = new SqlCommand("DBO.WSV_T_웹서비스_로그_RQ_저장", SqlCon) { CommandTimeout = 5, CommandType = CommandType.StoredProcedure })
                //{
                //	if (Params != null)
                //	{
                //		foreach (SqlParameter param in Params)
                //			sqlCommand.Parameters.Add(param);
                //                }

                //                sqlCommand.Parameters.Add("@일련번호", SqlDbType.BigInt, 0);
                //                sqlCommand.Parameters.Add("@결과", SqlDbType.Char, 1);
                //                sqlCommand.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                //                sqlCommand.Parameters["@일련번호"].Direction = ParameterDirection.Output;
                //                sqlCommand.Parameters["@결과"].Direction = ParameterDirection.Output;
                //                sqlCommand.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                //	if (SqlCon.State.Equals(ConnectionState.Closed))
                //	    SqlCon.Open();

                //	sqlCommand.ExecuteNonQuery();

                //                if (sqlCommand.Parameters["@결과"].Value.ToString().Equals("F"))
                //                    throw new Exception(sqlCommand.Parameters["@에러메시지"].Value.ToString());

                //                return Convert.ToInt64(sqlCommand.Parameters["@일련번호"].Value);
                //}
                return 0;
			}
			catch (Exception)
			{
                //throw new Exception(ex.Message);
                return 0;
			}
			finally
			{
				//if (SqlCon.State.Equals(ConnectionState.Open) || SqlCon.State != ConnectionState.Closed)
				//	SqlCon.Close();

				//SqlCon.Dispose();
			}
        }

        public void RSLogDBSave(Int64 LogSequence, int ServiceNumber, string ResData)
        {
            SqlConnection SqlCon = null;

            try
            {
                //SqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["WSVNSYNC"].ConnectionString);

                //using (SqlCommand sqlCommand = new SqlCommand("DBO.WSV_T_웹서비스_로그_RS_저장", SqlCon) { CommandTimeout = 10, CommandType = CommandType.StoredProcedure })
                //{
                //    sqlCommand.Parameters.Add("@요청번호", SqlDbType.BigInt, 0);
                //    sqlCommand.Parameters.Add("@서비스번호", SqlDbType.Int, 0);
                //    sqlCommand.Parameters.Add("@응답데이타", SqlDbType.VarChar, -1);
                //    sqlCommand.Parameters.Add("@결과", SqlDbType.Char, 1);
                //    sqlCommand.Parameters.Add("@에러메시지", SqlDbType.NVarChar, 1000);

                //    sqlCommand.Parameters["@요청번호"].Value = LogSequence;
                //    sqlCommand.Parameters["@서비스번호"].Value = ServiceNumber;
                //    sqlCommand.Parameters["@응답데이타"].Value = ResData;
                //    sqlCommand.Parameters["@결과"].Direction = ParameterDirection.Output;
                //    sqlCommand.Parameters["@에러메시지"].Direction = ParameterDirection.Output;

                //    if (SqlCon.State.Equals(ConnectionState.Closed))
                //        SqlCon.Open();

                //    sqlCommand.ExecuteNonQuery();
                //}
            }
            catch (Exception) { }
            finally
            {
                if (SqlCon.State.Equals(ConnectionState.Open) || SqlCon.State != ConnectionState.Closed)
                    SqlCon.Close();

                SqlCon.Dispose();
            }
        }
    }
}