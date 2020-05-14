using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    class Program
    {
        public static ITelegramBotClient botClient;
        public static Dictionary<string, bool> showWelcome = new Dictionary<string, bool>();
        public static Dictionary<string, string> refLinks = new Dictionary<string, string>();
        public static Dictionary<string, int> passwordAttempts = new Dictionary<string, int>();
        public static Dictionary<string, Dictionary<string, int>> userActivity = new Dictionary<string, Dictionary<string, int>>();
        public static Dictionary<string, string> referredBy = new Dictionary<string, string>();
        public static Dictionary<string, int> pointTotals = new Dictionary<string, int>();
        public static Dictionary<string, int> userPointOffset = new Dictionary<string, int>();
        public static Dictionary<string, int> pointsByReferrer = new Dictionary<string, int>();
        public static Dictionary<int, string> joinedReferrals = new Dictionary<int, string>();
        public static Dictionary<string, int> referralPoints = new Dictionary<string, int>();
        public static Dictionary<int, Dictionary<string, string>> interactedUser = new Dictionary<int, Dictionary<string, string>>();
        public static List<string> campaignDays = new List<string>();
        static User bot;
        static List<string> awaitingReply = new List<string>();

        static void Main()
        {
            botClient = new TelegramBotClient(Config.BotAccessToken);
            var me = botClient.GetMeAsync().Result;
            bot = me;
            Console.WriteLine($"I am user {me.Id} and my name is {me.FirstName}.");
            LoadData.LoadConf();
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
            Main();
        }

        public static Dictionary<string, int> createUserActivityDictionary(string userID)
        {
            Logging.addToLog("creating user activity dictionary for " + userID);
            if (userActivity.ContainsKey(userID))
            {
                Logging.addToLog("user activity dictionary already exists for " + userID);
                return userActivity[userID];
            }
            else
            {
                Dictionary<string, int> perDay = new Dictionary<string, int>();
                DateTime startDate = Convert.ToDateTime(Config.startDate);
                for (int i = 0; i < Config.numberOfDays; i++)
                {
                    DateTime dateTime = startDate.AddDays(i);
                    string date = dateTime.ToString("MM/dd/yyyy");
                    perDay.Add(date, 0);
                }
                userActivity.Add(userID, perDay);
                Logging.addToLog("added new user activity dictionary for " + userID);
                bool temp = SaveMethods.saveUserActivity();

                if (temp)
                {
                    string message = "UserActivity saved.";
                    Logging.addToLog(message);
                    Console.WriteLine(message);

                    return userActivity[userID];
                }
                else
                {
                    string message = "Error! UserActivity not saved.";
                    Logging.addToLog(message);
                    Console.WriteLine(message);

                    return null;
                }
            }
        }

        internal static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static string GetRefLink(User user)
        {
            Logging.addToLog("getting reflink for " + user.Id.ToString());

            string userID = user.Id.ToString();
            string base64String = "";
            if (refLinks.ContainsKey(userID))
            {
                Logging.addToLog("reflink already exists for " + user.Id.ToString());

                string link = Config.linkToBot + "?start=" + refLinks[userID];
                return "Exists?" + link;
            }
            else
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(userID);
                // Special "url-safe" base64 encode.
                base64String = Convert.ToBase64String(inputBytes)
                  .Replace('+', '-') // replace URL unsafe characters with safe ones
                  .Replace('/', '_') // replace URL unsafe characters with safe ones
                  .Replace("=", ""); // no padding

                if (refLinks.ContainsValue(base64String))
                {
                    bool unique = false;
                    int itteration = 0;
                    while (!unique)
                    {
                        itteration++;
                        if (itteration > 10)
                        {
                            Logging.addToLog("Error! Unable to generate unique link. " + user.Id.ToString());

                            unique = true;
                            return "Error?Unable to generate unique link.";
                        }
                        else
                        {
                            int size = 16;
                            byte[] data = new byte[4 * size];
                            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
                            {
                                crypto.GetBytes(data);
                            }
                            StringBuilder result = new StringBuilder(size);
                            for (int i = 0; i < size; i++)
                            {
                                var rnd = BitConverter.ToUInt32(data, i * 4);
                                var idx = rnd % chars.Length;

                                result.Append(chars[idx]);
                            }

                            if (refLinks.ContainsValue(result.ToString()))
                            {
                                unique = false;
                            }
                            else
                            {
                                base64String = result.ToString();
                                unique = true;
                                break;
                            }
                        }
                    }
                }

                if (!base64String.Contains("Error") && !base64String.Contains("Exist"))
                {
                    string link = Config.linkToBot + "?start=" + base64String;
                    refLinks.Add(userID, base64String);
                    Logging.addToLog("reflink " + base64String + " created for " + user.Id.ToString());
                    return link;
                }
                else
                {
                    if (base64String.Contains("Error"))
                    {
                        Logging.addToLog("Error! base64String contained an error message " + user.Id.ToString());
                    }
                    return "";
                }
            }
        }

        public static void updateInteractedUser(User user)
        {
            Dictionary<string, string> currentInfo = new Dictionary<string, string>();
            currentInfo.Add("Username", user.Username);
            currentInfo.Add("First", user.FirstName);
            currentInfo.Add("Last", user.LastName);

            if (interactedUser.ContainsKey(user.Id))
            {
                string message = "Checking existing user info";
                Console.WriteLine(message);
                Logging.addToLog(message);
                bool modified = false;


                Dictionary<string, string> temp = interactedUser[user.Id];

                if (temp.ContainsKey("Username"))
                {
                    if(temp["Username"] != currentInfo["Username"])
                    {
                        temp["Username"] = currentInfo["Username"];
                        modified = true;
                    }
                }
                else
                {
                    temp.Add("Username", user.Username);
                    modified = true;
                }
                if (temp.ContainsKey("First"))
                {
                    if (temp["First"] != currentInfo["First"])
                    {
                        temp["First"] = currentInfo["First"];
                        modified = true;
                    }
                }
                else
                {
                    temp.Add("First", user.FirstName);
                    modified = true;
                }
                if (temp.ContainsKey("Last"))
                {
                    if (temp["Last"] != currentInfo["Last"])
                    {
                        temp["Last"] = currentInfo["Last"];
                        modified = true;
                    }
                }
                else
                {
                    temp.Add("Last", user.LastName);
                    modified = true;
                }

                if (modified)
                {
                    string text = "Modified existing user info " + user.Id.ToString();
                    Console.WriteLine(text);
                    Logging.addToLog(text);

                    interactedUser[user.Id] = temp;
                    SaveMethods.saveInteracedUser();
                }
            }
            else
            {
                string text = "Added new user info " + user.Id.ToString();
                Console.WriteLine(text);
                Logging.addToLog(text);

                interactedUser.Add(user.Id, currentInfo);
                SaveMethods.saveInteracedUser();
            }
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Logging.addToLog("Message recieved from chat " + e.Message.Chat.Id.ToString() + " from user " + e.Message.From.Id.ToString());

            MessageType type = e.Message.Type;
            if(type == MessageType.ChatMembersAdded && e.Message.Chat.Id == Config.groupChatIdNumber)
            {
                Logging.addToLog("New user message.");

                User[] newUsers = e.Message.NewChatMembers;
                foreach(User u in newUsers)
                {
                    updateInteractedUser(u);

                    string message = u.Id.ToString() + " joined chat " + e.Message.Chat.Id.ToString();
                    Console.WriteLine(message);
                    Logging.addToLog(message);

                    if (referredBy.ContainsKey(u.Id.ToString()) && !joinedReferrals.ContainsKey(u.Id) && !u.IsBot)
                    {
                        string referredByID = referredBy[u.Id.ToString()];
                        joinedReferrals.Add(u.Id, DateTime.UtcNow.ToString("MM/dd/yyyy"));
                        if (campaignDays.Contains(DateTime.UtcNow.ToString("MM/dd/yyyy")))
                        {
                            if (referralPoints.ContainsKey(referredByID))
                            {
                                int total = referralPoints[referredByID];
                                total++;
                                referralPoints[referredByID] = total;
                            }
                            else
                            {
                                referralPoints.Add(referredByID, 1);
                            }
                            string text = "referral point added to user.";
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                        }

                        bool saveJoinedRefs = SaveMethods.saveJoinedReferrals();
                        if (saveJoinedRefs)
                        {
                            string text = "JoinedReferrals saved.";
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                        }
                        else
                        {
                            string text = "Error! JoinedReferrals not saved.";
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                        }
                        bool saveRefPoints = SaveMethods.saveReferralPoints();
                        if (saveRefPoints)
                        {
                            string text = "ReferralPoints saved.";
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                        }
                        else
                        {
                            string text = "Error! ReferralPoints not saved.";
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                        }

                        Logging.addToLog("new member " + u.Id.ToString() + " was referred by " + referredByID);
                        if (showWelcome.ContainsKey(Config.groupChatIdNumber.ToString()) && showWelcome[Config.groupChatIdNumber.ToString()] == true)
                        {
                            string refID = referredBy[u.Id.ToString()];
                            int referrerID = Convert.ToInt32(refID);
                            ChatMember member = await botClient.GetChatMemberAsync(Config.groupChatIdNumber, referrerID);
                            if (member != null)
                            {
                                Console.WriteLine(u.Id.ToString() + " joined chat and was referred by " + member.User.Id);
                                if (!string.IsNullOrWhiteSpace(e.Message.From.Username))
                                {
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Welcome @" + e.Message.From.Username + ", friend of @" + member.User.Username + "!");
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Welcome, friend of @" + member.User.Username + "!");
                                }
                            }
                        }
                    }
                }
            }
            else if (e.Message.Text != null)
            {
                string messageText = e.Message.Text;
                string senderID = e.Message.From.Id.ToString();
                string chatID = e.Message.Chat.Id.ToString();

                if (type == MessageType.Text)
                {
                    if (messageText.StartsWith("/getRefLink"))
                    {
                        updateInteractedUser(e.Message.From);

                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            string message = senderID + " requested reflink from a public chat " + chatID;
                            Logging.addToLog(message);
                            Console.WriteLine(message);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Please message the bot in private to use this command." + System.Environment.NewLine + "@" + bot.Username);
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(e.Message.From.Username))
                            {
                                string text = "Username is null or blank. getRefLink cancelled. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(text);
                                Console.WriteLine(text);
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username to use this command.");
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                            }
                            else
                            {
                                string message = senderID + " requested reflink from a private chat " + chatID;
                                Logging.addToLog(message);
                                Console.WriteLine(message);

                                User messageSender = e.Message.From;
                                string refLink = GetRefLink(messageSender);
                                if (string.IsNullOrWhiteSpace(refLink))
                                {
                                    string text = "RefLink is null or empty. User: " + senderID + " Chat: " + chatID;
                                    Logging.addToLog(text);
                                    Console.WriteLine(text);
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Unable to generate referral link.");
                                }
                                else if (refLink.Contains("Error"))
                                {
                                    string parse = refLink.Replace("?", "! ");
                                    string text = "RefLink resulted in error. User: " + senderID + " Chat: " + chatID + " " + parse;
                                    Logging.addToLog(text);
                                    Console.WriteLine(text);
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: parse);
                                }
                                else if (refLink.Contains("Exist"))
                                {
                                    string[] parse = refLink.Split('?');
                                    string text = "RefLink exists for user. User: " + senderID + " Chat: " + chatID;
                                    Logging.addToLog(text);
                                    Console.WriteLine(text);
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "A refferal link has already been generated for @" + e.Message.From.Username + System.Environment.NewLine + "Your refferal link is:" + System.Environment.NewLine + parse[1] + "?" + parse[2]);
                                }
                                else if (refLink.Contains("http"))
                                {
                                    bool saved = SaveMethods.SaveRefLinkFile();
                                    if (saved)
                                    {
                                        string text = "RefLink created for user and refLinkFile saved. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Your refferal link is:" + System.Environment.NewLine + refLink);
                                    }
                                    else
                                    {
                                        string text = "Unable to save refLink created for user. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Unable to generate referral link.");
                                    }
                                }
                                else
                                {
                                    string text = "Unknown error generating ref link. User: " + senderID + " Chat: " + chatID;
                                    Logging.addToLog(text);
                                    Console.WriteLine(text);
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Unable to generate referral link.");
                                }
                            }
                        }
                    }
                    else if (messageText.StartsWith("/disableWelcome"))
                    {
                        updateInteractedUser(e.Message.From);

                        string message = "Received a disableWelcome message. User: " + senderID + " Chat: " + chatID;
                        Logging.addToLog(message);
                        Console.WriteLine(message);

                        User messageSender = e.Message.From;
                        ChatMember chatMember = await botClient.GetChatMemberAsync(e.Message.Chat.Id, messageSender.Id);
                        if (!messageSender.IsBot && !string.IsNullOrWhiteSpace(messageSender.Username))
                        {
                            if (chatMember.Status == ChatMemberStatus.Creator || chatMember.Status == ChatMemberStatus.Administrator)
                            {
                                if (showWelcome.ContainsKey(chatID))
                                {
                                    if (showWelcome[chatID] != false)
                                    {
                                        showWelcome[chatID] = false;
                                        bool saved = SaveMethods.SaveWelcomeFile();
                                        if (saved)
                                        {
                                            string text = "Disabled Welcome and saved welcomeData. User: " + senderID + " Chat: " + chatID;
                                            Logging.addToLog(text);
                                            Console.WriteLine(text);
                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Succesfully disabled the Welcome feature.");
                                        }
                                        else
                                        {
                                            string text = "Error saving welcomeData. User: " + senderID + " Chat: " + chatID;
                                            Logging.addToLog(text);
                                            Console.WriteLine(text);
                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error saving the Welcome feature settings.");
                                        }
                                    }
                                    else
                                    {
                                        string text = "Welcome is already disabled in this chat. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Welcome feature is already disabled.");
                                    }
                                }
                                else
                                {
                                    showWelcome.Add(chatID, false);
                                    bool saved = SaveMethods.SaveWelcomeFile();
                                    if (saved)
                                    {
                                        string text = "Disabled Welcome and saved welcomeData. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Succesfully disabled the Welcome feature.");
                                    }
                                    else
                                    {
                                        string text = "Error saving welcomeData. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error saving the Welcome feature settings.");
                                    }
                                }
                            }
                            else
                            {
                                string text = "User not admin. Welcome feature not disabled. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(text);
                                Console.WriteLine(text);
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Only Administrators and Creators of a group can use this command.");
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(messageSender.Username))
                        {
                            string text = "Username is null or blank. Welcome feature not disabled. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username to use this command.");
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                        }
                    }
                    else if (messageText.StartsWith("/enableWelcome"))
                    {
                        updateInteractedUser(e.Message.From);

                        string message = "Received a enableWelcome message. User: " + senderID + " Chat: " + chatID;
                        Logging.addToLog(message);
                        Console.WriteLine(message);

                        User messageSender = e.Message.From;
                        ChatMember chatMember = await botClient.GetChatMemberAsync(e.Message.Chat.Id, messageSender.Id);
                        if (!messageSender.IsBot && !string.IsNullOrWhiteSpace(messageSender.Username))
                        {
                            if (chatMember.Status == ChatMemberStatus.Creator || chatMember.Status == ChatMemberStatus.Administrator)
                            {
                                if (showWelcome.ContainsKey(chatID))
                                {
                                    if (showWelcome[chatID] != true)
                                    {
                                        showWelcome[chatID] = true;
                                        bool saved = SaveMethods.SaveWelcomeFile();
                                        if (saved)
                                        {
                                            string text = "Enabled Welcome and saved welcomeData. User: " + senderID + " Chat: " + chatID;
                                            Logging.addToLog(text);
                                            Console.WriteLine(text);
                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Succesfully enabled the Welcome feature.");
                                        }
                                        else
                                        {
                                            string text = "Error saving welcomeData. User: " + senderID + " Chat: " + chatID;
                                            Logging.addToLog(text);
                                            Console.WriteLine(text);
                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error saving the Welcome feature settings.");
                                        }
                                    }
                                    else
                                    {
                                        string text = "Welcome is already enabled in this chat. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Welcome feature is already enabled.");
                                    }
                                }
                                else
                                {
                                    showWelcome.Add(chatID, true);
                                    bool saved = SaveMethods.SaveWelcomeFile();
                                    if (saved)
                                    {
                                        string text = "Enabled Welcome and saved welcomeData. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Succesfully enabled the Welcome feature.");
                                    }
                                    else
                                    {
                                        string text = "Error saving welcomeData. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error saving the Welcome feature settings.");
                                    }
                                }
                            }
                            else
                            {
                                string text = "User not admin. Welcome feature not enabled. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(text);
                                Console.WriteLine(text);
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Only Administrators and Creators of a group can use this command.");
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(messageSender.Username))
                        {
                            string text = "Username is null or blank. Welcome feature not enabled. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username to use this command.");
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                        }
                    }
                    else if (messageText.StartsWith("/start"))
                    {
                        updateInteractedUser(e.Message.From);

                        User messageSender = e.Message.From;
                        if (!messageSender.IsBot && !string.IsNullOrWhiteSpace(messageSender.Username))
                        {
                            if (e.Message.Chat.Type == ChatType.Private)
                            {
                                string message = "Received a private start message. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(message);
                                Console.WriteLine(message);
                                try
                                {
                                    if (messageText.Contains(" "))
                                    {
                                        string[] parse = messageText.Split(' ');
                                        string payload = parse[parse.Length - 1];
                                        string payloadText = "Private start message payload: " + payload + " User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(payloadText);
                                        Console.WriteLine(payloadText);

                                        if (!userActivity.ContainsKey(senderID))
                                        {
                                            Dictionary<string, int> activityOfUser = createUserActivityDictionary(senderID);
                                        }
                                        if (!referredBy.ContainsKey(senderID))
                                        {
                                            string referrer = "";
                                            if (refLinks.ContainsValue(payload))
                                            {
                                                foreach (KeyValuePair<string, string> entry in refLinks)
                                                {
                                                    if (entry.Value == payload)
                                                    {
                                                        referrer = entry.Key;
                                                        break;
                                                    }
                                                }
                                                if (senderID == referrer)
                                                {
                                                    string msg = "Can not refer yourself. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(msg);
                                                    Console.WriteLine(msg);
                                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "You can not refer yourself. Share your referral code with others to gain points.");
                                                }
                                                else
                                                {
                                                    referredBy.Add(senderID, referrer);
                                                    bool saved = SaveMethods.saveReferredBy();
                                                    if (saved)
                                                    {
                                                        string msg = "referredByData has been saved. User: " + senderID + " Referrer: " + referrer + " Chat: " + chatID;
                                                        Logging.addToLog(msg);
                                                        Console.WriteLine(msg);
                                                        if (!string.IsNullOrWhiteSpace(referrer) && Config.groupChatIdNumber != 0)
                                                        {
                                                            try
                                                            {
                                                                int referrerID = Convert.ToInt32(referrer);
                                                                ChatMember member = await botClient.GetChatMemberAsync(Config.groupChatIdNumber, referrerID);
                                                                string text = senderID + " was referred by " + referrer;
                                                                Logging.addToLog(text);
                                                                Console.WriteLine(text);
                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Welcome, friend of @" + member.User.Username + "!");
                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "To complete the signup process, please use this link to join the group." +
                                                                    System.Environment.NewLine + System.Environment.NewLine + Config.linkToGroup);
                                                            }
                                                            catch
                                                            {
                                                                string text = "Unable to get group. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text);
                                                                Console.WriteLine(text);
                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Unable to update data at this time. Please try the referral link again later.");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        string text = "Unable to save referredByData. User: " + senderID + " Chat: " + chatID;
                                                        Logging.addToLog(text);
                                                        Console.WriteLine(text);
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Unable to update data at this time. Please try the referral link again later.");
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        string text = senderID + " was referred by nobody";
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "To complete the signup process, please use this link to join the group." +
                                            System.Environment.NewLine + System.Environment.NewLine + Config.linkToGroup);
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                string message = "Received a group start message. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(message);
                                Console.WriteLine(message);
                                if (messageText.Contains(" "))
                                {
                                    string[] parse = messageText.Split(' ');
                                    string payload = parse[parse.Length - 1];
                                    if (payload.Contains("added2group"))
                                    {
                                        Config.groupChatIdNumber = e.Message.Chat.Id;
                                        bool saved = SaveMethods.saveChatId();

                                        if (saved)
                                        {
                                            var me = botClient.GetMeAsync().Result;
                                            string text = "Bot added to group " + chatID + " and groupChatIdNumber file was saved";
                                            Logging.addToLog(text);
                                            Console.WriteLine(text);
                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Hello, " + e.Message.Chat.Title + "!" + System.Environment.NewLine + "I am @" + me.Username + System.Environment.NewLine + "Please message me in private for a list of commands");
                                        }
                                    }
                                }
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(messageSender.Username))
                        {
                            string text = "Username is null or blank. start data not logged. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username and then use the link to start a conversation with this bot again.");
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                        }
                    }
                    else if (messageText.StartsWith("/myID"))
                    {
                        updateInteractedUser(e.Message.From);

                        string message = "Received id request message. User: " + senderID + " Chat: " + chatID;
                        Logging.addToLog(message);
                        Console.WriteLine(message);
                        string username = e.Message.From.Username;
                        string msg = "";
                        if (string.IsNullOrWhiteSpace(username))
                        {
                            msg = "Your";
                        }
                        else
                        {
                            msg = "@" + username + ", your";
                        }
                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: msg + " Telegram user ID is:" + System.Environment.NewLine + senderID);
                    }
                    else if (messageText.StartsWith("/editUser"))
                    {
                        updateInteractedUser(e.Message.From);

                        if (e.Message.Chat.Type != ChatType.Private)
                        {
                            string message = "Requested editUser from group chat. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(message);
                            Console.WriteLine(message);
                        }
                        else if (!string.IsNullOrWhiteSpace(e.Message.From.Username))
                        {
                            string message = "Requested editUser from private chat. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(message);
                            Console.WriteLine(message);

                            awaitingReply.Add(chatID + "?" + senderID);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Enter:" +
                                System.Environment.NewLine + "/pass <PASSWORD>" + System.Environment.NewLine +
                                "Replacing <PASSWORD> with the admin password.");
                        }
                        else if (string.IsNullOrWhiteSpace(e.Message.From.Username))
                        {
                            string text = "Username is null or blank. editUser cancelled. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username to use this command.");
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                        }
                    }
                    else if (messageText.StartsWith("/top10"))
                    {
                        updateInteractedUser(e.Message.From);

                        string message = "Requested top 10. User: " + senderID + " Chat: " + chatID;
                        Logging.addToLog(message);
                        Console.WriteLine(message);

                        if (!string.IsNullOrWhiteSpace(e.Message.From.Username))
                        {

                            updatePointTotals();
                            bool foundSender = false;
                            var sorted = pointsByReferrer.OrderByDescending(x => x.Value).ThenBy(x => x.Key);
                            List<string> places = new List<string>();

                            foreach (KeyValuePair<string, int> entry in sorted)
                            {
                                try
                                {
                                    if (entry.Key == senderID)
                                    {
                                        foundSender = true;
                                    }

                                    int userID = Convert.ToInt32(entry.Key);
                                    ChatMember member = await botClient.GetChatMemberAsync(Config.groupChatIdNumber, userID);

                                    places.Add(") @" + member.User.Username + "  Points: " + entry.Value.ToString());
                                }
                                catch { }

                                if (places.Count >= 10)
                                {
                                    break;
                                }
                            }
                            if (places.Any())
                            {
                                string msg = "Compiled top 10 list. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(msg);
                                Console.WriteLine(msg);

                                string text = "Current referral program rankings are:" + System.Environment.NewLine + System.Environment.NewLine;
                                int i = 1;
                                foreach (string s in places)
                                {
                                    text += i.ToString() + s + System.Environment.NewLine;
                                    i++;
                                }
                                if (!foundSender)
                                {
                                    string points = "";
                                    int PlaceOfUser = 1;
                                    foreach (KeyValuePair<string, int> entries in sorted)
                                    {
                                        if (entries.Key == senderID)
                                        {
                                            points = entries.Value.ToString();
                                            break;
                                        }
                                        else
                                        {
                                            PlaceOfUser++;
                                        }
                                    }
                                    if (!string.IsNullOrWhiteSpace(points))
                                    {
                                        text += "..." + System.Environment.NewLine + ".." + System.Environment.NewLine + "." + System.Environment.NewLine + PlaceOfUser.ToString() + ") @" + e.Message.From.Username + "  Points: " + points;
                                    }
                                }

                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: text);
                            }
                            else
                            {
                                string msg = "Top 10 list is empty. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(msg);
                                Console.WriteLine(msg);
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "There is currently no points assigned to users.");
                            }
                        }
                        else
                        {
                            string text = "Username is null or blank. top10 cancelled. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username to use this command.");
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                        }
                    }
                    else if (messageText.StartsWith("/pass"))
                    {
                        updateInteractedUser(e.Message.From);

                        if (awaitingReply.Contains(chatID + "?" + senderID))
                        {
                            if (!e.Message.From.IsBot && !string.IsNullOrWhiteSpace(e.Message.From.Username))
                            {
                                if (e.Message.Chat.Type == ChatType.Private)
                                {
                                    string message = "Recieved a password message. User: " + senderID + " Chat: " + chatID;
                                    Logging.addToLog(message);
                                    Console.WriteLine(message);
                                    if (messageText.Contains(" "))
                                    {
                                        awaitingReply.Remove(chatID + "?" + senderID);
                                        string[] parse = messageText.Split(' ');
                                        string payload = parse[parse.Length - 1];

                                        if (parse.Length == 2)
                                        {
                                            string result = checkPassword(payload, senderID);
                                            if (!string.IsNullOrWhiteSpace(result))
                                            {
                                                if (result.StartsWith("wrong"))
                                                {
                                                    string[] parse1 = result.Split('_');
                                                    string text = "Recieved an incorrect password. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);
                                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Incorrect password. You have failed " + parse1[parse1.Length - 1] + " out of 10 attempts before you are banned.");
                                                }
                                                else if (result == "banned")
                                                {
                                                    string text = "User banned from login for exceeding failed password attempts. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);
                                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Incorrect password. You have failed 10 out of 10 attempts." + System.Environment.NewLine + "You are now banned from attempting to login to the bot.");
                                                }
                                                else if (result == "confirmed")
                                                {
                                                    string text = "Password confirmed. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);
                                                    awaitingReply.Add(chatID + "?" + senderID);
                                                    string options = "1 - Referral link" + System.Environment.NewLine +
                                                        "2 - Referred by" + System.Environment.NewLine +
                                                        "3 - Offset points" + System.Environment.NewLine +
                                                        "4 - Failed logins" + System.Environment.NewLine +
                                                        "5 - Ban user";
                                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Enter:" +
                                                        System.Environment.NewLine + "/edit <USER ID NUMBER> <OPTION NUMBER> get" + System.Environment.NewLine +
                                                        "or" + System.Environment.NewLine + "/edit <USER ID NUMBER> <OPTION NUMBER> set <NEW VALUE>" + System.Environment.NewLine +
                                                        "Replacing <USER ID NUMBER> with the ID number of the user to be edited, replacing <NEW VALUE> with the value to set (true/false for Ban user)(amount to add to user's offset total for Offset Points (-number for subtraction)), and replacing <OPTION NUMBER> one of the following option numbers:" +
                                                        System.Environment.NewLine + System.Environment.NewLine + options);
                                                }
                                            }
                                            else
                                            {
                                                string text = "Error. Password check result was null. User: " + senderID + " Chat: " + chatID;
                                                Logging.addToLog(text);
                                                Console.WriteLine(text);
                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error. Password check result was null.");
                                            }
                                        }
                                    }
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(e.Message.From.Username))
                            {
                                string text = "Username is null or blank. password check cancelled. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(text);
                                Console.WriteLine(text);
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username to use this command.");
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                            }
                        }
                    }
                    else if (messageText.StartsWith("/edit"))
                    {
                        updateInteractedUser(e.Message.From);

                        if (awaitingReply.Contains(chatID + "?" + senderID))
                        {
                            if (!e.Message.From.IsBot && !string.IsNullOrWhiteSpace(e.Message.From.Username))
                            {
                                if (e.Message.Chat.Type == ChatType.Private)
                                {
                                    string message = "Recieved a edit option message. User: " + senderID + " Chat: " + chatID;
                                    Logging.addToLog(message);
                                    Console.WriteLine(message);
                                    if (messageText.Contains(" "))
                                    {
                                        awaitingReply.Remove(chatID + "?" + senderID);
                                        string[] parse = messageText.Split(' ');
                                        //format - /edit <USER ID NUMBER> <OPTION NUMBER> set <NEW VALUE>
                                        if (parse.Length == 4 || parse.Length == 5)
                                        {
                                            string idNum = parse[1];
                                            string option = parse[2];
                                            string getSet = parse[3];
                                            if (option == "1") //Referral link
                                            {
                                                if (getSet.ToLower() == "get")
                                                {
                                                    string text = "Requested option 1 get. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (refLinks.ContainsKey(idNum))
                                                    {
                                                        string result = refLinks[idNum];
                                                        string[] temp = result.Split('?');
                                                        string current = temp[temp.Length - 1];
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Current referral code is " + current);
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User has no referral code");
                                                    }
                                                }
                                                else if (getSet.ToLower() == "set" && parse.Length == 5)
                                                {
                                                    string text = "Requested option 1 set. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (refLinks.ContainsKey(idNum))
                                                    {
                                                        refLinks[idNum] = parse[4];
                                                        bool saved = SaveMethods.SaveRefLinkFile();
                                                        if (saved)
                                                        {
                                                            string text1 = "Referral code has been set and refLinkData was saved. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Referral code has been set to " + refLinks[idNum]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        refLinks.Add(idNum, parse[4]);
                                                        bool saved = SaveMethods.SaveRefLinkFile();
                                                        if (saved)
                                                        {
                                                            string text1 = "Referral code has been set and refLinkData was saved. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Referral code has been set to " + refLinks[idNum]);
                                                        }
                                                    }
                                                }
                                            }
                                            else if (option == "2") //Referred by
                                            {
                                                if (getSet.ToLower() == "get")
                                                {
                                                    string text = "Requested option 2 get. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (referredBy.ContainsKey(idNum))
                                                    {
                                                        string result = referredBy[idNum];
                                                        string[] temp = result.Split('?');
                                                        string current = temp[temp.Length - 1];
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User was referred by ID number " + current);
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User has no referred by value.");
                                                    }
                                                }
                                                else if (getSet.ToLower() == "set" && parse.Length == 5)
                                                {
                                                    string text = "Requested option 2 set. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (referredBy.ContainsKey(idNum))
                                                    {
                                                        referredBy[idNum] = parse[4];
                                                        bool saved = SaveMethods.saveReferredBy();
                                                        if (saved)
                                                        {
                                                            string text1 = "Referred by has been set and referredByData was saved. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Referred by has been set to " + refLinks[idNum]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        referredBy.Add(idNum, parse[4]);
                                                        bool saved = SaveMethods.saveReferredBy();
                                                        if (saved)
                                                        {
                                                            string text1 = "Referred by has been set and refferredByData was saved. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Referred by has been set to " + refLinks[idNum]);
                                                        }
                                                    }
                                                }
                                            }
                                            else if (option == "3") //Offset points
                                            {
                                                if (getSet.ToLower() == "get")
                                                {
                                                    string text = "Requested option 3 get. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (userPointOffset.ContainsKey(idNum))
                                                    {
                                                        int result = userPointOffset[idNum];
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User offset is currently " + result.ToString());
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User has no offset value.");
                                                    }
                                                }
                                                else if (getSet.ToLower() == "set" && parse.Length == 5)
                                                {
                                                    string text = "Requested option 3 set. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (userPointOffset.ContainsKey(idNum))
                                                    {
                                                        try
                                                        {
                                                            string change = "increased";
                                                            int test = Convert.ToInt32(parse[4]);
                                                            if(test < 0)
                                                            {
                                                                change = "decreased";
                                                            }
                                                            int current = userPointOffset[idNum];
                                                            current += test;
                                                            userPointOffset[idNum] = current;
                                                            bool saved = SaveMethods.SaveUserPointOffset();
                                                            if (saved)
                                                            {
                                                                string text1 = "User point offset by has been " + change.ToString().Replace("-", "") + " by " + test + " and userPointOffsetData was saved. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text1);
                                                                Console.WriteLine(text1);

                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User point offset by has been " + change.ToString().Replace("-", "") + " by " + test + " to a total of " + userPointOffset[idNum].ToString());
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            string text1 = "Error! Unable to updated user offset. Value not an integer. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error! Value to set must be an integer");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        try
                                                        {
                                                            int test = Convert.ToInt32(parse[4]);
                                                            userPointOffset.Add(idNum, test);
                                                            bool saved = SaveMethods.SaveUserPointOffset();
                                                            if (saved)
                                                            {
                                                                string text1 = "User point offset by has been set and userPointOffsetData was saved. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text1);
                                                                Console.WriteLine(text1);

                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User point offset by has been set to " + userPointOffset[idNum].ToString());
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            string text1 = "Error! Unable to set user offset. Value not an integer. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error! Value to set must be an integer");
                                                        }
                                                    }
                                                }
                                            }
                                            else if (option == "4") //Failed logins
                                            {
                                                if (getSet.ToLower() == "get")
                                                {
                                                    string text = "Requested option 4 get. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (passwordAttempts.ContainsKey(idNum))
                                                    {
                                                        int result = passwordAttempts[idNum];
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User failed login attempts is currently " + result.ToString());
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User has no failed login attempts.");
                                                    }
                                                }
                                                else if (getSet.ToLower() == "set" && parse.Length == 5)
                                                {
                                                    string text = "Requested option 4 set. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (passwordAttempts.ContainsKey(idNum))
                                                    {
                                                        try
                                                        {
                                                            int test = Convert.ToInt32(parse[4]);
                                                            passwordAttempts[idNum] = test;
                                                            bool saved = SaveMethods.SavePasswordAttempts();
                                                            if (saved)
                                                            {
                                                                string text1 = "User failed login attempts set and attemptsData was saved. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text1);
                                                                Console.WriteLine(text1);

                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User failed login attempts by has been set to " + passwordAttempts[idNum].ToString());
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            string text1 = "Error! Unable to set user failed login attempts. Value not an integer. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error! Value to set must be an integer");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        try
                                                        {
                                                            int test = Convert.ToInt32(parse[4]);
                                                            passwordAttempts.Add(idNum, test);
                                                            bool saved = SaveMethods.SavePasswordAttempts();
                                                            if (saved)
                                                            {
                                                                string text1 = "User failed login attempts set and attemptsData was saved. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text1);
                                                                Console.WriteLine(text1);

                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User failed login attempts by has been set to " + passwordAttempts[idNum].ToString());
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            string text1 = "Error! Unable to set user failed login attempts. Value not an integer. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error! Value to set must be an integer");
                                                        }
                                                    }
                                                }
                                            }
                                            else if (option == "5") //Ban user
                                            {
                                                if (getSet.ToLower() == "get")
                                                {
                                                    string text = "Requested option 5 get. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (userPointOffset.ContainsKey(idNum))
                                                    {
                                                        int result = userPointOffset[idNum];
                                                        if (result == -1000000)
                                                        {
                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User is banned");
                                                        }
                                                        else
                                                        {
                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User is not banned");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User is not banned.");
                                                    }
                                                }
                                                else if (getSet.ToLower() == "set" && parse.Length == 5)
                                                {
                                                    string text = "Requested option 5 set. User: " + senderID + " Chat: " + chatID;
                                                    Logging.addToLog(text);
                                                    Console.WriteLine(text);

                                                    if (userPointOffset.ContainsKey(idNum))
                                                    {
                                                        if (parse[4].ToLower() == "true")
                                                        {
                                                            userPointOffset[idNum] = -1000000;
                                                            bool saved = SaveMethods.SaveUserPointOffset();
                                                            if (saved)
                                                            {
                                                                string text1 = "User has been banned and userPointOffsetData saved. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text1);
                                                                Console.WriteLine(text1);

                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User has been banned.");
                                                            }
                                                        }
                                                        else if (parse[4].ToLower() == "false")
                                                        {
                                                            userPointOffset[idNum] = 0;
                                                            bool saved = SaveMethods.SaveUserPointOffset();
                                                            if (saved)
                                                            {
                                                                string text1 = "User has been unbanned and userPointOffsetData saved. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text1);
                                                                Console.WriteLine(text1);

                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User has been unbanned.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            string text1 = "Error! Unable to set ban user. Value must be true or false. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error! Ban value to set must be true or false");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (parse[4].ToLower() == "true")
                                                        {
                                                            userPointOffset.Add(idNum, -1000000);
                                                            bool saved = SaveMethods.SaveUserPointOffset();
                                                            if (saved)
                                                            {
                                                                string text1 = "User has been banned and userPointOffsetData saved. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text1);
                                                                Console.WriteLine(text1);

                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User has been banned.");
                                                            }
                                                        }
                                                        else if (parse[4].ToLower() == "false")
                                                        {
                                                            userPointOffset.Add(idNum, 0);
                                                            bool saved = SaveMethods.SaveUserPointOffset();
                                                            if (saved)
                                                            {
                                                                string text1 = "User has been unbanned and userPointOffsetData saved. User: " + senderID + " Chat: " + chatID;
                                                                Logging.addToLog(text1);
                                                                Console.WriteLine(text1);

                                                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User has been unbanned.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            string text1 = "Error! Unable to set ban user. Value must be true or false. User: " + senderID + " Chat: " + chatID;
                                                            Logging.addToLog(text1);
                                                            Console.WriteLine(text1);

                                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Error! Ban value to set must be true or false");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(e.Message.From.Username))
                            {
                                string text = "Username is null or blank. getRefLink cancelled. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(text);
                                Console.WriteLine(text);
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username to use this command.");
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                            }
                        }
                    }
                    else if (messageText.StartsWith("/refTotal"))
                    {
                        updateInteractedUser(e.Message.From);

                        string message = "Recieved a referral totals message. User: " + senderID + " Chat: " + chatID;
                        Logging.addToLog(message);
                        Console.WriteLine(message);

                        Dictionary<string, int> byDay = new Dictionary<string, int>();
                        byDay.Clear();
                        foreach(string s in campaignDays)
                        {
                            byDay.Add(s, 0);
                        }
                        int total = 0;

                        foreach (KeyValuePair<int, string> entries in joinedReferrals)
                        {
                            try
                            {
                                if (byDay.ContainsKey(entries.Value))
                                {
                                    int value = byDay[entries.Value];
                                    value++;
                                    byDay[entries.Value] = value;
                                    total++;
                                }
                            }
                            catch { }
                        }

                        string result = "Referred members joined by day:" + System.Environment.NewLine +
                            System.Environment.NewLine;
                        foreach (string key in campaignDays)
                        {
                            result += key + "    " + byDay[key].ToString() + System.Environment.NewLine;
                        }
                        result += "Total    " + total.ToString();

                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: result);
                    }
                    else if (messageText.StartsWith("/myPoints"))
                    {
                        updateInteractedUser(e.Message.From);

                        string message = "Requested user points. User: " + senderID + " Chat: " + chatID;
                        Logging.addToLog(message);
                        Console.WriteLine(message);

                        updatePointTotals();
                        bool foundSender = false;
                        var sorted = pointsByReferrer.OrderByDescending(x => x.Value).ThenBy(x => x.Key);
                        List<string> places = new List<string>();

                        string points = "";
                        int PlaceOfUser = 1;
                        foreach (KeyValuePair<string, int> entries in sorted)
                        {
                            if (entries.Key == senderID)
                            {
                                points = entries.Value.ToString();
                                break;
                            }
                            else
                            {
                                PlaceOfUser++;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(e.Message.From.Username))
                        {
                            if (!string.IsNullOrWhiteSpace(points) && foundSender)
                            {
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "@" + e.Message.From.Username + ", you currently have " + points + " points.");
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "@" + e.Message.From.Username + ", you have no points yet. Share your referral code with others to gain points.");
                            }
                        }
                        else
                        {
                            string text = "Username is null or blank. myPoints cancelled. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(text);
                            Console.WriteLine(text);
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Username not set. Register your Telegram username to use this command.");
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "https://telegram.org/faq#q-what-are-usernames-how-do-i-get-one");
                        }
                    }
                    else if (messageText.StartsWith("/FindMemberID"))
                    {
                        if (e.Message.Chat.Type == ChatType.Private && e.Message.From.IsBot != true)
                        {
                            updateInteractedUser(e.Message.From);

                            string message = "Recieved a find user ID message. User: " + senderID + " Chat: " + chatID;
                            Logging.addToLog(message);
                            Console.WriteLine(message);

                            List<int> userIDNumbers = new List<int>();
                            string[] parse = messageText.Split(' ');
                            if (parse.Length >= 2)
                            {
                                for (int i = 1; i < parse.Length; i++)
                                {
                                    foreach (KeyValuePair<int, Dictionary<string, string>> entry in interactedUser)
                                    {
                                        if (entry.Value.ContainsValue(parse[i]))
                                        {
                                            userIDNumbers.Add(entry.Key);
                                        }
                                    }
                                }

                                if (userIDNumbers.Any())
                                {
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Found " + userIDNumbers.Count.ToString() + " possible matches.");

                                    foreach (int ID in userIDNumbers)
                                    {
                                        ChatMember member = await botClient.GetChatMemberAsync(Config.groupChatIdNumber, ID);
                                        string text = "Found user ID " + ID.ToString();
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);

                                        UserProfilePhotos pfPics = await botClient.GetUserProfilePhotosAsync(ID);

                                        string response = "Found possible match." + System.Environment.NewLine +
                                            "User ID Number: " + ID.ToString() + System.Environment.NewLine +
                                            "User First Name: " + member.User.FirstName + System.Environment.NewLine +
                                            "User Last Name: " + member.User.LastName + System.Environment.NewLine +
                                            "User Username: " + member.User.Username;

                                        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: response);
                                        if (pfPics != null)
                                        {
                                            List<string> pictures = new List<string>();
                                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "User ID Number's profile pictures:");
                                            foreach (PhotoSize[] pics in pfPics.Photos)
                                            {
                                                PhotoSize pic = pics.FirstOrDefault();
                                                pictures.Add(pic.FileId);
                                            }
                                            if (pictures.Any())
                                            {
                                                foreach (string file in pictures)
                                                {
                                                    await botClient.SendPhotoAsync(chatId: e.Message.Chat.Id, photo: file);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "No matches found. Get the member to interact with this bot and try again.");
                                }
                            }
                        }
                    }
                    else if (messageText.StartsWith("/help"))
                    {
                        string message = "Recieved a help message. User: " + senderID + " Chat: " + chatID;
                        Logging.addToLog(message);
                        Console.WriteLine(message);

                        bool admin = false;
                        if (messageText.Contains(" ") && e.Message.Chat.Type == ChatType.Private && !e.Message.From.IsBot)
                        {
                            string[] parse = messageText.Split(" ");
                            if (parse.Length == 2)
                            {
                                string result = checkPassword(parse[1], senderID);
                                if (!string.IsNullOrWhiteSpace(result))
                                {
                                    if (result.StartsWith("wrong"))
                                    {
                                        string[] parse1 = result.Split('_');
                                        string text = "Recieved an incorrect password. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        admin = false;
                                    }
                                    else if (result == "banned")
                                    {
                                        string text = "User banned from login for exceeding failed password attempts. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        admin = false;
                                    }
                                    else if (result == "confirmed")
                                    {
                                        string text = "Password confirmed. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        admin = true;
                                    }
                                }
                                else
                                {
                                    string text = "Error. Password check result was null. User: " + senderID + " Chat: " + chatID;
                                    Logging.addToLog(text);
                                    Console.WriteLine(text);
                                    admin = false;
                                }
                            }
                        }

                        if (admin)
                        {
                            string text = "Command List:" + System.Environment.NewLine + System.Environment.NewLine +
                                "/disableWelcome - (Group Chat Only and Group Admin Only) Disables the welcome message the bot sends when a referred user joins the group." + System.Environment.NewLine +
                                "/editUser - (Private Chat Only) Begins the edit user wizard. Follow the prompts to edit user data." + System.Environment.NewLine +
                                "/enableWelcome - (Group Chat Only and Group Admin Only) Enables the welcome message the bot sends when a referred user joins the group." + System.Environment.NewLine +
                                "/FindMemberID - (Private Chat Only) Use: /FindMemberID <USER_NAME>  Replace <USER_NAME> with the Nickname or @Username to attempt to find the Telegram user ID number." + System.Environment.NewLine +
                                "/getRefLink - (Private Chat Only) Generates your referral code for the program." + System.Environment.NewLine +
                                "/myID - Gets your Telegram user ID." + System.Environment.NewLine +
                                "/myPoints - Gets your current referral point total." + System.Environment.NewLine +
                                "/refTotal - Displays a list of total referred members per day." + System.Environment.NewLine +
                                "/top10 - Displays the names and points of the top 10 ranks in the referral program.";
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: text);
                        }
                        else
                        {
                            string text = "Command List:" + System.Environment.NewLine + System.Environment.NewLine +
                                "/getRefLink - (Private Chat Only) Generates your referral code for the program." + System.Environment.NewLine +
                                "/myID - Gets your Telegram user ID." + System.Environment.NewLine +
                                "/myPoints - Gets your current referral point total." + System.Environment.NewLine +
                                "/refTotal - Displays a list of total referred members per day." + System.Environment.NewLine +
                                "/top10 - Displays the names and points of the top 10 ranks in the referral program.";
                            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: text);
                        }
                    }
                    else
                    {
                        if (e.Message.Chat.Id == Config.groupChatIdNumber && !e.Message.Text.Contains('/') && referredBy.ContainsKey(senderID))
                        {
                            try
                            {
                                int textLength = e.Message.Text.Length;
                                if (textLength >= Config.thresholdForMessagePoint)
                                {
                                    updateInteractedUser(e.Message.From);

                                    Dictionary<string, int> currentSender = new Dictionary<string, int>();
                                    if (!userActivity.ContainsKey(senderID))
                                    {
                                        currentSender = createUserActivityDictionary(senderID);
                                        if (currentSender == null)
                                        {
                                            string text = "currentSender new Dictionary is null. User: " + senderID + " Chat: " + chatID;
                                            Logging.addToLog(text);
                                            Console.WriteLine(text);
                                        }
                                    }
                                    else
                                    {
                                        currentSender = userActivity[senderID];
                                    }

                                    string today = DateTime.UtcNow.ToString("MM/dd/yyyy");
                                    if (currentSender.ContainsKey(today))
                                    {
                                        int current = currentSender[today];
                                        current++;
                                        currentSender[today] = current;
                                    }

                                    if (userActivity.ContainsKey(senderID) && currentSender != null)
                                    {
                                        userActivity[senderID] = currentSender;
                                    }
                                    else if (currentSender != null)
                                    {
                                        userActivity.Add(senderID, currentSender);
                                    }

                                    bool saved = SaveMethods.saveUserActivity();
                                    if (saved)
                                    {
                                        string text = "userActivity was saved. User: " + senderID + " Chat: " + chatID;
                                        Logging.addToLog(text);
                                        Console.WriteLine(text);
                                        updatePointTotals();
                                    }
                                }
                            }
                            catch
                            {
                                string text = "Error updating user activity. User: " + senderID + " Chat: " + chatID;
                                Logging.addToLog(text);
                                Console.WriteLine(text);
                            }
                        }
                    }
                }
            }
        }

        public static void updatePointTotals()
        {
            if (userActivity.Any())
            {
                string message = "Beginning to calculate point totals";
                Logging.addToLog(message);
                Console.WriteLine(message);

                //calculate points from referred user activity per day
                pointTotals.Clear();
                foreach (KeyValuePair<string, Dictionary<string, int>> entry in userActivity)
                {
                    int total = 0;
                    string userID = entry.Key;
                    Dictionary<string, int> temp = entry.Value;
                    foreach(int points in temp.Values)
                    {
                        int checkMax = points;
                        if (checkMax > Config.maxNumberOfPointsPerDayPerUser)
                        {
                            checkMax = Config.maxNumberOfPointsPerDayPerUser;
                        }

                        total = total + checkMax;
                    }
                    pointTotals.Add(userID, total);
                }

                //find referrer from referree and add points from activity to referrer's score
                pointsByReferrer.Clear();
                foreach(KeyValuePair<string, int> data in pointTotals)
                {
                    int total = data.Value;
                    string referringUser = "";
                    if (referredBy.ContainsKey(data.Key))
                    {
                        referringUser = referredBy[data.Key];
                        if (pointsByReferrer.ContainsKey(referringUser))
                        {
                            int temp = pointsByReferrer[referringUser];
                            total = total + temp;
                            pointsByReferrer[referringUser] = total;
                        }
                        else
                        {
                            pointsByReferrer.Add(referringUser, total);
                        }
                    }
                }

                //add points for users joining chat
                foreach(KeyValuePair<string, int> entry in referralPoints)
                {
                    if (pointsByReferrer.ContainsKey(entry.Key))
                    {
                        int total = pointsByReferrer[entry.Key];
                        total = total + entry.Value;
                        pointsByReferrer[entry.Key] = total;
                    }
                    else
                    {
                        pointsByReferrer.Add(entry.Key, entry.Value);
                    }
                }

                //apply any point offset
                foreach(KeyValuePair<string, int> offsets in userPointOffset)
                {
                    if (pointsByReferrer.ContainsKey(offsets.Key))
                    {
                        int total = 0;
                        if(offsets.Value == -1000000)
                        {
                            total = -1000000;
                        }
                        else
                        {
                                int temp = pointsByReferrer[offsets.Key];
                                total = temp + offsets.Value;
                                pointsByReferrer[offsets.Key] = total;
                        }
                    }
                    else
                    {
                        pointsByReferrer.Add(offsets.Key, offsets.Value);
                    }
                }

                //removed banned users
                List<string> banned = new List<string>();
                foreach(KeyValuePair<string, int> entry in userPointOffset)
                {
                    if(entry.Value == -1000000)
                    {
                        banned.Add(entry.Key);
                    }
                }
                if (banned.Any())
                {
                    foreach(string s in banned)
                    {
                        if (pointsByReferrer.ContainsKey(s))
                        {
                            pointsByReferrer.Remove(s);
                        }
                    }
                }

                if (pointsByReferrer.Any())
                {
                    bool saved = SaveMethods.savePointsByReferrer();
                    if (saved)
                    {
                        string text = "pointsByReferrer was saved.";
                        Logging.addToLog(text);
                        Console.WriteLine(text);
                    }
                }
            }
        }

        public static string checkPassword(string text, string user)
        {
            string message = "Beginning check password.";
            Logging.addToLog(message);
            Console.WriteLine(message);

            try
            {
                int attempts = 0;
                if (passwordAttempts.ContainsKey(user))
                {
                    attempts = passwordAttempts[user];
                }

                if (attempts < 11)
                {
                    if (text != Config.apw)
                    {
                        attempts++;

                        if (passwordAttempts.ContainsKey(user))
                        {
                            passwordAttempts[user] = attempts;
                        }
                        else
                        {
                            passwordAttempts.Add(user, attempts);
                        }
                        bool saved = SaveMethods.SavePasswordAttempts();
                        if (saved)
                        {
                            string text1 = "passwordAttempts was saved.";
                            Logging.addToLog(text1);
                            Console.WriteLine(text1);
                        }
                        else
                        {
                            string text1 = "Could not save passwordAttempts.";
                            Logging.addToLog(text1);
                            Console.WriteLine(text1);
                        }


                        if (attempts < 10)
                        {
                            return "wrong_" + attempts.ToString();
                        }
                        else if (attempts == 11)
                        {
                            return "banned";
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (text == Config.apw)
                    {
                        return "confirmed";
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}