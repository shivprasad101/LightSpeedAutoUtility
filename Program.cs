using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Microsoft.Exchange.WebServices.Data;
using System.Net;

namespace LightSpeedAutoUtility
{
    class Program
    {
     static Ado dataLayer = new Ado();
       public static List<int> branchList = new List<int>();
       public static Dictionary<int, string> BIdsNames = new Dictionary<int, string>();
        public static string MailSubject = "";
        public static string mailBody = "";

        static void Main(string[] args)
        {
            try
            {
                string startDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                CheckFolderExistOrNot();
                PrepareBranchIds();
                ZeroPickedByDateRange(startDate);
                ProductByDateRange(startDate);
                SendAlertSuccess();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Program.ErrorLog(ex);
                MailSubject = "Error-OOS LightSpeed download and upload to DW";
                mailBody = "Error occured while downloading or uploading LightSpeed data to DW -> " + ex.Message;
                Program.SendAlertError();
            }
        }

       
        public static void SendAlertError()
        {
            //var zipPath = @"D:\AutomationForUltipro\UltiproXlsxFiles";
            ExchangeService service = new ExchangeService();
            string from = ConfigurationSettings.AppSettings["FromAddress"];
            string frompass = ConfigurationSettings.AppSettings["FromPassword"];
            string to = ConfigurationSettings.AppSettings["ToAddress"];
            //var files = new DirectoryInfo(zipPath).GetFiles().First();
            //var attachToMail = files;
            service.Credentials = new NetworkCredential(from, frompass);
            service.Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");
            EmailMessage message = new Microsoft.Exchange.WebServices.Data.EmailMessage(service);
            message.Subject = MailSubject;
            message.Body = mailBody;
            //var path = System.IO.Path.Combine(attachToMail.Directory.ToString(), attachToMail.Name);
            //message.Attachments.AddFileAttachment(path);
            message.ToRecipients.Add(to);
            message.Send();
            MailSubject = "";
            mailBody = "";
        }

        public static void SendAlertSuccess()
        {
            //var zipPath = @"D:\AutomationForUltipro\UltiproXlsxFiles";
            ExchangeService service = new ExchangeService();
            string from = ConfigurationSettings.AppSettings["FromAddress"];
            string frompass = ConfigurationSettings.AppSettings["FromPassword"];
            string to = ConfigurationSettings.AppSettings["ToAddress"];
            //var files = new DirectoryInfo(zipPath).GetFiles().First();
            //var attachToMail = files;
            service.Credentials = new NetworkCredential(from, frompass);
            service.Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");
            EmailMessage message = new Microsoft.Exchange.WebServices.Data.EmailMessage(service);
            message.Subject = "Success - OOS LightSpeed download and upload to DW";
            message.Body = "All OOS LightSpeed data successfully downloaded and uploaded to datawarehouse on:" + DateTime.Now;
            //var path = System.IO.Path.Combine(attachToMail.Directory.ToString(), attachToMail.Name);
            //message.Attachments.AddFileAttachment(path);
            message.ToRecipients.Add(to);
            message.Send();
            MailSubject = "";
            mailBody = "";
        }
        public static void ErrorLog(Exception ex)
        {
            string filePath = ConfigurationSettings.AppSettings["ErrorFile"];

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }
        public static void CheckFolderExistOrNot()
        {
            string ErrorFolder = ConfigurationSettings.AppSettings["ErrorFolder"];
            string ErrorFile = ConfigurationSettings.AppSettings["ErrorFile"];
            if (!System.IO.Directory.Exists(ErrorFolder))
            {
                System.IO.Directory.CreateDirectory(ErrorFolder);
            }
            if (!System.IO.File.Exists(ErrorFile))
            {
                System.IO.File.Create(ErrorFile);
            }
        }
        public static void PrepareBranchIds()
        {
            var ids = ConfigurationManager.AppSettings["BranchIds"];
            var BranchNames = ConfigurationManager.AppSettings["BranchNames"];
            
            List<int> TagIds = ids.Split(',').Select(int.Parse).ToList();
            List<string> branchNames2 = BranchNames.Split(',').ToList();
            int count = 0;
            foreach (var id in TagIds)
            {    
                if(TagIds.Count > count)
                {
                    BIdsNames.Add(id, branchNames2[count].ToString());
                    count++;
                }                                   
            }           
            branchList = TagIds;
        }

        public static void ZeroPickedByDateRange(string startDate)
        {           
                dataLayer.ZeroPickedByDateRange(startDate, startDate, BIdsNames);          
        }

        public static void ProductByDateRange(string startDate)
        {        
                dataLayer.ProductByDateRange(startDate, startDate, BIdsNames);         
        }


    }
}
