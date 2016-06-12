using System;//
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Webkit;
using Android.Content.PM;
using System.IO;

namespace DualPDF
{
    [Activity(Label = "DualPDF", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        WebView web_view1;
        WebView web_view2;
        public static readonly int PickPDFId1 = 1750;
        public static readonly int PickPDFId2 = 1751;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.RequestedOrientation = ScreenOrientation.Landscape;

            SetContentView(Resource.Layout.Main);

            web_view1 = FindViewById<WebView>(Resource.Id.webView1);
            web_view1.Settings.AllowUniversalAccessFromFileURLs = true;
            web_view1.Settings.JavaScriptEnabled = true;
            web_view1.LoadUrl("file:///android_asset/pdfjs-1.4.20/web/viewer.html?file=");
            
            //string fname = "file://" + CacheDir + "/temp1.pdf";
            //web_view1.LoadUrl("javascript:PDFViewerApplication.open('" + fname + "')");

            web_view2 = FindViewById<WebView>(Resource.Id.webView2);
            web_view2.Settings.AllowUniversalAccessFromFileURLs = true;
            web_view2.Settings.JavaScriptEnabled = true;
            web_view2.LoadUrl("file:///android_asset/pdfjs-1.4.20/web/viewer.html?file=");

            //fname = "file://" + CacheDir + "/temp2.pdf";
            //web_view2.LoadUrl("javascript:PDFViewerApplication.open('" + fname + "')");

            return;
        }

        //protected override void OnResume()
        //{
        //    base.OnResume();
        //    web_view1.Reload();
        //    web_view2.Reload();
        //    return;
        //}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            MenuInflater.Inflate(Resource.Menu.menu, menu);

            return true;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            string fname;
            if ((requestCode == PickPDFId1 || requestCode == PickPDFId2) && (resultCode == Result.Ok) && (data != null))
            {
                /*
                if (data.Data.ToString().StartsWith("file"))
                {
                    fname = data.Data.ToString();
                }
                else if (data.Data.ToString().StartsWith("content"))
                {
                    fname = (requestCode == PickPDFId1) ? "/temp1.pdf" : "/temp2.pdf";
                    fname = this.CacheDir + fname;
                    Console.WriteLine(fname);
                    Stream iS = ContentResolver.OpenInputStream(data.Data);
                    FileStream oS = new FileStream(fname, FileMode.Create, FileAccess.Write);
                    iS.CopyTo(oS);
                    iS.Close(); oS.Close();

                    Console.Write(fname);
                    //fname = "file://" + fname;
                }
                else { return; }
                */

                fname = (requestCode == PickPDFId1) ? "/temp1.pdf" : "/temp2.pdf";
                fname = CacheDir + fname;
                //fname = Android.OS.Environment.ExternalStorageDirectory + fname;
                Console.WriteLine("###DEBUG### File writing... "+fname);
                Stream iS = ContentResolver.OpenInputStream(data.Data);
                FileStream oS = new FileStream(fname, FileMode.Create, FileAccess.Write);
                iS.CopyTo(oS);
                iS.Close(); oS.Close();

                Console.Write(fname);
                fname = "file://" + fname;
            }
            else { return; }

            if (requestCode == PickPDFId1){ openpdf(web_view1, fname); }
            else{ openpdf(web_view2, fname); }

            return;
        }

        public void openpdf(WebView w, string fname)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
            {
                string url = String.Format("file:///android_asset/pdfjs-1.4.20/web/viewer.html?file={0}", fname);
                w.LoadUrl(url);
            }else
            {
                w.LoadUrl("javascript:PDFViewerApplication.open('" + fname + "')");
                w.ClearCache(false);
            }
            return;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_open1)
            {
                startIntent(0);
            }
            if (item.ItemId == Resource.Id.menu_open2)
            {
                startIntent(1);
            }

            if (item.ItemId == Resource.Id.menu_license)
            {
                var dlg = new AlertDialog.Builder(this);
                dlg.SetTitle("License");
                dlg.SetMessage("This app uses PDF.js");
                dlg.SetPositiveButton(
                    "WebSite", (s, a) => openweb("https://mozilla.github.io/pdf.js/")
                );
                dlg.SetNegativeButton(
                    "License", (s, a) => openweb("http://www.apache.org/licenses/LICENSE-2.0.html")
                );
                dlg.Create().Show();
            }

            if (item.ItemId == Resource.Id.menu_about)
            {
                var dlg = new AlertDialog.Builder(this);
                string vname = "Version: " + PackageManager.GetPackageInfo(this.PackageName, 0).VersionName;
                dlg.SetTitle("About this application");
                dlg.SetMessage(vname);
                dlg.SetPositiveButton(
                    "WebSite", (s, a) => openweb("http://www.yyyak.com/blog/post/dualpdf")
                );

                dlg.Create().Show();
            }

            return base.OnOptionsItemSelected(item);
        }

        protected void openweb(string url)
        {
            var uri = Android.Net.Uri.Parse(url);
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);

            return;
        }

        protected void startIntent(int button_num)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
            {
                //Intent intent = new Intent(Intent.ActionOpenDocument);
                Intent intent = new Intent(Intent.ActionGetContent);
                intent.SetType("application/pdf");
                try
                {
                    StartActivityForResult(Intent.CreateChooser(intent, "Select PDF File"), 1750 + button_num);
                }
                catch (Exception e)
                {
                    Toast.MakeText(this, "No App handling PDF", ToastLength.Long).Show();
                    return;
                }
            }
            else
            {
                Intent intent = new Intent(Intent.ActionGetContent);
                intent.SetType("application/pdf");
                //intent.SetType("*/*");
                //intent.SetAction(Intent.ActionPick);
                try
                {
                    StartActivityForResult(intent, 1750 + button_num);
                }
                catch (Exception e)
                {
                    Toast.MakeText(this, "No App handling PDF", ToastLength.Long).Show();
                    return;
                }
            }
            return;
        }

        

    }
}

