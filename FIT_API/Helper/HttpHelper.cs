using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FIT_API.Helper
{
    public class TokenHelper
    {
        //public static Customer GetCustomer(IIdentity identity)
        //{            

        //    if (identity == null)
        //    {
        //        Customer customer = new Customer() { PartyID = string.Empty, MemberID = string.Empty, Name = string.Empty, Status = string.Empty, SupplyID = string.Empty };
        //        return customer;
        //    }

        //    var Identity = identity as ClaimsIdentity;
        //    if (Identity.Claims.Count() == 8)
        //    {
        //        IList<Claim> claim = Identity.Claims.ToList();
        //        Customer customer = new Customer() { PartyID = claim[0].Value, MemberID = claim[1].Value, Name = claim[2].Value, Status = claim[3].Value, SupplyID = claim[4].Value };
        //        return customer;
        //    }
        //    else
        //    {
        //        Customer customer = new Customer() { PartyID = string.Empty, MemberID = string.Empty, Name = string.Empty, Status = string.Empty, SupplyID = string.Empty };
        //        return customer;
        //    }

           
        //}
    }

    public class MailManager
    {
        private static readonly string _admin_email_name = "트립박스";
        private static readonly string _admin_email = "support@tripbox.kr";
        private static readonly string _admin_email_password = "trb082603*";
        private static readonly string _email_host = "wsmtp.ecounterp.com";
        private static readonly int _email_port = 587;

        public static void SendMail(string mailto, string subject, string body)
        {
            MailMessage message = new MailMessage();
            //message.To.Add("instead@mytripbox.co.kr");
            message.To.Add(mailto);
            //message.To.Add("hackerjy@naver.com");
            message.From = new MailAddress(_admin_email, "트립박스", System.Text.Encoding.UTF8);


            //string _내용 = "비밀번호 : 1234567890";
            //+ "<a href=" + "https://tripboxapi.azurewebsites.net/Users/emailAuthentication?email_auth_key=" + "dddd" + ">메일 인증하기</a>";


            message.Subject = subject;
            message.SubjectEncoding = UTF8Encoding.UTF8;
            message.Body = body;
            message.BodyEncoding = UTF8Encoding.UTF8;
            message.IsBodyHtml = true; //메일내용이 HTML형식임
            message.Priority = MailPriority.High; //중요도 높음
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure; //메일 배달 실패시 알림

            SmtpClient client = new SmtpClient();
            client.Host = _email_host; //SMTP(발송)서버 도메인
            client.Port = _email_port; //25, SMTP서버 포트
            client.EnableSsl = false; //SSL 사용
            client.Timeout = 10000;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(_admin_email, _admin_email_password);//보내는 사람 메일 서버접속계정, 암호, Anonymous이용시 생략
            client.Send(message);

            message.Dispose();
        }

        
    }

    public class FTPManager
    {
        private const string FTPAddress = "ftp://tripbox.speedgabia.com/";
        private const string Username = "tripbox";
        private const string Password = "jjong0105!";

        public byte[] DownloadFile(string relativePath)
        {
            var fileData = new byte[0];

            try
            {
                // parse the path
                relativePath = ParseRelativePath(relativePath);

                using (var request = new WebClient())
                {
                    request.Credentials = new NetworkCredential(Username, Password);
                    fileData = request.DownloadData(FTPAddress + relativePath);
                }

            }
            catch { }

            return fileData;
        }

        public bool DoesFileExist(string relativePath)
        {
            var fileExist = false;

            var request = WebRequest.Create(FTPAddress + ParseRelativePath(relativePath));
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(Username, Password);

            try
            {
                using (var response = request.GetResponse())
                {
                    fileExist = true;
                }
            }
            catch { }

            return fileExist;
        }

        public string GetUniqueFileNameWithPath(string dirPath, string fileN)
        {
            string fileName = fileN;

            int indexOfDot = fileName.LastIndexOf(".");
            string strName = fileName.Substring(0, indexOfDot);
            string strExt = fileName.Substring(indexOfDot + 1);

            bool bExist = true;
            int fileCount = 0;

            while (bExist)
            {
                if (DoesFileExist(dirPath + fileName) == true)
                {
                    fileCount++;
                    fileName = strName + "(" + fileCount + ")" + "." + strExt;
                }
                else
                {
                    bExist = false;
                }
            }

            // return Path.Combine(dirPath, fileName);
            return fileName;

        }

        // upload file and handle everything
        public bool UploadFile(string relativePath, byte[] fileBinary)
        {
            bool _return = true;
            try
            {
                // parse the path
                relativePath = ParseRelativePath(relativePath);

                // create the folder before writing the file
                CreateFolders(relativePath);

                // delete the file
                DeleteFile(relativePath);

                // upload the file
                SendFile(relativePath, fileBinary);
                _return = true;
            }
            catch
            {
                _return = false;
            }

            return _return;
        }

        private void SendFile(string relativePath, byte[] fileBinary)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(FTPAddress + relativePath);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(Username, Password);
                var stream = request.GetRequestStream();
                var sourceStream = new MemoryStream(fileBinary);
                var length = 1024;
                var buffer = new byte[length];
                var bytesRead = 0;

                do
                {
                    bytesRead = sourceStream.Read(buffer, 0, length);
                    stream.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);

                sourceStream.Close();
                stream.Close();
            }
            catch
            {

            }
        }

        public void DeleteFile(string relativePath)
        {
            try
            {
                WebRequest ftpRequest = WebRequest.Create(FTPAddress + relativePath);
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpRequest.Credentials = new NetworkCredential(Username, Password);
                ftpRequest.GetResponse();
            }
            catch { }
        }

        // parse the relative path for strange slashes
        private string ParseRelativePath(string relativePath)
        {
            // split to remove all slashes and duplicates
            var split = relativePath.Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);

            // join the string back with valid slash
            var result = String.Join("/", split);

            return result;
        }

        // Check if the relative path need folders to be created
        private void CreateFolders(string relativePath)
        {
            if (relativePath.IndexOf("/") > 0)
            {
                var folders = relativePath.Split(new[] { "/" }, StringSplitOptions.None).ToList();

                var folderToCreate = FTPAddress.Substring(0, FTPAddress.Length - 1);

                for (int i = 0; i < folders.Count - 1; i++)
                {
                    folderToCreate += ("/" + folders[i]);

                    CreateFolder(folderToCreate);
                }
            }
        }

        // create a single folder on the FTP
        private void CreateFolder(string folderPath)
        {
            try
            {
                WebRequest ftpRequest = WebRequest.Create(folderPath);
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                ftpRequest.Credentials = new NetworkCredential(Username, Password);
                ftpRequest.GetResponse();
            }
            catch { } // create folder will fail if it already exist
        }
    }

}
