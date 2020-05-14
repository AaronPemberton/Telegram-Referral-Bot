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
    class LoadData
    {
        public static void LoadConf()
        {
            string message = "Beginning load config.";
            Logging.addToLog(message);
            Console.WriteLine(message);

            string file = Path.Combine(Config.outputFilePath, "showWelcomeData.518");
            string backup = file + "_old";

            if (System.IO.File.Exists(file))
            {
                List<string> temp = System.IO.File.ReadAllLines(file).ToList();
                Program.showWelcome.Clear();
                foreach (string s in temp)
                {
                    string[] parse = s.Split('?');
                    bool option = false;
                    if (parse[1] == "True")
                    {
                        option = true;
                    }
                    if (!Program.showWelcome.ContainsKey(parse[0]))
                    {
                        Program.showWelcome.Add(parse[0], option);
                    }
                }
                string text = "showWelcomeData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup))
            {
                List<string> temp = System.IO.File.ReadAllLines(backup).ToList();
                Program.showWelcome.Clear();
                foreach (string s in temp)
                {
                    string[] parse = s.Split('?');
                    bool option = false;
                    if (parse[1] == "True")
                    {
                        option = true;
                    }
                    if (!Program.showWelcome.ContainsKey(parse[0]))
                    {
                        Program.showWelcome.Add(parse[0], option);
                    }
                }
                string text = "showWelcomeData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file1 = Path.Combine(Config.outputFilePath, "RefLinkData.518");
            string backup1 = file1 + "_old";

            if (System.IO.File.Exists(file1))
            {
                List<string> temp1 = System.IO.File.ReadAllLines(file1).ToList();
                Program.refLinks.Clear();
                foreach (string s in temp1)
                {
                    string[] parse = s.Split('?');
                    if (!Program.refLinks.ContainsKey(parse[0]))
                    {
                        Program.refLinks.Add(parse[0], parse[1]);
                    }
                }
                string text = "RefLinkData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup1))
            {
                List<string> temp1 = System.IO.File.ReadAllLines(backup1).ToList();
                Program.refLinks.Clear();
                foreach (string s in temp1)
                {
                    string[] parse = s.Split('?');
                    if (!Program.refLinks.ContainsKey(parse[0]))
                    {
                        Program.refLinks.Add(parse[0], parse[1]);
                    }
                }
                string text = "RefLinkData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file2 = Path.Combine(Config.outputFilePath, "userActivityData.518");
            string backup2 = file2 + "_old";

            if (System.IO.File.Exists(file2))
            {
                List<string> temp2 = System.IO.File.ReadAllLines(file2).ToList();
                Program.userActivity.Clear();
                foreach (string s in temp2)
                {
                    if (s.Contains('?'))
                    {
                        string[] parse = s.Split('?');
                        string userID = parse[0];
                        Dictionary<string, int> temp = new Dictionary<string, int>();
                        foreach (string st in parse)
                        {
                            if (st != userID && st.Contains("&"))
                            {
                                string[] parse1 = st.Split('&');
                                if (parse1.Length == 2)
                                {
                                    try
                                    {
                                        int value = Convert.ToInt32(parse1[1]);
                                        temp.Add(parse1[0], value);
                                    }
                                    catch { }
                                }
                            }
                        }
                        if (temp.Any())
                        {
                            if (!Program.userActivity.ContainsKey(userID))
                            {
                                Program.userActivity.Add(userID, temp);
                            }
                        }
                    }
                }
                string text = "userActivityData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup2))
            {
                List<string> temp2 = System.IO.File.ReadAllLines(backup2).ToList();
                Program.userActivity.Clear();
                foreach (string s in temp2)
                {
                    if (s.Contains('?'))
                    {
                        string[] parse = s.Split('?');
                        string userID = parse[0];
                        Dictionary<string, int> temp = new Dictionary<string, int>();
                        foreach (string st in parse)
                        {
                            if (st != userID && st.Contains("&"))
                            {
                                string[] parse1 = st.Split('&');
                                if (parse1.Length == 2)
                                {
                                    try
                                    {
                                        int value = Convert.ToInt32(parse1[1]);
                                        temp.Add(parse1[0], value);
                                    }
                                    catch { }
                                }
                            }
                        }
                        if (temp.Any())
                        {
                            if (!Program.userActivity.ContainsKey(userID))
                            {
                                Program.userActivity.Add(userID, temp);
                            }
                        }
                    }
                }
                string text = "userActivityData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file3 = Path.Combine(Config.outputFilePath, "referredByData.518");
            string backup3 = file3 + "_old";

            if (System.IO.File.Exists(file3))
            {
                List<string> temp3 = System.IO.File.ReadAllLines(file3).ToList();
                Program.referredBy.Clear();
                foreach (string s in temp3)
                {
                    string[] parse = s.Split('?');
                    if (!Program.referredBy.ContainsKey(parse[0]))
                    {
                        Program.referredBy.Add(parse[0], parse[1]);
                    }
                }
                string text = "referredByData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup3))
            {
                List<string> temp3 = System.IO.File.ReadAllLines(backup3).ToList();
                Program.referredBy.Clear();
                foreach (string s in temp3)
                {
                    string[] parse = s.Split('?');
                    if (!Program.referredBy.ContainsKey(parse[0]))
                    {
                        Program.referredBy.Add(parse[0], parse[1]);
                    }
                }
                string text = "referredByData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file4 = Path.Combine(Config.outputFilePath, "groupIDnumber.518");
            if (System.IO.File.Exists(file4))
            {
                try
                {
                    string[] temp4 = System.IO.File.ReadAllLines(file4);
                    if (temp4.Any())
                    {
                        long groupIDnumber = Convert.ToInt64(temp4[0]);
                        if (groupIDnumber != 0)
                        {
                            Task<Chat> group = getGroup(groupIDnumber);
                            if (group != null)
                            {
                                Config.groupChatIdNumber = groupIDnumber;
                            }
                        }
                    }
                }
                catch
                {
                    Config.groupChatIdNumber = 0;
                }
                string text = "groupIDnumber loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else
            {
                string text = "groupIDnumber file not found. Value set to default (0).";
                Logging.addToLog(text);
                Console.WriteLine(text);

                Config.groupChatIdNumber = 0;
            }

            string file5 = Path.Combine(Config.outputFilePath, "pointsByReferrerData.518");
            string backup5 = file5 + "_old";

            if (System.IO.File.Exists(file5))
            {
                List<string> temp5 = System.IO.File.ReadAllLines(file5).ToList();
                Program.pointsByReferrer.Clear();
                foreach (string s in temp5)
                {
                    string[] parse = s.Split('?');
                    if (!Program.pointsByReferrer.ContainsKey(parse[0]))
                    {
                        int temp = 0;
                        try
                        {
                            temp = Convert.ToInt32(parse[1]);
                        }
                        catch { }
                        Program.pointsByReferrer.Add(parse[0], temp);
                    }
                }
                string text = "pointsByReferrerData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup5))
            {
                List<string> temp5 = System.IO.File.ReadAllLines(backup5).ToList();
                Program.pointsByReferrer.Clear();
                foreach (string s in temp5)
                {
                    string[] parse = s.Split('?');
                    if (!Program.pointsByReferrer.ContainsKey(parse[0]))
                    {
                        int temp = 0;
                        try
                        {
                            temp = Convert.ToInt32(parse[1]);
                        }
                        catch { }
                        Program.pointsByReferrer.Add(parse[0], temp);
                    }
                }
                string text = "pointsByReferrerData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file6 = Path.Combine(Config.outputFilePath, "userPointOffsetData.518");
            string backup6 = file6 + "_old";

            if (System.IO.File.Exists(file6))
            {
                List<string> temp6 = System.IO.File.ReadAllLines(file6).ToList();
                Program.userPointOffset.Clear();
                foreach (string s in temp6)
                {
                    string[] parse = s.Split('?');
                    if (parse.Length == 2)
                    {
                        try
                        {
                            int value = Convert.ToInt32(parse[1]);
                            if (!Program.userPointOffset.ContainsKey(parse[0]))
                            {
                                Program.userPointOffset.Add(parse[0], value);
                            }
                        }
                        catch { }
                    }
                }
                string text = "userPointOffsetData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup6))
            {
                List<string> temp6 = System.IO.File.ReadAllLines(backup6).ToList();
                Program.userPointOffset.Clear();
                foreach (string s in temp6)
                {
                    string[] parse = s.Split('?');
                    if (parse.Length == 2)
                    {
                        try
                        {
                            int value = Convert.ToInt32(parse[1]);
                            if (!Program.userPointOffset.ContainsKey(parse[0]))
                            {
                                Program.userPointOffset.Add(parse[0], value);
                            }
                        }
                        catch { }
                    }
                }
                string text = "userPointOffsetData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file7 = Path.Combine(Config.outputFilePath, "attemptsData.518");
            string backup7 = file7 + "_old";

            if (System.IO.File.Exists(file7))
            {
                List<string> temp7 = System.IO.File.ReadAllLines(file7).ToList();
                Program.passwordAttempts.Clear();
                foreach (string s in temp7)
                {
                    string[] parse = s.Split('?');
                    if (parse.Length == 2)
                    {
                        try
                        {
                            int value = Convert.ToInt32(parse[1]);
                            if (!Program.passwordAttempts.ContainsKey(parse[0]))
                            {
                                Program.passwordAttempts.Add(parse[0], value);
                            }
                        }
                        catch { }
                    }
                }
                string text = "attemptsData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup7))
            {
                List<string> temp7 = System.IO.File.ReadAllLines(backup7).ToList();
                Program.passwordAttempts.Clear();
                foreach (string s in temp7)
                {
                    string[] parse = s.Split('?');
                    if (parse.Length == 2)
                    {
                        try
                        {
                            int value = Convert.ToInt32(parse[1]);
                            if (!Program.passwordAttempts.ContainsKey(parse[0]))
                            {
                                Program.passwordAttempts.Add(parse[0], value);
                            }
                        }
                        catch { }
                    }
                }
                string text = "attemptsData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file8 = Path.Combine(Config.outputFilePath, "joinedReferralsData.518");
            string backup8 = file8 + "_old";

            if (System.IO.File.Exists(file8))
            {
                List<string> temp8 = System.IO.File.ReadAllLines(file8).ToList();
                Program.joinedReferrals.Clear();
                foreach (string s in temp8)
                {
                    try
                    {
                        string[] parse = s.Split('?');
                        if(parse.Length == 2)
                        {
                            int idNum = Convert.ToInt32(parse[0]);
                            Program.joinedReferrals.Add(idNum, parse[1]);
                        }
                    }
                    catch
                    {
                        string text1 = "Unable to add " + s + " to joinedReferrals";
                        Logging.addToLog(text1);
                        Console.WriteLine(text1);
                    }
                }
                string text = "joinedReferralsData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup8))
            {
                List<string> temp8 = System.IO.File.ReadAllLines(backup8).ToList();
                Program.joinedReferrals.Clear();
                foreach (string s in temp8)
                {
                    try
                    {
                        string[] parse = s.Split('?');
                        if (parse.Length == 2)
                        {
                            int idNum = Convert.ToInt32(parse[0]);
                            Program.joinedReferrals.Add(idNum, parse[1]);
                        }
                    }
                    catch
                    {
                        string text1 = "Unable to add " + s + " to joinedReferrals";
                        Logging.addToLog(text1);
                        Console.WriteLine(text1);
                    }
                }
                string text = "joinedReferralsData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file9 = Path.Combine(Config.outputFilePath, "referralPointsData.518");
            string backup9 = file9 + "_old";

            if (System.IO.File.Exists(file9))
            {
                List<string> temp9 = System.IO.File.ReadAllLines(file9).ToList();
                Program.referralPoints.Clear();
                foreach (string s in temp9)
                {
                    string[] temp = s.Split('?');
                    if (temp.Length == 2)
                    {
                        try
                        {
                            int value = Convert.ToInt32(temp[1]);
                            Program.referralPoints.Add(temp[0], value);
                        }
                        catch
                        {
                            Console.WriteLine("Unable to add " + temp[0] + "  " + temp[1] + " to referralPoints");
                        }

                    }
                }
                string text = "referralPointsData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup9))
            {
                List<string> temp9 = System.IO.File.ReadAllLines(backup9).ToList();
                Program.referralPoints.Clear();
                foreach (string s in temp9)
                {
                    string[] temp = s.Split('?');
                    if (temp.Length == 2)
                    {
                        try
                        {
                            int value = Convert.ToInt32(temp[1]);
                            Program.referralPoints.Add(temp[0], value);
                        }
                        catch
                        {
                            Console.WriteLine("Unable to add " + temp[0] + "  " + temp[1] + " to referralPoints");
                        }
                    }
                }
                string text = "referralPointsData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file10 = Path.Combine(Config.outputFilePath, "interactedUserData.518");
            string backup10 = file10 + "_old";

            if (System.IO.File.Exists(file10))
            {
                List<string> temp10 = System.IO.File.ReadAllLines(file10).ToList();
                Program.interactedUser.Clear();
                foreach (string s in temp10)
                {
                    string[] temp = s.Split("?????", StringSplitOptions.RemoveEmptyEntries);
                    if (temp.Length >= 2)
                    {
                        try
                        {
                            int value = Convert.ToInt32(temp[0]);
                            Dictionary<string, string> result = new Dictionary<string, string>();
                            string[] data = temp[1].Split("&&&&&");
                            foreach(string s1 in data)
                            {
                                string[] temp1 = s1.Split("#####");
                                if(temp1.Length == 2)
                                {
                                    result.Add(temp1[0], temp1[1]);
                                }
                            }
                            Program.interactedUser.Add(value, result);
                        }
                        catch
                        {
                            Console.WriteLine("Unable to add " + s + " to interactedUser");
                        }

                    }
                }
                string text = "interactedUserData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup10))
            {
                List<string> temp10 = System.IO.File.ReadAllLines(backup10).ToList();
                Program.interactedUser.Clear();
                foreach (string s in temp10)
                {
                    string[] temp = s.Split("?????", StringSplitOptions.RemoveEmptyEntries);
                    if (temp.Length >= 2)
                    {
                        try
                        {
                            int value = Convert.ToInt32(temp[0]);
                            Dictionary<string, string> result = new Dictionary<string, string>();
                            string[] data = temp[1].Split("&&&&&");
                            foreach (string s1 in data)
                            {
                                string[] temp1 = s1.Split("#####");
                                if (temp1.Length == 2)
                                {
                                    result.Add(temp1[0], temp1[1]);
                                }
                            }
                            Program.interactedUser.Add(value, result);
                        }
                        catch
                        {
                            Console.WriteLine("Unable to add " + s + " to interactedUser");
                        }

                    }
                }
                string text = "interactedUserData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }

            string file11 = Path.Combine(Config.outputFilePath, "campaignDaysData.518");
            string backup11 = file11 + "_old";

            if (System.IO.File.Exists(file11))
            {
                Program.campaignDays.Clear();
                Program.campaignDays = System.IO.File.ReadAllLines(file11).ToList();
                Program.campaignDays.Sort();
                string text = "campaignDaysData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else if (System.IO.File.Exists(backup11))
            {
                Program.campaignDays.Clear();
                Program.campaignDays = System.IO.File.ReadAllLines(backup11).ToList();
                Program.campaignDays.Sort();
                string text = "campaignDaysData loaded.";
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
            else
            {
                createCampaignDays();
            }
        }

        public static async Task<Chat> getGroup(long groupID)
        {
            string text = "Start getGroup.";
            Logging.addToLog(text);
            Console.WriteLine(text);

            Chat group = new Chat();
            group = await Program.botClient.GetChatAsync(groupID);
            return group;
        }

        public static void createCampaignDays()
        {
            Program.campaignDays.Clear();
            DateTime startDate = Convert.ToDateTime(Config.startDate);
            for (int i = 0; i < Config.numberOfDays; i++)
            {
                DateTime dateTime = startDate.AddDays(i);
                string date = dateTime.ToString("MM/dd/yyyy");
                Program.campaignDays.Add(date);
            }
            Logging.addToLog("Created campaignDays list");
            SaveMethods.saveCampaignDays();
        }
    }
}