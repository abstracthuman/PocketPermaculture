using System;
using System.IO;
using System.Net;

namespace WebApiClient
{
    public class MakeWebCall
    {   
        public object GetRequest(string uri, string query)
        {
            string responseObject = "";

            try
            {
                Uri myUri = new Uri(query != "" ? uri + query : uri);
                WebRequest request = (HttpWebRequest)WebRequest.Create(myUri);

                request.ContentType = "application/json";
                request.Method = "GET";

                WebResponse response = request.GetResponse();
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    responseObject = streamReader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.GetBaseException().Message);
            }

            return responseObject;
        }
    }
}
