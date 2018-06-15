using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace CheckProcessAndRestart
{
    static class Utility
    {
        static string strSmtpServer = ConfigurationManager.AppSettings["SMTP_SERVER"].ToString();
        static string strSmtpServerPort = ConfigurationManager.AppSettings["SMTP_SERVER_PORT"].ToString();
        static string strSmtpServerAccount = ConfigurationManager.AppSettings["SMTP_ACCOUNT"].ToString();
        static string strSmtpServerPassword = ConfigurationManager.AppSettings["SMTP_PASSWORD"].ToString();
        static string strFrom = ConfigurationManager.AppSettings["SMTP_FROM"].ToString();
        static List<string> strTo = ConfigurationManager.AppSettings["SMTP_TO"].ToString().Split(";,".ToCharArray()).ToList();
        static string strEnableSsl = ConfigurationManager.AppSettings["SMTP_ENABLESSL"].ToString(); 

        /// <summary>
        /// 寄信的功能
        /// </summary>
        /// <param name="strSubject"></param>
        /// <param name="strBody"></param>
        /// <param name="blIsHtml"></param>
        /// <returns></returns>
        static public string SendMail(string strSubject, string strBody, bool blIsHtml)
        {
            string strMsg = "Ok";

            try
            {
                using (MailMessage mms = new MailMessage())
                {
                    mms.From = new MailAddress(strFrom);
                    mms.Subject = strSubject;
                    mms.Body = strBody;
                    mms.IsBodyHtml = blIsHtml;

                    for (int i = 0; i < strTo.Count; i++)
                        mms.To.Add(new MailAddress(strTo[i].Trim()));

                    using (SmtpClient client = new SmtpClient(strSmtpServer, int.Parse(strSmtpServerPort)))
                    {
                        client.Credentials = new NetworkCredential(strSmtpServerAccount, strSmtpServerPassword);
                        client.EnableSsl = bool.Parse(strEnableSsl);
                        client.Send(mms);
                    }
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }

            return strMsg;
        }
    }
}
