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
    class SaveMethods
    {
        public static bool createDirectory()
        {
            try
            {
                Directory.CreateDirectory(Config.outputFilePath);
                string text = "Created new directory " + Config.outputFilePath;
                Logging.addToLog(text);
                Console.WriteLine(text);
                return true;
            }
            catch
            {
                string text = "Error creating new directory " + Config.outputFilePath;
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static void makeBackups(string file)
        {
            try
            {
                string backup = file + "_old";

                if (System.IO.File.Exists(file))
                {
                    try
                    {
                        if (System.IO.File.Exists(backup))
                        {
                            System.IO.File.Copy(backup, backup + "1", true);
                        }
                        System.IO.File.Copy(file, backup, true);
                    }
                    catch
                    {
                        //could not copy file
                        string text1 = "Could not copy " + file;
                        Logging.addToLog(text1);
                        Console.WriteLine(text1);
                    }
                }
            }
            catch
            {
                string text = "Error! Unable to save " + file;
                Logging.addToLog(text);
                Console.WriteLine(text);
            }
        }

        public static bool saveUserActivity()
        {
            string file = Path.Combine(Config.outputFilePath, "userActivityData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string data = "";

                string text = "Starting textwriter userActivityData";
                Logging.addToLog(text);
                Console.WriteLine(text);
                using (TextWriter sw = new StreamWriter(file))
                {
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
                return true;
            }
            catch
            {
                string text = "Error! Unable to save userActivityData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool SaveRefLinkFile()
        {
            string file = Path.Combine(Config.outputFilePath, "RefLinkData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter refLinkData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (KeyValuePair<string, string> entry in Program.refLinks)
                    {
                        sw.WriteLine(entry.Key + "?" + entry.Value);
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save refLinkData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool SavePasswordAttempts()
        {
            string file = Path.Combine(Config.outputFilePath, "attemptsData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter attemptsData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (KeyValuePair<string, int> entry in Program.passwordAttempts)
                    {
                        sw.WriteLine(entry.Key + "?" + entry.Value.ToString());
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save attemptsData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool SaveWelcomeFile()
        {
            string file = Path.Combine(Config.outputFilePath, "showWelcomeData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter showWelcomeData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (KeyValuePair<string, bool> entry in Program.showWelcome)
                    {
                        sw.WriteLine(entry.Key + "?" + entry.Value);
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save showWelcomeData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool saveReferredBy()
        {
            string file = Path.Combine(Config.outputFilePath, "referredByData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter referredByData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (KeyValuePair<string, string> entry in Program.referredBy)
                    {
                        sw.WriteLine(entry.Key + "?" + entry.Value);
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save referredByData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool saveChatId()
        {
            string file = Path.Combine(Config.outputFilePath, "groupIDnumber.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter groupIDnumber";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                        sw.WriteLine(Config.groupChatIdNumber.ToString());
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save groupIDnumber.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool savePointsByReferrer()
        {
            string file = Path.Combine(Config.outputFilePath, "pointsByReferrerData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter pointsByReferrerData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (KeyValuePair<string, int> entry in Program.pointsByReferrer)
                    {
                        sw.WriteLine(entry.Key + "?" + entry.Value.ToString());
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save pointsByReferrerData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool SaveUserPointOffset()
        {
            string file = Path.Combine(Config.outputFilePath, "userPointOffsetData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter userPointOffsetData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (KeyValuePair<string, int> entry in Program.userPointOffset)
                    {
                        sw.WriteLine(entry.Key + "?" + entry.Value.ToString());
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save userPointOffsetData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool saveJoinedReferrals()
        {
            string file = Path.Combine(Config.outputFilePath, "joinedReferralsData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter joinedReferralsData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach(KeyValuePair<int, string> entries in Program.joinedReferrals)
                    {
                        sw.WriteLine(entries.Key.ToString() + "?" + entries.Value);
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save joinedReferralsData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool saveInteracedUser()
        {
            string file = Path.Combine(Config.outputFilePath, "interactedUserData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter interactedUserData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (KeyValuePair<int, Dictionary<string, string>> entry in Program.interactedUser)
                    {
                        string result = entry.Key.ToString() + "?????";
                        foreach(KeyValuePair<string, string> value in entry.Value)
                        {
                            result += "&&&&&" + value.Key + "#####" + value.Value;
                        }
                        sw.WriteLine(result);
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save interactedUserData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool saveReferralPoints()
        {
            string file = Path.Combine(Config.outputFilePath, " referralPointsData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter referralPointsData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (KeyValuePair<string, int> entry in Program.referralPoints)
                    {
                        sw.WriteLine(entry.Key + "?" + entry.Value.ToString());
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save referralPointsData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }

        public static bool saveCampaignDays()
        {
            string file = Path.Combine(Config.outputFilePath, "campaignDaysData.518");

            if (!Directory.Exists(Config.outputFilePath))
            {
                bool created = createDirectory();
                if (!created)
                {
                    return false;
                }
            }

            makeBackups(file);

            try
            {
                string text = "Starting textwriter campaignDaysData";
                Logging.addToLog(text);
                Console.WriteLine(text);

                using (TextWriter sw = new StreamWriter(file))
                {
                    foreach (string entry in Program.campaignDays)
                    {
                        sw.WriteLine(entry);
                    }
                }
                return true;
            }
            catch
            {
                string text = "Error! Unable to save campaignDaysData.";
                Logging.addToLog(text);
                Console.WriteLine(text);
                return false;
            }
        }
    }
}