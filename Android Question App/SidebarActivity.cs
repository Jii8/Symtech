using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Webkit;
using static Android.App.ActionBar;

namespace Android_Question_App
{
    [Activity(Label = "SidebarActivity", Theme = "@style/AppTheme.NoActionBar")]
    public class SidebarActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var sidebarHtml = Intent.Extras.GetString(Constant.SIDEBAR_HTML); // intent name as a constant to refer to the same variable.
            var webView = new WebView(this); // show static webpage
            AddContentView(webView, new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)); // match parent instead of hardcoded values
            webView.LoadData(sidebarHtml, GetString(Resource.String.mimeType_html), GetString(Resource.String.encoding_utf_8)); // moved to resource file
        }
    }
}