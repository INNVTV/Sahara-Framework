using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Accounts
{
    public static class Registration
    {

        #region RESERVED ACCOUNT/SYSTEM NAMES

        public static readonly ReadOnlyCollection<string> ReservedAccountNames =
        new ReadOnlyCollection<string>(new[]
        {
            #region Reserved Subdomains/AccountNames
            
            //a
            
            "about",
            "abuse",
            "account",
            "accounts",
            "accountname",
            "accountnamekey",
            "accountid",
            "admin",
            "administrator",
            "administrators",           
            "api",
            "atom",
            "authentication",
            "authorization",
            "auth",

            //b

            "b2b",
            "b2c",
            "blog",
            "business",

            //c
            
            "calendar",
            "cdn",
            "collaborate",
            "cloud",
            "catalog",
            "chat",
            "code",
            "comment",
            "contact",
            "css",
            "cms",
            
            //d

            "dns",
            "developer",
            "developers",
            "develop",
            "data",
            "dev",
            "download",
            
            //e

            "email",
            "enterprise",
            "export",
            "explore",
            
            //f

            "featured",
            "features",
            "ftp",
            "feed",
            "feeds",

            //g
            "go",

            //h
            "help",

            //i
            
            "image",
            "images",
            "imaging",
            "imap",
            "img",
            "imgs",
            "inbox",
            "inventory",
            "inventories",
            "inventoryhawk",
            "import",

            //j

            //k

            //l

            "login",
            "logon",
            "logout",
            "log",
            "logs",
            "lookbook",
            "lookbooks",

            //m

            "mail",
            "marketing",
            "members",
            "mobilemail",
            "mobile",
            "manage",
            "map",
            "maps",
            "media",

            //n
            "news",

            //o

            //p

            "platform",
            "pop",
            "pop3",
            "private",
            "privacy",
            "public",
            "press",
            "print",
            "portal",
            "post",
            "profile",
            "root",

            //q

            //r

            "recovery",
            "recover",
            "register",
            "registration",
            "reset",

            //s
            "search",
            "subscribe",
            "sub",
            "subdomain",
            "secure",
            "secret",
            "smtp",
            "support",
            "ssl",
            "sync",

            //t

            "tablet",
            "tcp",
            "tcpip",
            "test",
            "tests",
            "testing",
            "transfer",
            "team",
            "teams",


            //u

            "user",
            "users",
            "usr",

            //v

            "verify",
            "verification",
            
            //w

            "webhook",
            "webhooks",
            "wiki",
            "www",
            "webmaster",
            "master",
            "http",
            "https",
            "cloudflare",


            //x
            "xml",

            //y

            //z


            

            #endregion

            #region Profanities Filter

            //a
            "asshole",

            //b

            //c

            "cunt",
            "cock",

            //d

            "dick",

            //e

            //f

            "fuck",

            //g

            //h

            //i

            //j

            //k

            //l

            //m

            //n

            //o

            //p

            "pussy",

            //q

            //r

            //s

            "shit",

            //t

            "tits",

            //u

            //v

            //w

            //x

            //y

            //z

            #endregion

        });

        #endregion

        #region BASE SETTINGS

        #region CONFIGURATIONS

        
        public static int AccountNameMinimumLength = 3;
        public static int AccountNameMaximunLength = 40;

        public static int UserFirstNameMinimumLength = 1;
        public static int UserFirstNameNameMaximunLength = 20;

        public static int UserLastNameMinimumLength = 1;
        public static int UserLastNameNameMaximunLength = 22;

        public static int PasswordMinimumLength = 8;

        #endregion


        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Environment Settings

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    break;

                #endregion


                #region Stage

                case "stage":


                    break;

                #endregion


                #region Local/Debug

                case "debug":


                    break;

                case "local":


                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }
}
