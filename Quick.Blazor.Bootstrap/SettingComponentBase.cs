using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap.Utils;
using System;

namespace Quick.Blazor.Bootstrap
{
    public abstract class SettingComponentBase<T> : ComponentBase
                where T : new()
    {
        public static string ConfigFileFolder { get; private set; }
        public static T GlobalConfig { get; private set; }

        public T Config { get; private set; }

        public static void Init(string configFileFolder)
        {
            ConfigFileFolder = configFileFolder;
            GlobalConfig = ConfigFileUtils.Load<T>(folder: ConfigFileFolder);
            if (GlobalConfig == null)
                GlobalConfig = new T();
        }

        protected override void OnInitialized()
        {
            Config = ConfigFileUtils.Load<T>(folder: ConfigFileFolder);
            if (Config == null)
                Config = new T();
        }

        protected void save()
        {
            ConfigFileUtils.Save(Config, folder: ConfigFileFolder);
            GlobalConfig = Config;
        }
    }
}
