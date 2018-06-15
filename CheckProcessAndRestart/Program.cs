using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckProcessAndRestart
{
    class Program
    {
        static bool blDebug = false;

        static void Main(string[] args)
        {
            if (args.Contains("debug"))
                blDebug = true;

            List<Model.ComputerProcess> objComputerList = new List<Model.ComputerProcess>();
            List<Model.ComputerProcess> objSendModel = new List<Model.ComputerProcess>();

            try
            {
                // 載入清單
                objComputerList = JsonConvert.DeserializeObject<List<Model.ComputerProcess>>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Config.json"));

                Process currentProcess = Process.GetCurrentProcess();
                for (int c = 0; c < objComputerList.Count; c++)
                {
                    List<Process> objComputerProcess = Process.GetProcesses(objComputerList[c].ComputerName).ToList();
                    PrintLine("Computer:" + objComputerList[c].ComputerName + ", Get Processes.");

                    // 判定指定的process是否存在
                    for (int i = 0; i < objComputerList[c].ProcessName.Count; i++)
                    {
                        int intCount = objComputerProcess.Where(x => x.ProcessName.ToLower() == objComputerList[c].ProcessName[i].Name).Count();

                        // 如果有發現不存在的Process，就建立通知物件並放入清單
                        if (intCount == 0)
                        {
                            Model.ComputerProcess objComputer = objSendModel.Where(x => x.ComputerName == objComputerList[c].ComputerName).FirstOrDefault();

                            if (objComputer == null)
                            {
                                objSendModel.Add(new Model.ComputerProcess()
                                {
                                    ComputerName = objComputerList[c].ComputerName,
                                    ProcessName = new List<Model.ComputerProcess.ComputerProcessItem>()
                                    {
                                        new Model.ComputerProcess.ComputerProcessItem()
                                        {
                                            Mode = objComputerList[c].ProcessName[i].Mode,
                                            Name = objComputerList[c].ProcessName[i].Name,
                                            Path = objComputerList[c].ProcessName[i].Path,
                                            Directory = objComputerList[c].ProcessName[i].Directory,                                        }
                                    }
                                });
                            }
                            else
                            {
                                objComputer.ProcessName.Add(new Model.ComputerProcess.ComputerProcessItem()
                                {
                                    Mode = objComputerList[c].ProcessName[i].Mode,
                                    Name = objComputerList[c].ProcessName[i].Name,
                                    Path = objComputerList[c].ProcessName[i].Path,
                                    Directory = objComputerList[c].ProcessName[i].Directory,
                                });
                            }

                            if (objComputerList[c].ProcessName[i].Mode.ToLower() == "restart")
                            {
                                ProcessStartInfo pInfo = new ProcessStartInfo(objComputerList[c].ProcessName[i].Path);
                                pInfo.Arguments = objComputerList[c].ProcessName[i].Args;
                                pInfo.CreateNoWindow = true;
                                pInfo.UseShellExecute = true;
                                pInfo.WorkingDirectory = objComputerList[c].ProcessName[i].Directory;
                                var process = Process.Start(pInfo);
                            }
                        }
                    }
                }

                // 如果有警示的內容就寄信
                if (objSendModel.Count > 0)
                {
                    string strSubject = ConfigurationManager.AppSettings["SMTP_SUBJECT"].ToString();
                    string strBody = ConfigurationManager.AppSettings["SMTP_BODY"].ToString();

                    // 處理Subject
                    string strComputerList = String.Join(", ", objSendModel.Select(c => c.ComputerName).ToArray());
                    strSubject = strSubject.Replace("{$ComputerName}", strComputerList);

                    // 處理Body
                    string strBodyContent = "";
                    for (int i = 0; i < objSendModel.Count; i++)
                    {
                        Model.ComputerProcess strProcesses = objSendModel.Where(x => x.ComputerName == objSendModel[i].ComputerName).FirstOrDefault();
                        for (int p = 0; p < strProcesses.ProcessName.Count; p++)
                        {
                            string strBodyLine = strBody;
                            strBodyLine = strBodyLine.Replace("{$ComputerName}", objSendModel[i].ComputerName);
                            string strProcessInfo = strProcesses.ProcessName[p].Name + ":" + strProcesses.ProcessName[p].Mode + ":" + strProcesses.ProcessName[p].Path;
                            strBodyLine = strBodyLine.Replace("{$ProcessName}", strProcessInfo);
                            strBodyContent += "<p>" + strBodyLine + "</p>";
                        }
                    }

                    // 寄出email
                    PrintLine("subject:" + strSubject);
                    PrintLine("Body:" + System.Web.HttpUtility.HtmlDecode(strBodyContent));
                    string strMsg = Utility.SendMail(strSubject, System.Web.HttpUtility.HtmlDecode(strBodyContent), true);
                    PrintLine("EMail Send:" + strMsg);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:" + e.Message);
                Console.WriteLine("Exception:" + e.InnerException);
                Console.ReadKey();
            }
        }

        static private void PrintLine(string strMsg)
        {
            if (blDebug)
                Console.WriteLine(strMsg);
        }
    }
}
