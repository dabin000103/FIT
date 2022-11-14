using System;
using System.Data;
using System.Text;

namespace AirWebService
{
	/// <summary>
	/// Json 변환 함수
	/// </summary>
	public class JsonConverter
	{
		/// <summary>
		/// DataSet을 JSON으로 출력
		/// </summary>
		/// <param name="ds">DataSet</param>
		/// <param name="TableNameView">테이블의 이름 출력 여부</param>
		/// <returns></returns>
		public static string ConvertToJson(DataSet ds, bool TableNameView = false)
		{
			if (ds.Tables.Count.Equals(1))
			{
				if (TableNameView)
					return String.Format("{{\"{0}\":{1}}}", ds.Tables[0].TableName, ConvertToJson(ds.Tables[0], TableNameView));
				else
					return ConvertToJson(ds.Tables[0]);
			}
			else
			{
				StringBuilder sb1 = new StringBuilder(1024);

				sb1.Append("{");

				for (int i = 0; i < ds.Tables.Count; i++)
				{
					if (i > 0)
						sb1.Append(",");

					sb1.Append(String.Format("\"{0}\":{1}", ds.Tables[i].TableName, ConvertToJson(ds.Tables[i], TableNameView)));
				}

				sb1.Append("}");

				return sb1.ToString();
			}
		}

		/// <summary>
		/// DataTable을 JSON으로 출력
		/// </summary>
		/// <param name="dt">DataTable</param>
		/// <param name="TableNameView">테이블의 이름 출력 여부</param>
		/// <returns></returns>
		public static string ConvertToJson(DataTable dt, bool TableNameView = false)
		{
			string[] StrDc = new string[dt.Columns.Count];
			string HeadStr = string.Empty;
			bool isArray = (dt.Rows.Count > 1) ? true : false;

			for (int i = 0; i < dt.Columns.Count; i++)
			{
				StrDc[i] = dt.Columns[i].Caption;
				HeadStr += String.Format("\"{0}\":\"{0}{1}¾\",", StrDc[i], i);
			}

			HeadStr = HeadStr.Substring(0, HeadStr.Length - 1);

			StringBuilder sb = new StringBuilder(512);

			for (int i = 0; i < dt.Rows.Count; i++)
			{
				string TempStr = HeadStr;

				sb.Append("{");

				for (int j = 0; j < dt.Columns.Count; j++)
					TempStr = TempStr.Replace(String.Concat(dt.Columns[j], j.ToString(), "¾"), ConvertToString(dt.Rows[i][j].ToString()));

				sb.Append(TempStr + "},");
			}

			if (sb.ToString().Length > 0)
			{
				sb = new StringBuilder(sb.ToString().Substring(0, sb.ToString().Length - 1));

				return (isArray || TableNameView) ? String.Concat("[", sb.ToString(), "]") : sb.ToString();
			}
			else
				return "\"\"";
		}

		/// <summary>
		/// JSON에서 인식되지 않는 문자 치환
		/// </summary>
		/// <param name="Text"></param>
		/// <returns></returns>
		public static string ConvertToString(string Text)
		{
			return Text.Replace("\"", "'").Replace(Environment.NewLine, "\\n").Replace("\\", "\\\\");
		}
	}
}