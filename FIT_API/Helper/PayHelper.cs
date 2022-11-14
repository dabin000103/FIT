using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FIT_API.Helper
{
    public class PayHelper
    {
        /// <summary>
        /// SettleBank PayHelper
        /// </summary>
        public class SettleBank
        {
            /* 파라미터 AES256 암호화 */
            public static string Encrypt(String key, String val)
            {
                RijndaelManaged aes = new RijndaelManaged
                {
                    KeySize = 256,
                    BlockSize = 128,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7,
                    Key = System.Text.Encoding.UTF8.GetBytes(key),
                    IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
                };

                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);

                byte[] xBuff = null;
                using (var ms = new System.IO.MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Encoding.UTF8.GetBytes(val);
                        cs.Write(xXml, 0, xXml.Length);
                    }

                    xBuff = ms.ToArray();
                }

                String Output = Convert.ToBase64String(xBuff);

                return Output;
            }

            /* 파라미터 AES256복호화 */
            public static string Decrypt(String key, String val)
            {
                RijndaelManaged aes = new RijndaelManaged
                {
                    KeySize = 256,
                    BlockSize = 128,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7,
                    Key = Encoding.UTF8.GetBytes(key),
                    IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
                };

                var decrypt = aes.CreateDecryptor();
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Convert.FromBase64String(val);
                        cs.Write(xXml, 0, xXml.Length);
                    }

                    xBuff = ms.ToArray();
                }

                String Output = Encoding.UTF8.GetString(xBuff);
                return Output;
            }

            /* Sha256 암호화 */
            public static string Sha256(string payload)
            {
                SHA256 sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in hash)
                {
                    stringBuilder.AppendFormat("{0:x2}", b);
                }
                return stringBuilder.ToString();
            }

            /* API 호출 메소드 */
            public static dynamic SendApi(string target_url, string postData, int timeout, string method, string contenttype)
            {
                //LogMessage(LOG_FILE, "[SendAPI Start] URL(" + target_url + "), Timeout(" + timeout + ")");
                //LogMessage(LOG_FILE, "[SendAPI Request] Parameters : " + HttpUtility.UrlDecode(postData));

                method = string.IsNullOrEmpty(method) ? "POST" : method;

                target_url = method.Equals("GET") ? target_url + '?' + JsonConvert.DeserializeObject(postData) : target_url;

                JObject responseFromServer = new JObject();
                String responseStr = "";

                try
                {
                    WebRequest webRequest = WebRequest.Create(target_url);
                    webRequest.Method = method; // POST로 설정
                    webRequest.Timeout = timeout;                                  // Timeout 설정

                    if (webRequest.Method.Equals("POST"))
                    {
                        byte[] byteArray = Encoding.UTF8.GetBytes(postData);           // byte[]로 변환
                        webRequest.ContentType = string.IsNullOrEmpty(contenttype) ? "Application/json" : contenttype; // ContentType 설정
                        webRequest.ContentLength = byteArray.Length;                   // Content 길이 설정

                        // Get the request stream. request 스트림을 얻는다.
                        Stream dataStream = webRequest.GetRequestStream();

                        // request스트림에 데이터 출력
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        // request스트림 닫기
                        dataStream.Close();
                    }

                    // response객체를 얻는다.
                    WebResponse webResponse = webRequest.GetResponse();

                    //LogMessage(LOG_FILE, "[SendAPI Result] HTTP POST request for URL(" + target_url + ") resulted in HTTP status code " + (int)((HttpWebResponse)webResponse).StatusCode + "(" + ((HttpWebResponse)webResponse).StatusDescription + ")");

                    // 서버로부터 리턴된 내용을 스트림에서 읽어온다.
                    using (Stream dataStream = webResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        responseStr = reader.ReadToEnd();
                        responseFromServer = JObject.Parse(responseStr);
                    }

                    webResponse.Close();

                    //LogMessage(LOG_FILE, "[SendAPI End] Response : " + responseFromServer.ToString() + "\n");
                }
                catch (WebException ex)
                {
                    Dictionary<String, String> head = new Dictionary<String, String>();
                    Dictionary<String, String> body = new Dictionary<String, String>();
                    Dictionary<String, Object> resData = new Dictionary<String, Object>();

                    if (ex.Response is HttpWebResponse)
                    {
                        head = new Dictionary<String, String>
                        {
                            { "outStatCd", ((int)((HttpWebResponse)ex.Response).StatusCode).ToString() },
                            { "outRsltCd", ((int)((HttpWebResponse)ex.Response).StatusCode).ToString() },
                            { "outRsltMsg", "[SendAPI Exception] "+ex.Message }
                        };

                        resData = new Dictionary<String, Object>
                        {
                            { "params", head },
                            { "data", body }
                        };
                    }
                    else
                    {
                        head = new Dictionary<String, String>
                        {
                            { "outStatCd", "0099" },
                            { "outRsltCd", "0099" },
                            { "outRsltMsg", "[SendAPI Exception] "+ex.Message }
                        };

                        resData = new Dictionary<String, Object>
                        {
                            { "params", head },
                            { "data", body }
                        };
                    }

                    responseFromServer = JObject.FromObject(resData);
                }
                return responseFromServer;
            }
        }
    }
}
