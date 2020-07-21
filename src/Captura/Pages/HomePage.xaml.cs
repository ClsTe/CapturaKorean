using Captura.ViewModels;
using Microsoft.Win32;
using mshtml;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Captura
{
    public partial class HomePage
    {
        private static bool willNavigate;

        public HomePage()
        {
            int BrowserVer, RegVal;

            try
            {
                // get the installed IE version
                using (WebBrowser Wb = new WebBrowser())
                    BrowserVer = Wb.Version.Major;

                // set the appropriate IE version
                if (BrowserVer >= 11)
                    RegVal = 11001;
                else if (BrowserVer == 10)
                    RegVal = 10001;
                else if (BrowserVer == 9)
                    RegVal = 9999;
                else if (BrowserVer == 8)
                    RegVal = 8888;
                else
                    RegVal = 7000;

                // set the actual key
                using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", RegistryKeyPermissionCheck.ReadWriteSubTree))
                    if (Key.GetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe") == null)
                        Key.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", RegVal, RegistryValueKind.DWord);

                //SetBrowserFeatureControl();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            InitializeComponent();
            //banner.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(WebBrowser_LoadCompletedAsync);

            if (DataContext is MainViewModel vm)
            {
                vm.Refreshed += () =>
                {
                    AudioDropdown.Shake();

                    VideoWriterComboBox.Shake();
                };
            }
        }

        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }

        public void setTimeout(Action TheAction, int Timeout)
        {
            Thread t = new Thread(
                () =>
                {
                    Thread.Sleep(Timeout);
                    TheAction.Invoke();
                }
            );
            t.Start();
        }

        private async Task<string> SimLongRunningProcessAsync()
        {
            await Task.Delay(10000);
            return "Success";
        }

        private void WebBrowser_Navigating_1(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
           // first page needs to be loaded in webBrowser control
            if (!willNavigate)
            {
                willNavigate = true;
                return;
            }

            // cancel navigation to the clicked link in the webBrowser control
            e.Cancel = true;

            var startInfo = new ProcessStartInfo
            {
                FileName = e.Uri.ToString()
            };

            Process.Start(startInfo);
            banner.Visibility = System.Windows.Visibility.Hidden;
        }

        async void WebBrowser_LoadCompletedAsync(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            banner.Visibility = System.Windows.Visibility.Hidden;
            var result = await SimLongRunningProcessAsync();

            mshtml.IHTMLDocument2 doc = this.banner.Document as mshtml.IHTMLDocument2;
            mshtml.IHTMLWindow2 win = doc.parentWindow as mshtml.IHTMLWindow2;

            IHTMLDocument2 frameDocument = (IHTMLDocument2)CrossFrame.GetDocumentFromWindow(win);
            mshtml.IHTMLWindow2 frameDocumentWin = frameDocument.parentWindow as mshtml.IHTMLWindow2;
            mshtml.IHTMLDocument2 innerdoc = frameDocumentWin.document as mshtml.IHTMLDocument2;


            banner.Visibility = System.Windows.Visibility.Visible;

            ((mshtml.HTMLDocumentEvents2_Event)doc).onclick += new mshtml.HTMLDocumentEvents2_onclickEventHandler(ClickEventHandler);
            ((mshtml.HTMLDocumentEvents2_Event)innerdoc).onclick += new mshtml.HTMLDocumentEvents2_onclickEventHandler(InnerClickEventHandler);
        }


        private bool ClickEventHandler(mshtml.IHTMLEventObj e)
        {
            banner.Visibility = System.Windows.Visibility.Hidden;
            return false;
        }

        private bool InnerClickEventHandler(mshtml.IHTMLEventObj e)
        {
            banner.Visibility = System.Windows.Visibility.Hidden;
            //Process.Start("https://coupa.ng/bGnnnr");
            
            //var a = (mshtml.HTMLAnchorElement)e.srcElement;
            //Process.Start("https://coupa.ng/bGnnnr");
            return false;
        }


        private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }

        private void SetBrowserFeatureControl()
        {
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // FeatureControl settings are per-process
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            // make the control is not running inside Visual Studio Designer
            if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                return;

            // SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode()); // Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode.
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }
    }

    public class CrossFrame
    {
        // Returns null in case of failure.
        public static IHTMLDocument2 GetDocumentFromWindow(IHTMLWindow2 htmlWindow)
        {
            if (htmlWindow == null)
            {
                return null;
            }

            // First try the usual way to get the document.
            try
            {
                IHTMLDocument2 doc = htmlWindow.document;
                return doc;
            }
            catch (COMException comEx)
            {
                // I think COMException won't be ever fired but just to be sure ...
                if (comEx.ErrorCode != E_ACCESSDENIED)
                {
                    return null;
                }
            }
            catch (System.UnauthorizedAccessException)
            {
            }
            catch
            {
                // Any other error.
                return null;
            }

            // At this point the error was E_ACCESSDENIED because the frame contains a document from another domain.
            // IE tries to prevent a cross frame scripting security issue.
            try
            {
                // Convert IHTMLWindow2 to IWebBrowser2 using IServiceProvider.
                IServiceProvider sp = (IServiceProvider)htmlWindow;

                // Use IServiceProvider.QueryService to get IWebBrowser2 object.
                Object brws = null;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out brws);

                // Get the document from IWebBrowser2.
                SHDocVw.IWebBrowser2 browser = (SHDocVw.IWebBrowser2)(brws);

                mshtml.IHTMLDocument2 doc = browser.Document as mshtml.IHTMLDocument2;
                return (IHTMLDocument2)doc;
            }
            catch
            {
            }

            return null;
        }

        private const int E_ACCESSDENIED = unchecked((int)0x80070005L);
        private static Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        private static Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E");
    }

    // This is the COM IServiceProvider interface, not System.IServiceProvider .Net interface!
    [ComImport(), ComVisible(true), Guid("6D5140C1-7436-11CE-8034-00AA006009FA"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IServiceProvider
    {
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig]
        int QueryService(ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
    }
}
