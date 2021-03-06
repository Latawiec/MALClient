using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Util;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Transformations;
using FFImageLoading.Views;
using GalaSoft.MvvmLight.Helpers;
using MALClient.Android.BindingConverters;
using MALClient.Models.Models;
using MALClient.Models.Models.MalSpecific;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Android.Fragments
{
    public class FriendsFeedsPageFragment : MalFragmentBase
    {
        private FriendsFeedsViewModel ViewModel;
        private GridViewColumnHelper _gridViewColumnHelper;

        protected override void Init(Bundle savedInstanceState)
        {
            ViewModel = ViewModelLocator.FriendsFeeds;
            ViewModel.Init();
        }

        protected override void InitBindings()
        {
            Bindings.Add(
                this.SetBinding(() => ViewModel.Loading,
                    () => FriendsFeedsPageLoadingSpinner.Visibility).ConvertSourceToTarget(Converters.BoolToVisibility));

            _gridViewColumnHelper = new GridViewColumnHelper(FriendsFeedsPageGridView);

            Bindings.Add(this.SetBinding(() => ViewModel.Feeds).WhenSourceChanges(() =>
            {
                if (ViewModel.Feeds != null)
                    FriendsFeedsPageGridView.InjectFlingAdapter(ViewModel.Feeds,DataTemplateFull,DataTemplateFling,ContainerTemplate);
                else
                    FriendsFeedsPageGridView.Adapter = null;
            }));
        }

        private View ContainerTemplate()
        {
            var view = Activity.LayoutInflater.Inflate(Resource.Layout.FriendsFeedsPageItem, null);

            view.Click += RootFeedItemOnClick;
            view.FindViewById(Resource.Id.FriendsFeedsPageItemUserImageButton).Click+= UserButtonOnClick;

            return view;
        }

        private void DataTemplateFling(View view, UserFeedEntryModel userFeedEntryModel)
        {
            view.FindViewById(Resource.Id.FriendsFeedsPageItemImage).Visibility = ViewStates.Invisible;
            view.FindViewById(Resource.Id.FriendsFeedsPageItemUserImageButton).Visibility = ViewStates.Invisible;
            view.FindViewById(Resource.Id.FriendsFeedsPageItemImgPlaceholder).Visibility = ViewStates.Visible;
            view.FindViewById<TextView>(Resource.Id.FriendsFeedsPageItemUserName).Text = userFeedEntryModel.User.Name;
            view.FindViewById<TextView>(Resource.Id.FriendsFeedsPageItemTitle).Text = userFeedEntryModel.Title;
            view.FindViewById<TextView>(Resource.Id.FriendsFeedsPageItemContent).Text = userFeedEntryModel.Description;
            view.FindViewById<TextView>(Resource.Id.FriendsFeedsPageItemDate).Text = userFeedEntryModel.Date.ToDiffString();
        }

        private void DataTemplateFull(View view, UserFeedEntryModel userFeedEntryModel)
        {
            var img = view.FindViewById<ImageViewAsync>(Resource.Id.FriendsFeedsPageItemImage);
            if (img.Tag == null || (string) img.Tag != $"{userFeedEntryModel.Date.Ticks}{userFeedEntryModel.User.Name}")
            {
                img.Tag = $"{userFeedEntryModel.Date.Ticks}{userFeedEntryModel.User.Name}";
                string imgUrl = null;
                if(AnimeImageQuery.IsCached(userFeedEntryModel.Id,true,ref imgUrl))
                    img.Into(imgUrl); 
                else
                    img.IntoWithTask(AnimeImageQuery.GetImageUrl(userFeedEntryModel.Id, true));
                view.FindViewById<ImageViewAsync>(Resource.Id.FriendsFeedsPageItemUserImage).Into(userFeedEntryModel.User.ImgUrl, new CircleTransformation());
                view.FindViewById(Resource.Id.FriendsFeedsPageItemUserImageButton).Tag = userFeedEntryModel.User.Wrap();
                view.FindViewById(Resource.Id.FriendsFeedsPageItemUserImageButton).Visibility = ViewStates.Visible;
                view.FindViewById(Resource.Id.FriendsFeedsPageItemImgPlaceholder).Visibility = ViewStates.Gone;
                view.FindViewById<TextView>(Resource.Id.FriendsFeedsPageItemUserName).Text = userFeedEntryModel.User.Name;
                view.FindViewById<TextView>(Resource.Id.FriendsFeedsPageItemTitle).Text = userFeedEntryModel.Title;
                view.FindViewById<TextView>(Resource.Id.FriendsFeedsPageItemContent).Text = userFeedEntryModel.Description;
                view.FindViewById<TextView>(Resource.Id.FriendsFeedsPageItemDate).Text = userFeedEntryModel.Date.ToDiffString();
            }           
        }

        private void RootFeedItemOnClick(object sender, EventArgs eventArgs)
        {
            ViewModel.NavigateDeitalsCommand.Execute((sender as View).Tag.Unwrap<UserFeedEntryModel>());
        }

        private void UserButtonOnClick(object sender, EventArgs eventArgs)
        {
            ViewModel.NavigateProfileCommand.Execute((sender as View).Tag.Unwrap<MalUser>());
        }

        public override int LayoutResourceId => Resource.Layout.FriendsFeedsPage;

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            _gridViewColumnHelper?.OnConfigurationChanged(newConfig);
            base.OnConfigurationChanged(newConfig);
        }

        #region Views

        private GridView _friendsFeedsPageGridView;
        private ProgressBar _friendsFeedsPageLoadingSpinner;

        public GridView FriendsFeedsPageGridView => _friendsFeedsPageGridView ?? (_friendsFeedsPageGridView = FindViewById<GridView>(Resource.Id.FriendsFeedsPageGridView));

        public ProgressBar FriendsFeedsPageLoadingSpinner => _friendsFeedsPageLoadingSpinner ?? (_friendsFeedsPageLoadingSpinner = FindViewById<ProgressBar>(Resource.Id.FriendsFeedsPageLoadingSpinner));

        #endregion
    }
}