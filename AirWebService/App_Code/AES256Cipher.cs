using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AirWebService
{
    public class AES256Cipher
    {
        const string MODETOUR = "MODETOUR";
        const string TMon = "TMon";
        const string SKPlanet = "SKPlanet";
        const string Ebay = "Ebay";
        const string TravelHow = "TravelHow";
        
        /// <summary>
        /// 사이트별 암호화 KEY Name
        /// </summary>
        /// <param name="SNM">사이트번호</param>
        /// <returns></returns>
        public static string KeyName(int SNM)
        {
            //티몬
            if (SNM.Equals(4925) || SNM.Equals(4926))
                return TMon;
            
            //11번가
            else if (SNM.Equals(4924) || SNM.Equals(4929))
                return SKPlanet;

            //지마켓(5020,5119),옥션(5161,5163),G9(5162,5164)
            else if (SNM.Equals(5020) || SNM.Equals(5119) || SNM.Equals(5161) || SNM.Equals(5163) || SNM.Equals(5162) || SNM.Equals(5164))
                return Ebay;

            //트래블하우
            else if (SNM.Equals(4716))
                return TravelHow;

            //모두닷컴(2,3915) 및 기타
            else
                return MODETOUR;
        }

        //거래처에 따른 AES KEY
        public static byte[] SelectAesKey(string KeyName)
        {
            string StrKey = string.Empty;
            
            //트래블하우
            if (KeyName.Equals(TravelHow))
                StrKey = "YeSGFazFC4gcbaty";

            //티켓몬스터
            else if (KeyName.Equals(TMon))
                StrKey = "Xlzp0TA6hst9M1xj";

            //11번가
            else if (KeyName.Equals(SKPlanet))
                StrKey = "S1KvmFOflsT1rHkD";
            
                //이베이코리아
            else if (KeyName.Equals(Ebay))
                StrKey = "DLQPDL@GKDRHDRNJS!#tjdrhdgkwk&gg";
            
            //모두닷컴 및 기타
            else
                StrKey = "TkFk0dgK6qsL91Ek";

            return Encoding.UTF8.GetBytes(StrKey);
        }

        //거래처에 따른 IV KEY
        public static byte[] SelectIvKey(string KeyName)
        {
            string StrKey = string.Empty;

            if (KeyName.Equals(Ebay))
                StrKey = "gkdrhd!#dlqpdl@!";

            return String.IsNullOrWhiteSpace(StrKey) ? new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } : Encoding.UTF8.GetBytes(StrKey);
        }

        /// <summary>
        /// 암호화
        /// </summary>
        /// <param name="KeyName">거래처명</param>
        /// <param name="InputData"></param>
        /// <returns></returns>
        public string AESEncrypt(string KeyName, string InputData)
        {
            if (String.IsNullOrWhiteSpace(InputData))
                return InputData;
            else
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = SelectAesKey(KeyName);
                aes.IV = SelectIvKey(KeyName);

                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Encoding.UTF8.GetBytes(InputData);
                        cs.Write(xXml, 0, xXml.Length);
                    }

                    xBuff = ms.ToArray();
                }

                return Convert.ToBase64String(xBuff);
            }
        }

        /// <summary>
        /// 복호화
        /// </summary>
        /// <param name="KeyName">거래처명</param>
        /// <param name="InputData"></param>
        /// <returns></returns>
        public string AESDecrypt(string KeyName, string InputData)
        {
            if (String.IsNullOrWhiteSpace(InputData))
                return InputData;
            else
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = SelectAesKey(KeyName);
                aes.IV = SelectIvKey(KeyName);

                var decrypt = aes.CreateDecryptor();
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Convert.FromBase64String(InputData);
                        cs.Write(xXml, 0, xXml.Length);
                    }

                    xBuff = ms.ToArray();
                }

                return Encoding.UTF8.GetString(xBuff);
            }
        }
    }
}