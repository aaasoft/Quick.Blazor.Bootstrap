using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.Blazor.Bootstrap.Utils
{
    internal class ConfigFileUtils
    {
        private static string getTypeFilePath(Type type, string fileSuffix, string folder)
        {
            var fileName = type.FullName;
            if (!string.IsNullOrEmpty(fileSuffix))
                fileName += "." + fileSuffix;
            fileName += ".json";
            if (string.IsNullOrEmpty(folder))
                return fileName;
            return Path.Combine(folder, fileName);
        }

        public static string GetTypeFilePath<T>(string fileSuffix, string folder = null)
        {
            return getTypeFilePath(typeof(T), fileSuffix, folder);
        }

        public static T Load<T>(string fileSuffix = null, string folder = null)
        {
            try
            {
                var file = GetTypeFilePath<T>(fileSuffix, folder);
                if (!File.Exists(file))
                    return default(T);
                var content = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                return default(T);
            }
        }

        public static void Save(object configObj, string fileSuffix = null, string folder = null)
        {
            if (configObj == null)
                return;

            var file = getTypeFilePath(configObj.GetType(), fileSuffix, folder);
            if (File.Exists(file))
                File.Delete(file);
            var content = JsonConvert.SerializeObject(configObj);
            File.WriteAllText(file, content, Encoding.UTF8);
        }
    }
}
