using Sahara.Core.Accounts.Themes.Models;
using Sahara.Core.Accounts.Themes.TableEntities;
using Sahara.Core.Common.ResponseTypes;
using System;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Newtonsoft.Json;

namespace Sahara.Core.Accounts.Themes.Public
{
    public static class ThemesManager
    {
        public static DataAccessResponseType CreateTheme(ThemeModel theme)
        {
            var themeTableEntity = new ThemeTableEntity(theme.Name);


            themeTableEntity.Font = theme.Font;

            themeTableEntity.ColorBackground = theme.Colors.Background;
            themeTableEntity.ColorBackgroundGradientTop = theme.Colors.BackgroundGradianetTop;
            themeTableEntity.ColorBackgroundGradientBottom = theme.Colors.BackgroundGradientBottom;
            themeTableEntity.ColorForeground = theme.Colors.Foreground;
            themeTableEntity.ColorHighlight = theme.Colors.Highlight;
            themeTableEntity.ColorOverlay = theme.Colors.Overlay;
            themeTableEntity.ColorShadow = theme.Colors.Shadow;
            themeTableEntity.ColorTrim = theme.Colors.Trim;
            
            return Internal.ThemesTableManager.StoreTheme(themeTableEntity);
        }

        public static List<ThemeModel> GetThemes()
        {
            List<ThemeModel> themes = null;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ThemesHash.Fields.ThemesList();

            try
            {
                var redisValue = cache.HashGet(ThemesHash.Key, redisHashField);
                if (redisValue.HasValue)
                {
                    themes = JsonConvert.DeserializeObject<List<ThemeModel>>(redisValue);
                }
            }
            catch
            {

            }
        
            if(themes == null)
            {
                var themeEntities = Internal.ThemesTableManager.GetThemes().ToList();

                themes = new List<ThemeModel>();

                foreach (ThemeTableEntity themeEntity in themeEntities)
                {
                    themes.Add(new ThemeModel
                    {
                        Name = themeEntity.ThemeName,
                        NameKey = themeEntity.ThemeNameKey,

                        Font = themeEntity.Font,

                        Colors = new ThemeColorsModel
                        {
                            Background = themeEntity.ColorBackground,
                            BackgroundGradianetTop = themeEntity.ColorBackgroundGradientTop,
                            BackgroundGradientBottom = themeEntity.ColorBackgroundGradientBottom,
                            Shadow = themeEntity.ColorShadow,
                            Highlight = themeEntity.ColorHighlight,
                            Overlay = themeEntity.ColorOverlay,
                            Trim = themeEntity.ColorTrim,
                            Foreground = themeEntity.ColorForeground
                        }
                    });
                }

                //Sort Alphabetically
                //themes.Sort((ThemeModel));
                themes = themes.OrderBy(o => o.NameKey).ToList();

                //Store into Cache
                try
                {
                    cache.HashSet(ThemesHash.Key, redisHashField, JsonConvert.SerializeObject(themes), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }


            }


            return themes;
        }

    }
}
