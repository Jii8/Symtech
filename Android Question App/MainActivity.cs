using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Transitions;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using static Android.Widget.AdapterView;

namespace Android_Question_App
{

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region Properties

        EditText mSubredditEditText;
        Button mSubredditSearchButton;
        ListView mSubredditListView;

        ProgressBar mProgressBar;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            mProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            mSubredditEditText = FindViewById<EditText>(Resource.Id.search_textInput);
            mSubredditSearchButton = FindViewById<Button>(Resource.Id.search_button);
            mSubredditSearchButton.Click += SubredditSearchButton_Click;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #region Events

        private void SubredditSearchButton_Click(object sender, EventArgs e)
        {
            // Validation. Check whether search box has any input
            if (!IsValidTextInput(mSubredditEditText.Text))
            {
                Toast.MakeText(this, String.Format(GetString(Resource.String.errorMsg_emptyInput), "subreddit search textbox"), ToastLength.Short).Show();
            }
            else
            {
                mProgressBar.Visibility = ViewStates.Visible; // to show loading progress bar

                List<string> subredditList = new List<string>(); // subreddit search result items

                // Async task to avoid screen freezing and for better user experience
                Task.Run(async () =>
                {
                    try
                    {
                        var subreddits = await SubredditSearchResult();

                        // moved subreddit json keys to resource file
                        foreach (var subreddit in subreddits[GetString(Resource.String.subreddit_data)][GetString(Resource.String.subreddit_data_children)] as JArray)
                        {
                            var subredditName = subreddit[GetString(Resource.String.subreddit_data)][GetString(Resource.String.subreddit_data_display_name_pref)].ToString();
                            subredditList.Add(subredditName);
                        }

                        // UI updates must be in mainthread to update/show in the screen
                        MainThread.BeginInvokeOnMainThread(() =>
                            {
                                // Bind the search result to the list view
                                mSubredditListView = (ListView)FindViewById<ListView>(Resource.Id.subreddit_listView);
                                mSubredditListView.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, subredditList);
                                mSubredditListView.ItemClick += SubredditListItem_Click;

                                mProgressBar.Visibility = ViewStates.Invisible;

                            });
                    }
                    catch (Exception)
                    {
                        // to show error message in the screen
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            mProgressBar.Visibility = ViewStates.Gone;

                            Toast.MakeText(this, GetString(Resource.String.errorMsg_systemError_generic), ToastLength.Long).Show();
                        });
                    }

                });
            }
        }

        private void SubredditListItem_Click(object sender, ItemClickEventArgs e)
        {
            try
            {

                var subredditName = Convert.ToString(mSubredditListView.GetItemAtPosition(e.Position)); // get the clicked item text                    

                // moved url to resource file
                var sidebarHtml = new WebClient().DownloadString(GetString(Resource.String.reddit_url) + subredditName + GetString(Resource.String.subreddit_sidebar_path));
                var intent = new Intent(this, typeof(SidebarActivity));
                intent.PutExtra(Constant.SIDEBAR_HTML, sidebarHtml); // intent name as a constant to refer to the same variable.
                this.StartActivity(intent);

            }
            catch (Exception)
            {
                Toast.MakeText(this, GetString(Resource.String.errorMsg_systemError_generic), ToastLength.Long).Show();

            }
        }

        #endregion

        #region Validation

        private bool IsValidTextInput(string textInput)
        {
            if (string.IsNullOrEmpty(textInput) || string.IsNullOrWhiteSpace(textInput))
                return false;
            else
                return true;
        }

        #endregion

        #region Async Methods

        async Task<JObject> SubredditSearchResult()
        {
            try
            {
                var jsonSubreddits = await new WebClient().DownloadStringTaskAsync(GetString(Resource.String.subreddit_url) + mSubredditEditText.Text); // renamed button
                var subreddits = JsonConvert.DeserializeObject<JObject>(jsonSubreddits);

                return subreddits;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }
}

