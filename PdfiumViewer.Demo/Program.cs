using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using YamlDotNet.Serialization;

namespace PdfSearcher
{
    [YamlSerializable]
    public class Config
    {

        [YamlMember]
        public string PDFDatabasePath;

    }
    public static class Program
    {
        public const string DefaultPDFDatabasePath = "C:\\Working\\PdfDatabase";

        public const string ConfigFile = "Config.yml";

        public static Config GlobalConfig = new Config();

        public static void LoadGlobalConfig()
        {
            var ConfigPath = Path.Combine(Environment.CurrentDirectory, ConfigFile);

            if (File.Exists(ConfigPath))
            {
                using (var reader = new StreamReader(ConfigPath))
                {
                    var des = new Deserializer();
                    GlobalConfig = des.Deserialize<Config>(reader);
                    GlobalConfig = GlobalConfig ?? new Config();
                }
            }
        }
        public static void SaveGlobalConfig()
        {
            if (GlobalConfig != null)
            {
                var ConfigPath = Path.Combine(Environment.CurrentDirectory, ConfigFile);
                using (var writer = new StreamWriter(ConfigPath))
                {
                    var ser = new Serializer();
                    ser.Serialize(writer,GlobalConfig);
                }
            }
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            LoadGlobalConfig();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
