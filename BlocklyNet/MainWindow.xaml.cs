using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using System.ComponentModel;
using mshtml;
using ICSharpCode.AvalonEdit.Highlighting;
using System.IO;
using System.Xml;

namespace BlocklyNet
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string xml ="";
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BlocklyNet_Loaded(object sender, RoutedEventArgs e)
        {
            SetWebBrowserFeatures(10);
            Debug.WriteLine(GetBrowserVersion());
            BlockIndex.Source = new Uri(basePath+ @"Block\demos\code\index.html");
            EditInit();
        }

        static void SetWebBrowserFeatures(int ieVersion)
        {
            // don't change the registry if running in-proc inside Visual Studio
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            //获取程序及名称
            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            //得到浏览器的模式的值
            UInt32 ieMode = GeoEmulationModee(ieVersion);
            var featureControlRegKey = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\";
            //设置浏览器对应用程序（appName）以什么模式（ieMode）运行
            Registry.SetValue(featureControlRegKey + "FEATURE_BROWSER_EMULATION",
                appName, ieMode, RegistryValueKind.DWord);
            // enable the features which are "On" for the full Internet Explorer browser
            //不晓得设置有什么用
            Registry.SetValue(featureControlRegKey + "FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION",
                appName, 1, RegistryValueKind.DWord);
            //Registry.SetValue(featureControlRegKey + "FEATURE_AJAX_CONNECTIONEVENTS",
            //    appName, 1, RegistryValueKind.DWord);

            //Registry.SetValue(featureControlRegKey + "FEATURE_GPU_RENDERING",
            //    appName, 1, RegistryValueKind.DWord);

            //Registry.SetValue(featureControlRegKey + "FEATURE_WEBOC_DOCUMENT_ZOOM",
            //    appName, 1, RegistryValueKind.DWord);

            //Registry.SetValue(featureControlRegKey + "FEATURE_NINPUT_LEGACYMODE",
            //    appName, 0, RegistryValueKind.DWord);
        }

        static int GetBrowserVersion()
        {
            int browserVersion = 0;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }
            //如果小于7
            if (browserVersion < 7)
            {
                throw new ApplicationException("不支持的浏览器版本!");
            }
            return browserVersion;
        }

        static UInt32 GeoEmulationModee(int browserVersion)
        {
            UInt32 mode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode. 
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. 
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. 
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode.                    
                    break;
                case 10:
                    mode = 10000; // Internet Explorer 10.
                    break;
                case 11:
                    mode = 11000; // Internet Explorer 11
                    break;
            }
            return mode;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            
            var doc = this.BlockIndex.Document as HTMLDocument;
            xml = (string)BlockIndex.InvokeScript("save", null);
            CodeView.Text = xml;
            Debug.WriteLine(xml);
        }

        private void load_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(xml);
            Debug.WriteLine(BlockIndex.InvokeScript("load", xml));
        }

        private void run_Click(object sender, RoutedEventArgs e)
        {
            CodeView.Text = (string)BlockIndex.InvokeScript("generate", xml);
        }

        private void EditInit()
        {
            IHighlightingDefinition customHighlighting;
            using (StreamReader s = new StreamReader(Environment.CurrentDirectory + "\\Highlights\\Python_Mode.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("Python", new string[] { ".py" }, customHighlighting);
            CodeView.SyntaxHighlighting = customHighlighting;
        }


    }
}
