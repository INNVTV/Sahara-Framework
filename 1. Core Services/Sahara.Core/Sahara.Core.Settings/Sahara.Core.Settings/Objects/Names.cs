using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Objects
{
    public static class Names
    {

        #region RESERVED NAMES

        public static readonly ReadOnlyCollection<string> ReservedObjectNames =
        new ReadOnlyCollection<string>(new[]
        {
            #region Reserved ObjectNames
            
            //a
            
            //"",

            //b


            //c
            

            //d


            //e

            
            //f


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


            //q

            //r


            //s


            //t


            //u


            //v


            
            //w


            //x

            //y

            //z


            

            #endregion

            #region Profanities Filter

            //a

            "",

            //b

            //c

            //"cunt",

            //d

            //"dick",

            //e

            //f

            //"fuck",

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

            //"pussy",

            //q

            //r

            //s

            //"shit",

            //t

            //"tits",

            //u

            //v

            //w

            //x

            //y

            //z

            #endregion

        });

        public static readonly ReadOnlyCollection<string> ReservedPropertyNames = new ReadOnlyCollection<string>(new[]
{
            #region Reserved PropertyNames
            
            //a
            "admin",
            "account",
            "accountname",
            "accountnamekey",
            
            //"",

            //b


            //c
            "category",
            "categorykey",
            "categorization",
            "categorizations",

            //d
            "dateCreated",
            "documenttype",
            "default",
            //e

            
            //f
            "fullyqualifiedname",
            "filepath",
            //g

            //h

            //i
            "id",
            "image",
            "images",

            //j

            //k

            //l
            "locationpath",
            "locations",
            //m
            "metadata",
            "locationmetadata",

            //n
            "name",
            "namekey",
            "null",
            //o
            "orderid",
            "order",
            "orderby",
            "ordering",

            //p
            "properties",
            "property",
            "predefined",
            "productid",
            

            //q

            //r


            //s
            "selflink",
            "search",

            "sort",
            "sorting",
            "sort-by",
            "sortby",

            "subcategory",
            "subcategorykey",
            "subcategorization",

            "subsubcategory",
            "subsubcategorykey",
            "subsubcategorization",

            "subsubsubcategory",
            "subsubsubcategorykey",
            "subsubsubcategorization",

            "swatches",

            //t
            "tags",
            "tag",
            "thumbnails",
            "thumbnail",
            "title",

            //u


            //v
            "visible",

            
            //w


            //x

            //y

            //z


            

            #endregion

            #region Profanities Filter

            //a

            "",

    //b

    //c

    //"cunt",

    //d

    //"dick",

    //e

    //f

    //"fuck",

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

    //"pussy",

    //q

    //r

    //s

    //"shit",

    //t

    //"tits",

    //u

    //v

    //w

    //x

    //y

    //z

    #endregion

});

        public static readonly ReadOnlyCollection<string> ReservedImageGroupNames = new ReadOnlyCollection<string>(new[]
{
            #region Reserved PropertyNames
            
            //a
            "admin",
            "account",
            
            //"",

            //b


            //c
            "category",
            "categorization",

            //d
            "default",
            //e

            //g

            //h

            //i
            "image",
            "images",

            //j

            //k

            //l

            //m


            //n
            "name",
            "null",
            //o

            //p

            //q

            //r


            //s


            "subcategory",
            "subcategorization",

            "subsubcategory",
            "subsubcategorization",

            "subsubsubcategory",
            "subsubsubcategorization",

            //t
            "thumbnail",

            //u


            //v
 

            
            //w


            //x

            //y

            //z


            

            #endregion

            #region Profanities Filter

            //a

            "",

    //b

    //c

    //"cunt",

    //d

    //"dick",

    //e

    //f

    //"fuck",

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

    //"pussy",

    //q

    //r

    //s

    //"shit",

    //t

    //"tits",

    //u

    //v

    //w

    //x

    //y

    //z

    #endregion

});

        public static readonly ReadOnlyCollection<string> ReservedImageFormatNames = new ReadOnlyCollection<string>(new[]
{
            #region Reserved PropertyNames
            
            //a
            
            //"",

            //b


            //c


            //d

            //e

            //g

            //h

            //i
            "image",
            "images",

            //j

            //k

            //l

            //m


            //n
            "name",
            "null",
            //o

            //p

            //q

            //r


            //s


            //t

            //u


            //v
 

            
            //w


            //x

            //y

            //z


            

            #endregion

            #region Profanities Filter

            //a

            "",

    //b

    //c

    //"cunt",

    //d

    //"dick",

    //e

    //f

    //"fuck",

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

    //"pussy",

    //q

    //r

    //s

    //"shit",

    //t

    //"tits",

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


        public static int ObjectNameMinimumLength = 1;
        public static int ObjectNameMaximunLength = 110;

        public static int TagNameMinimumLength = 1;
        public static int TagNameMaximunLength = 35;

        public static int PropertyNameMinimumLength = 1;
        public static int PropertyNameMaximunLength = 35;

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
