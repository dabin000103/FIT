using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace FIT_API
{
    public static class Global
    {               

        //public static ErrorMessage ErrorMsg(string _msg)
        //{
        //    var _errorMessage = new ErrorMessage
        //    {
        //        Message = new string(_msg)
        //    };

        //    return _errorMessage;
        //}

        /// <summary>
        /// 데이터테이블로 변환
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static DataTable DictonarysToDataTable(List<Dictionary<string, object>> list)
        {
            DataTable table = new();

            foreach (Dictionary<string, object> dict in list)
            {
                foreach (KeyValuePair<string, object> entry in dict)
                {
                    if (!table.Columns.Contains(entry.Key.ToString()))
                    {
                        table.Columns.Add(entry.Key);
                    }
                }
                table.Rows.Add(dict.Values.ToArray());
            }

            return table;
        }

        //public static Image ByteArrayToImage(byte[] b)
        //{
        //    ImageConverter imgcvt = new ImageConverter();
        //    Image img = (Image)imgcvt.ConvertFrom(b);
        //    return img;
        //}

        //public static byte[] ImageToByteArray(Image img)
        //{
        //    ImageConverter imgcvt = new ImageConverter();
        //    byte[] b = (byte[])imgcvt.ConvertTo(img, typeof(byte[]));
        //    return b;
        //}

        //public static Stream ToStream(this Image image, ImageFormat format)
        //{
        //    var stream = new System.IO.MemoryStream();
        //    image.Save(stream, format);
        //    stream.Position = 0;
        //    return stream;
        //}

        /// <summary>
        /// 비밀번호 암호화
        /// </summary>
        /// <param name="_value"></param>
        /// <returns></returns>
        public static string SHA384암호화(string _value)
        {
            using (SHA384 sha384Hash = SHA384.Create())
            {
                //From String to byte array
                byte[] sourceBytes = Encoding.UTF8.GetBytes(_value);
                byte[] hashBytes = sha384Hash.ComputeHash(sourceBytes);

                // replacing - with empty
                string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);

                // final output
                // Console.WriteLine(string.Format("The SHA384 hash of {0} is: {1}", InputBytes, hash));
                return hash;
            }
        }

        // HMAC 생성 함수
        public static string GenerateHMAC(string key)
        {
            //key 암호
            //payload 'sha384'

            string payload = "sha384";

            // 키 생성
            var hmac_key = Encoding.UTF8.GetBytes(key);

            //// timestamp 생성
            //var timeStamp = DateTime.UtcNow;
            //var timeSpan = (timeStamp - new DateTime(1970, 1, 1, 0, 0, 0));
            //var hmac_timeStamp = (long)timeSpan.TotalMilliseconds;

            // HMAC-SHA384 객체 생성
            using (HMACSHA384 sha = new HMACSHA384(hmac_key))
            {
                // 본문 생성
                // 한글이 포함될 경우 글이 깨지는 경우가 생기기 때문에 payload를 base64로 변환 후 암호화를 진행한다.
                // 타임스탬프와 본문의 내용을 합하여 사용하는 경우가 일반적이다.
                // 타임스탬프 값을 이용해 호출, 응답 시간의 차이를 구해 invalid를 하거나 accepted를 하는 방식으로 사용가능하다.
                // 예시에서는 (본문 + 타임스탬프)이지만, 구글링을 통해 찾아보면 (본문 + "^" + 타임스탬프) 등의 방법을 취한다.
                //var bytes = Encoding.UTF8.GetBytes(payload + hmac_timeStamp);
                var bytes = Encoding.UTF8.GetBytes(payload);
                string base64 = Convert.ToBase64String(bytes);
                var message = Encoding.UTF8.GetBytes(base64);

                // 암호화
                var hash = sha.ComputeHash(message);

                // base64 컨버팅
                return Convert.ToBase64String(hash);
            }

        }

        public static byte[] FileToByteArray(string fileName)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(fileName,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(fileName).Length;
            buff = br.ReadBytes((int)numBytes);
            return buff;
        }

        #region Alter

        public static string Alter(object Expression, string AlterValue)
        {
            if (Expression == null)
            {
                return AlterValue;
            }
            else
            {
                return Expression.ToString();
            }
        }

        /// <summary>
        /// Alternative 
        /// </summary>
        /// <param name="Expression">string</param>
        /// <param name="AlterValue">string</param>
        /// <returns>string</returns>
        public static string Alter(string Expression, string AlterValue)
        {
            if (Expression == null)
            {
                Expression = string.Empty;
            }
            if (Expression.Length == 0)
            {
                return AlterValue;
            }
            else
            {
                return Expression;
            }
        }

        public static bool IsNumeric(object Expression)
        {
            bool isNum;
            double retNum;

            isNum = Double.TryParse(Convert.ToString(Expression), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        /// <summary>
        /// Alternative 
        /// </summary>
        /// <param name="Expression">string</param>
        /// <param name="AlterValue">int</param>
        /// <returns>int</returns>
        public static int Alter(string Expression, int AlterValue)
        {
            if (Expression.Length == 0)
            {
                return AlterValue;
            }
            else
            {
                if (IsNumeric(Expression))
                {
                    return int.Parse(Expression);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Alternative
        /// </summary>
        /// <param name="Expression">string</param>
        /// <param name="AlterValue">long</param>
        /// <returns>long</returns>
        public static long Alter(string Expression, long AlterValue)
        {
            if (Expression.Length == 0)
            {
                return AlterValue;
            }
            else
            {
                if (IsNumeric(Expression))
                {
                    return long.Parse(Expression);
                }
                else
                {
                    return 0;
                }
            }
        }

        public static object Alter(string Expression, object AlterValue)
        {
            if (Expression.Length == 0)
            {
                return AlterValue;
            }
            else
            {
                return Expression;
            }
        }

        #endregion
    }
}
