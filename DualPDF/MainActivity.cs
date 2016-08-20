using System;
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
    [Activity(Label = "DualPDF", MainLauncher = true, Icon = "@drawable/icon", LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
    [IntentFilter(new[] { Intent.ActionView },
     Categories = new[] { Intent.CategoryDefault },
     DataMimeType = "application/pdf")]
    public class MainActivity : Activity
    {
        WebView web_view1;
        WebView web_view2;
        LinearLayout l1;
        LinearLayout l2;
        public static readonly int PickPDFId1 = 1750;
        public static readonly int PickPDFId2 = 1751;
        //String baseaddr = "file:///android_asset/pdfjs-1.4.20/web/viewer.html?file=";
        String baseaddr = "file:///android_asset/pdfjs-1.5.188/web/viewer.html?file=";


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            

            SetContentView(Resource.Layout.Main);
            l1 = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            l2 = FindViewById<LinearLayout>(Resource.Id.linearLayout2);
            web_view1 = FindViewById<WebView>(Resource.Id.webView1);
            web_view2 = FindViewById<WebView>(Resource.Id.webView2);
 
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBean) {
                web_view1.Settings.AllowUniversalAccessFromFileURLs = true;
            }
            web_view1.Settings.JavaScriptEnabled = true;
            web_view1.LoadUrl(baseaddr);
            
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBean)
            {
                web_view2.Settings.AllowUniversalAccessFromFileURLs = true;
            }
            web_view2.Settings.JavaScriptEnabled = true;
            web_view2.LoadUrl(baseaddr);

            Intent i = this.Intent;
            if (i.Data != null)
            {
                var dlg = new AlertDialog.Builder(this);
                string[] items = { "Left", "Right" };
                dlg.SetTitle("Select Display");
                dlg.SetItems(items,
                    (sender, args) =>
                    {
                        copyfile(1750 + args.Which, i);
                        openpdf(args.Which);
                    }
                );
                dlg.Create().Show();
            }

            if (System.IO.File.Exists(CacheDir + "/temp1.pdf"))
            {
                openpdf(0);
            }
            if (System.IO.File.Exists(CacheDir + "/temp2.pdf"))
            {
                openpdf(1);
            }
            return;
        }


        protected override void OnNewIntent(Intent i)
        {
            base.OnNewIntent(i);

            if (i.Data != null)
            {
                var dlg = new AlertDialog.Builder(this);
                string[] items = { "Left/Upper", "Right/Lower" };
                dlg.SetTitle("Select Display");
                dlg.SetItems(items,
                    (sender, args) =>
                    {
                        copyfile(1750 + args.Which, i);
                        openpdf(args.Which);
                    }
                );
                dlg.Create().Show();
            }
            return;
        }
        
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            MenuInflater.Inflate(Resource.Menu.menu, menu);

            return true;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if ((requestCode == PickPDFId1 || requestCode == PickPDFId2) && (resultCode == Result.Ok) && (data.Data != null))
            {
                copyfile(requestCode,data);
            }
            else { return; }

            if (requestCode == PickPDFId1){ openpdf(0); }
            else{ openpdf(1); }
            return;
        }

        protected void copyfile(int requestCode, Intent data)
        {
            string fname = (requestCode == PickPDFId1) ? "/temp1.pdf" : "/temp2.pdf";
            fname = CacheDir + fname;
            Console.WriteLine("###DEBUG### File writing... " + fname);
            Console.WriteLine("###DEBUG### Intent: " + data.Data);
            Stream iS = ContentResolver.OpenInputStream(data.Data);
            FileStream oS = new FileStream(fname, FileMode.Create, FileAccess.Write);
            iS.CopyTo(oS);
            iS.Close(); oS.Close();
        }

        public void openpdf(int num)
        {
            string fname;
            WebView w = (num == 0) ? web_view1 : web_view2;
            fname = "file://" + CacheDir;
            fname += (num == 0) ? "/temp1.pdf" : "/temp2.pdf";


            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat || Android.OS.Build.VERSION.SdkInt<Android.OS.BuildVersionCodes.Honeycomb)
            {
                string url = baseaddr + fname;
                w.ClearCache(false);
                w.LoadUrl(url);
            }
            else
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

            if (item.ItemId == Resource.Id.menu_left)
            {
                if (((LinearLayout.LayoutParams)l2.LayoutParameters).Weight > 0.1)
                {
                    LinearLayout.LayoutParams params1 = (LinearLayout.LayoutParams)l1.LayoutParameters;
                    params1.Weight += 0.1f;
                    LinearLayout.LayoutParams params2 = (LinearLayout.LayoutParams)l2.LayoutParameters;
                    params2.Weight -= 0.1f;
                    l1.LayoutParameters = params1;
                    l2.LayoutParameters = params2;
                }
            }
            if (item.ItemId == Resource.Id.menu_right)
            {
                if (((LinearLayout.LayoutParams)l1.LayoutParameters).Weight > 0.1)
                {
                    LinearLayout.LayoutParams params1 = (LinearLayout.LayoutParams)l1.LayoutParameters;
                    params1.Weight -= 0.1f;
                    LinearLayout.LayoutParams params2 = (LinearLayout.LayoutParams)l2.LayoutParameters;
                    params2.Weight += 0.1f;
                    l1.LayoutParameters = params1;
                    l2.LayoutParameters = params2;
                }
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

