# Telegram-Referral-Bot
A Telegram Bot to track public group referrals with a built in point system.

ABOUT:

This is a Telegram Chatbot for a referral program to a group chat. It has a built in point system that also awards extra points to the referrer when referrees are active in the group chat. Members can generate a link to the ReferralBot which contains the member's unique referral code. When a new user clicks the link to join a chat with the bot, the referral is logged and a user is sent a message and link to join the official group. Once the user joins the group, the referring member is awarded the point.

This bot is created for there to only be one group to watch for new members. If you need to referr users to multiple groups, you will need multiple instances of this bot.

Important! Admin password is not stored encrypted in this base design. For a permanent solution, reconfigure for secure storage method.


BOT CONFIGURATION:

Enter a private chat with @BotFather and send the command /newbot 

Follow the prompts to create a new bot.

Once the bot is created, set the bot to allow groups and to turn off group privacy.
	@BotFather /mybots > Bot Settings > Allow Groups? > Turn groups on
	@BotFather /mybots > Bot Settings > Group Privacy > Turn off

Setup any user visible bot commands.
	@BotFather /mybots > Edit Bot > Edit Commands > <command> - <description>

In Config.cs:
Replace <YOUR_BOT_ACCESS_TOKEN> with your bot access token provided from BotFather.

Replace </path/to/output/folder> with your output folder path. Example: (Windows) C:\MyBotFolder - (Linux) home/Output

Replace https://telegram.me/<GROUP_NAME> with your group link.

Replace <BOT_NAME> with your bot's username.

Replace <PASSWORD> with a password for editing.

Replace <START_DATE> with the date the referral campaign will begin. Month/Day/Year for example 5/21/2020.

Change the value of numberOfDays with the integer number of days the referral campaign is to last.

Change the value of maxNumberOfPointsPerDayPerUser to an integer number for max points per day. Points are obtained by referring new members and group chat activity of those new members. Every post by the referred user above thresholdForMessagePoint gives the reffering user 1 point, up to maxNumberOfPointsPerDayPerUser. In order for a message to be accepted for a point reward, the message text length must be longer than thresholdForMessagePoint. This prevents some spam, nuisance posts, and unengaging posts such as a simpele "Hi guys!" from being counted. This can also be set to 0 to disable the points from user posts feature. The maxNumberOfPointsPerDayPerUser applies for each referred user. 
	For example: Bob referred Jane, Ginger and Katie. maxNumberOfPointsPerDayPerUser is set to 5. Jane sent 3 messages in the chat today that exceeded thresholdForMessagePoint. Ginger sent 15 and Katie sent none. Bob would have 1 point for each when they joined the group with his referral code. He would get an additional 3 points today from Jane, 5 from Ginger (the max due to maxNumberOfPointsPerDayPerUser), and 0 from Katie. Giving him 8 points for today's user activity and 3 for referals for a total of 11 points. 

In order for a message to be accepted for a point reward, the message text length must be longer than thresholdForMessagePoint. This prevents some spam, nuisance posts, and unengaging posts such as a simpele "Hi guys!" from being counted. Change the value of thresholdForMessagePoint to the number of characters required in the message in order to be awarded a point.

Build/Publish bot.

Ubuntu 18.04 VPS install:
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo add-apt-repository universe
sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1

Clone repository: git clone https://github.com/AaronPemberton/Telegram-Referral-Bot.git

Start the bot on the VPS with the command: dotnet <path/to/bot.dll>
For example: dotnet TelegramReferralBot/TelegramReferralBot/bin/Release/netcoreapp2.1/publish/TelegramReferralBot.dll

If sucessfully started, bot will respond in the console with its user ID and name.

Add bot to group with the following link (replace <BOT_USER_NAME> with the bot user name): https://telegram.me/<BOT_USER_NAME>?startgroup=added2group

If sucessfully started, bot will respond with a greeting.

Set bot permisions to Admin if not already.


BOT COMMANDS:

	Everyone:
		/help - Sends a list of available commands.
		/getRefLink - (Private Chat Only) Generates your referral code for the program.
		/myID - Gets your Telegram user ID.
		/myPoints - Gets your current referral point total.
		/refTotal - Displays a list of total referred members per day.
		/top10 - Displays the names and points of the top 10 ranks in the referral program.

	Admins:
		/help - Sends a list of available commands.
		/help <PASSWORD> - (Private Chat Only. Replace <PASSWORD> with the admin password) Sends the full list of commands.
		/disableWelcome - (Group Chat Only and Group Admin Only) Disables the welcome message the bot sends when a referred user joins the group.
		/editUser - (Private Chat Only) Begins the edit user wizard. Follow the prompts to edit user data.
		/enableWelcome - (Group Chat Only and Group Admin Only) Enables the welcome message the bot sends when a referred user joins the group.
		/FindMemberID - (Private Chat Only) Use: /FindMemberID <USER_NAME>  Replace <USER_NAME> with the Nickname or @Username to attempt to find the Telegram user ID number.
		/getRefLink - (Private Chat Only) Generates your referral code for the program.
		/myID - Gets your Telegram user ID.
		/myPoints - Gets your current referral point total.
		/refTotal - Displays a list of total referred members per day.
		/top10 - Displays the names and points of the top 10 ranks in the referral program.

ADDITIONAL INFORMATION:
This bot is design to only track one public group at a time. You will need to create seperate instances of this bot to link more than one group.

Log files are saved to a Log folder inside of the outputFilePath.

First log file of each day contains a data dump from all databases. This is to help in reconstructing the databases if needed.

The offset feature, set by /editUser wizard, can be used to adjust a member's point total, if required. It can also be used to award bonus points for completing additional tasks, if desired. Set this value to -1000000 to ban the user from the referral campaign. This can also be achieved by Option 5 - Ban user in the /editUser wizard.

Failed logins, accessed in the /editUser wizard, is the number of attempts the user tried password protected commands with an incorrect password. After 10 failed attempts, the user is banned from commands that require a password until their Failed logins value is set to 0 by another admin with the password.

When a new member joins the group, if they joined with a referral code, the bot will respond in the group with a welcome message. This serves as a convienent way of seeing who was referred by whom and when new referrals join. This feature can be disabled by any group admin with the command /disableWelcome in the group. Or enabled by any group admin with /enableWelcome in the group.

Some commands require a user ID number of the member, such as the /editUser wizard. A user's ID number can be obtain two ways with this bot. 1) Have the user send the /myID command. 2) If the user has interacted with the bot, the bot maintains a log of users. Send the /FindMemberId <USER_NAME> command in a private chat with the bot, replacing <USER_NAME> with the nickname or @username of the member you need the ID number for. If any matches are found in the database, the bot will respond with info on each potential match for you to verify which user is the correct one.
