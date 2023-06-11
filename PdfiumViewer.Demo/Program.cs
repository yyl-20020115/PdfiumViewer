using System;
using System.IO;
using System.Windows.Forms;
using YamlDotNet.Serialization;

namespace PdfSearcher
{
    [YamlSerializable]
    public class Configure
    {

        [YamlMember]
        public string PDFDatabasePath;

    }
    public static class Program
    {
        public const string DefaultPDFDatabasePath = "C:\\Working\\PdfDatabase";

        public const string ConfigFile = "Config.yml";

        public static Configure GlobalConfigure = new Configure();

        public static void LoadGlobalConfigure()
        {
            var ConfigPath = Path.Combine(Environment.CurrentDirectory, ConfigFile);

            if (File.Exists(ConfigPath))
            {
                using (var reader = new StreamReader(ConfigPath))
                {
                    var des = new Deserializer();
                    GlobalConfigure = des.Deserialize<Configure>(reader);
                    GlobalConfigure = GlobalConfigure ?? new Configure();
                }
            }
        }
        public static void SaveGlobalConfigure()
        {
            if (GlobalConfigure != null)
            {
                var ConfigPath = Path.Combine(Environment.CurrentDirectory, ConfigFile);
                using (var writer = new StreamWriter(ConfigPath))
                {
                    var ser = new Serializer();
                    ser.Serialize(writer, GlobalConfigure);
                }
            }
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            LoadGlobalConfigure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
