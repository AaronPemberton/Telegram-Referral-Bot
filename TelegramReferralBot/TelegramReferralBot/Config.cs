using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramReferralBot
{
    /* SETUP INFO
     * 
     * Use BotFather /newbot to create new bot following wizard
     * 
     * BotFather /mybots > Bot Settings > Allow Groups? > Turn groups on
     * BotFather /mybots > Bot Settings > Group Privacy > Turn off
     * 
     * replace <YOUR_BOT_ACCESS_TOKEN> with your bot access token provided from BotFather
     * 
     * add bot to group with the following link (replace <BOT_USER_NAME> with the bot user name): https://telegram.me/<BOT_USER_NAME>?startgroup=added2group
     * 
     * Set bot permisions to Admin if not already
     * 
     * replace </path/to/output/folder> with your output folder path
     * 
     * replace https://telegram.me/<GROUP_NAME> with your group link
     * 
     * replave <BOT_NAME> with your bot's username
     * 
     * replace <PASSWORD> with a password for editing
     * 
     * replace <START_DATE> with the date the referral campaign will begin. Month/Day/Year for example 5/21/2020.
     * 
     * Change the value of numberOfDays with the integer number of days the referral campaign is to last. 
     * 
     * Change the value of maxNumberOfPointsPerDayPerUser to an integer number for max points per day. 
     * Points are obtained by referring new members and group chat activity of those new members. 
     * Every post by the referred user above thresholdForMessagePoint gives the reffering user 1 point, up to maxNumberOfPointsPerDayPerUser. 
     * In order for a message to be accepted for a point reward, the message text length must be longer than thresholdForMessagePoint. 
     * This prevents some spam, nuisance posts, and unengaging posts such as a simpele "Hi guys!" from being counted. 
     * This can also be set to 0 to disable the points from user posts feature. The maxNumberOfPointsPerDayPerUser applies for each referred user. 
     * For example: Bob referred Jane, Ginger and Katie. maxNumberOfPointsPerDayPerUser is set to 5. 
     * Jane sent 3 messages in the chat today that exceeded thresholdForMessagePoint. Ginger sent 15 and Katie sent none.
     * Bob would have 1 point for each when they joined the group with his referral code.
     * He would get an additional 3 points today from Jane, 5 from Ginger (the max due to maxNumberOfPointsPerDayPerUser), and 0 from Katie. 
     * Giving him 8 points for today's user activity and 3 for referals for a total of 11 points.
     * 
     * Change the value of thresholdForMessagePoint to the number of characters required in the message in order to be awarded a point.
     * 
     * set user offset value (through /editUser in private chat) to -1000000 if user is banned from the contest.
     */

    public static class Config
    {
        public readonly static string BotAccessToken = "<YOUR_BOT_ACCESS_TOKEN>";

        public readonly static string outputFilePath = "</path/to/output/folder>";

        public readonly static string linkToGroup = @"https://telegram.me/<GROUP_NAME>";

        public readonly static string linkToBot = @"https://telegram.me/<BOT_NAME>";

        public readonly static string apw = "<PASSWORD>";

        public readonly static string startDate = "<START_DATE>";

        public readonly static int numberOfDays = 7; //length of referral campaign in days

        public readonly static int maxNumberOfPointsPerDayPerUser = 10;

        public readonly static int thresholdForMessagePoint = 15;

        //This value is filled out through code upon recieving the added2group payload from the invite link for the bot
        public static long groupChatIdNumber = 0;
    }
}
