using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramReferralBot
{
    class Logging
    {
        public static void addToLog(string text)
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            string filename = "Logfile-" + today + "_1.txt";
            string folder = Path.Combine(Config.outputFilePath, "Logs");
            string file = Path.Combine(folder, filename);

            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    string msg = "Created new directory " + folder;
                    Logging.addToLog(msg);
                    Console.WriteLine(msg);
                }

                if (!System.IO.File.Exists(file))
                {
                    //dump current data into log
                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("userActivity=");
                        string data = "";
                        foreach (KeyValuePair<string, Dictionary<string, int>> entry in Program.userActivity)
                        {
                            data = entry.Key;
                            foreach (KeyValuePair<string, int> line in entry.Value)
                            {
                                data += "?" + line.Key + "&" + line.Value.ToString();
                            }

                            sw.WriteLine(data);
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("refLinks=");
                        foreach (KeyValuePair<string, string> entry in Program.refLinks)
                        {
                            sw.WriteLine(entry.Key + "?" + entry.Value);
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("passwordAttempts=");
                        foreach (KeyValuePair<string, int> entry in Program.passwordAttempts)
                        {
                            sw.WriteLine(entry.Key + "?" + entry.Value.ToString());
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("showWelcome=");
                        foreach (KeyValuePair<string, bool> entry in Program.showWelcome)
                        {
                            sw.WriteLine(entry.Key + "?" + entry.Value);
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("referredBy=");
                        foreach (KeyValuePair<string, string> entry in Program.referredBy)
                        {
                            sw.WriteLine(entry.Key + "?" + entry.Value);
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("groupChatIdNumber=");
                        sw.WriteLine(Config.groupChatIdNumber.ToString());
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("pointsByReferrer=");
                        foreach (KeyValuePair<string, int> entry in Program.pointsByReferrer)
                        {
                            sw.WriteLine(entry.Key + "?" + entry.Value.ToString());
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("userPointOffset=");
                        foreach (KeyValuePair<string, int> entry in Program.userPointOffset)
                        {
                            sw.WriteLine(entry.Key + "?" + entry.Value.ToString());
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        foreach (KeyValuePair<int, string> entries in Program.joinedReferrals)
                        {
                            sw.WriteLine(entries.Key.ToString() + "?" + entries.Value);
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("referralPoints=");
                        foreach (KeyValuePair<string, int> entry in Program.referralPoints)
                        {
                            sw.WriteLine(entry.Key + "?" + entry.Value.ToString());
                        }
                    }

                    using (StreamWriter sw = System.IO.File.AppendText(file))
                    {
                        sw.WriteLine("interactedUser=");
                        foreach (KeyValuePair<int, Dictionary<string, string>> entry in Program.interactedUser)
                        {
                            string result = entry.Key.ToString() + "?????";
                            foreach (KeyValuePair<string, string> value in entry.Value)
                            {
                                result += "&&&&&" + value.Key + "#####" + value.Value;
                            }
                            sw.WriteLine(result);
                        }
                    }
                }
                else if (new FileInfo(file).Length > 1073741824)  //max file size for telegram is 1.5GB, 50MB if sent by bot. This keeps files under 1GB
                {
                    List<string> files = Directory.GetFiles(folder).ToList();
                    int i = 1;
                    int num = 0;
                    foreach (string item in files)
                    {
                        if (item.Contains(today))
                        {
                            string[] temp = item.Split('_');
                            string number = temp[temp.Length - 1].Replace(".txt", "");
                            try
                            {
                                num = Convert.ToInt32(number);
                            }
                            catch { }
                            if (num > i)
                            {
                                i = num;
                            }
                        }
                    }
                    i++;
                    filename = "Logfile-" + today + "_" + i.ToString() + ".txt";
                    file = Path.Combine(folder, filename);
                }

                using (StreamWriter sw = System.IO.File.AppendText(file))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString("yyyyMMddHHmmssffff") + ":  " + text);
                }
            }
            catch { }
        }
    }
}