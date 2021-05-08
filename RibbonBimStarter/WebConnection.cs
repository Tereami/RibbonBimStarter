#region License
/*Данный код опубликован под лицензией Creative Commons Attribution-NonСommercial-ShareAlike.
Разрешено использовать, распространять, изменять и брать данный код за основу для производных 
в некоммерческих целях, при условии указания авторства и если производные лицензируются на тех же условиях.
Код поставляется "как есть". Автор не несет ответственности за возможные последствия использования.
Зуев Александр, 2021, все права защищены.
This code is listed under the Creative Commons Attribution-NonСommercial-ShareAlike license.
You may use, redistribute, remix, tweak, and build upon this work non-commercially,
as long as you credit the author by linking back and license your new creations under the same terms.
This code is provided 'as is'. Author disclaims any implied warranty.
Zuev Aleksandr, 2021, all rigths reserved.*/
#endregion
#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.IO;

#endregion

namespace RibbonBimStarter
{
    public class WebConnection
    {
        public int status;
        public string error;
        public string responseString;

        private byte[] credentialsByteArray;
        private string _website;


        public WebConnection(string email, string password, string website = "https://bim-starter.com/")
        {
            _website = website;
            string credentialsString = "email=" + email + "&password=" + password;
            credentialsByteArray = System.Text.Encoding.UTF8.GetBytes(credentialsString);
        }

        public void SendRequestAndReadString(string url)
        {
            status = 0;
            responseString = "";
            error = "";
            HttpWebRequest request = CreateRequest(url);
            if (request == null) return;

            Debug.WriteLine("Go to Webresponse");
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                error = ex.Message;
                response = (HttpWebResponse)ex.Response;
            }

            status = (int)response.StatusCode;
            if (status != 200)
            {
                error = "Error! Status: " + status;
                return;
            }
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseString = reader.ReadToEnd();
                }
            }
            response.Close();
            Debug.WriteLine("Webresponse success " + url + " status " + status);
        }

        public string DownloadFamily(string guid, string familyname)
        {
            HttpWebRequest request = CreateRequest("familydownloadapp/?guid=" + guid);
            if (request == null)
            {
                status = 400;
                error = "Request is null";
                return null;
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Debug.WriteLine("HttpWebResponse success");
                    WebHeaderCollection headers = response.Headers;

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        error = headers["Message"];
                        status = (int)response.StatusCode;
                        Debug.WriteLine(headers.ToString());
                        return null;
                    }

                    //string checkFilename0 = headers["Content-Disposition"].Split('=').Last();

                    string header = headers["Content-Disposition"];
                    string checkFilename0 = Encoding.UTF8.GetString(Encoding.GetEncoding("iso-8859-1").GetBytes(header));

                    string filename0 = checkFilename0.Split('=').Last();
                    filename0 = filename0.Replace("\\", "").Replace("\"", "");

                    string filename = familyname + ".rfa";

                    string pathToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bim-starter", filename);
                    Debug.WriteLine("File: " + pathToSave);
                    if (File.Exists(pathToSave))
                    {
                        Debug.WriteLine("File exists, try to delete");
                        System.IO.File.Delete(pathToSave);
                    }

                    Debug.WriteLine("Go to save file by FileStream");
                    byte[] buffer = new byte[1024];
                    int read;
                    using (FileStream download = new FileStream(pathToSave, FileMode.Create))
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                download.Write(buffer, 0, read);
                            }
                        }
                    }
                    long filesizeFromHeader = long.Parse(headers["Content-Length"]);
                    long downloadedSize = new FileInfo(pathToSave).Length;
                    if (filesizeFromHeader != downloadedSize)
                    {
                        Debug.WriteLine("Error download! " + filesizeFromHeader + " but " + downloadedSize);
                        error = "Incorrect file size!";
                        status = 416;
                    }
                    else
                    {
                        status = 200;
                        Debug.WriteLine("File download successfully");
                    }
                    return pathToSave;
                }
            }
            catch (Exception ex)
            {
                error = "Error: " + ex.Message;
                Debug.WriteLine("Error http response! " + ex.Message);
                return null;
            }
        }


        private HttpWebRequest CreateRequest(string url)
        {
            Debug.WriteLine("webrequest starting");
            string fullurl = _website + url;
            HttpWebRequest request = WebRequest.Create(fullurl) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = credentialsByteArray.Length;
            request.Timeout = 60000;

            //if (s.Proxy != ProxyType.No)
            //{
            //    request.Proxy = GetProxy(s);
            //}
            try
            {
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(credentialsByteArray, 0, credentialsByteArray.Length);
                    Debug.WriteLine("DataStream write successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error dataStream write" + ex.Message);
                error = ex.Message;
                return null;
            }
            return request;
        }
    }
}
