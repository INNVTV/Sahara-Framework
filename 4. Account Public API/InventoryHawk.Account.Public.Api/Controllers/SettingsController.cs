using InventoryHawk.Account.Public.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Routing;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    [RoutePrefix("Settings")]
    public class SettingsController : ApiController
    {
        #region Fiddler Settings

        /*
         * [POST] url/accountname
         *
         * Content-type: application/x-www-form-urlencoded
         * Authorization: Basic 
         * 
         * 
         * REQUEST BODY:
         * AccountName=Account Name
         * 
         */

        #endregion

        //Get ALL settings "/settings"
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            //Get the subdomain (if exists) for the api call
            string subdomain = Common.GetSubDomain(Request.RequestUri);

            var theme = new ThemeModel();

            //Select a random theme from 1-5
            Random random = new Random();

            int rand = random.Next(1, 6);

            switch (rand)
            {
                case 1:
                    theme = new ThemeModel
                    {
                        Name = "Purple",
                        Colors = new ThemeColorsModel
                        {
                            Background = "B604BF",
                            BackgroundGradientTop = "B604BF",
                            BackgroundGradientBottom = "D289D6",

                            Shadow = "6D0073",
                            Highlight = "EEC5F0",

                            Foreground = "966799",
                            Overlay = "5F467A",
                            Trim = "0010C4"
                        },
                        Font = new ThemeFontModel
                        {
                            Name = "poiret",
                        }
                    };
                    break;
                case 2:
                    theme = new ThemeModel
                    {
                        Name = "Tan",
                        Colors = new ThemeColorsModel
                        {
                            Background = "D9C2BA",
                            BackgroundGradientTop = "D9C2BA",
                            BackgroundGradientBottom = "91817C",

                            Shadow = "997B71",
                            Highlight = "FAD1C3",

                            Foreground = "C97559",
                            Overlay = "544D4A",
                            Trim = "4D2416"
                        },
                        Font = new ThemeFontModel
                        {
                            Name = "segoe",
                        }
                    };
                    break;
                case 3:
                    theme = new ThemeModel
                    {
                        Name = "Charcoal",
                        Colors = new ThemeColorsModel
                        {
                            Background = "000000",
                            BackgroundGradientTop = "000000",
                            BackgroundGradientBottom = "4A4A4A",

                            Shadow = "A8A8A8",
                            Highlight = "D9D9D9",

                            Foreground = "FFFFFF",
                            Overlay = "828282",
                            Trim = "4D4D4D"
                        },
                        Font = new ThemeFontModel
                        {
                            Name = "segoe",
                        }
                    };
                    break;
                case 4:
                    theme = new ThemeModel
                    {
                        Name = "Cyan",
                        Colors = new ThemeColorsModel
                        {
                            Background = "9DD9ED",
                            BackgroundGradientTop = "9DD9ED",
                            BackgroundGradientBottom = "4F94AB",

                            Shadow = "1D4E5E",
                            Highlight = "CFF3FF",

                            Foreground = "1D4E5E",
                            Overlay = "A18F28",
                            Trim = "FF8800"
                        },
                        Font = new ThemeFontModel
                        {
                            Name = "segoe",
                        }
                    };
                    break;

                case 5:
                    theme = new ThemeModel
                    {
                        Name = "White",
                        Colors = new ThemeColorsModel
                        {
                            Background = "FFFFFF",
                            BackgroundGradientTop = "FFFFFF",
                            BackgroundGradientBottom = "DEDEDE",

                            Shadow = "858585",
                            Highlight = "000000",

                            Foreground = "000000",
                            Overlay = "3D3D3D",
                            Trim = "5E5E5E"
                        },
                        Font = new ThemeFontModel
                        {
                            Name = "segoe",
                        }
                    };
                    break;
            }



            var settings = new SettingsModel
            {
                Display = new SettingsDisplayModel
                {
                    Lists = new SettingsDisplayListsModel
                    {
                        ShowPrice = true,
                        ShowTitle = true,
                        ShowSubTitle = true,
                        ShowDescription = true
                    },
                    Details = new SettingsDisplayDetailsModel
                    {
                        ShowPrice = true
                    }

                },

                Theme = theme
            };

            //var json = LowercaseJsonSerializer.SerializeObject(settings);

            httpResponse = Request.CreateResponse(HttpStatusCode.OK, settings);

            return httpResponse;
        }

        //Get partial settings "/settings/{name}"
        /*
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("Theme")]
        [HttpGet]
        public HttpResponseMessage Theme()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            var theme = new ThemeModel
            {
                ThemeName = "Tiffany",
                Colors = new ThemeColorsModel
                {
                    Background = "90C3D4",
                    Foreground = "EDE26D",
                    Trim = "2E2E2E",
                    Overlay = "4EBADE"
                }
            };

            //var json = LowercaseJsonSerializer.SerializeObject(theme);

            httpResponse = Request.CreateResponse(HttpStatusCode.OK, theme);

            return httpResponse;
        }*/
    }
}