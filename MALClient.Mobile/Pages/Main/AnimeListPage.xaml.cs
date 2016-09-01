﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MalClient.Shared.ViewModels;
using MALClient.ViewModels;
using MALClient.XShared.Utils.Enums;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Interfaces;
using MALClient.XShared.ViewModels.Main;
using AnimeListPageNavigationArgs = MALClient.XShared.NavArgs.AnimeListPageNavigationArgs;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages.Main
{
   
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnimeListPage : Page , IDimensionsProvider
    {
        private ScrollViewer _indefiniteScrollViewer;
        private AnimeListViewModel ViewModel => DataContext as AnimeListViewModel;

        public ScrollViewer IndefiniteScrollViewer
        {
            private get
            {
                return _indefiniteScrollViewer ??
                       (_indefiniteScrollViewer =
                           VisualTreeHelper.GetChild(
                               VisualTreeHelper.GetChild((DependencyObject)GetScrollingContainer(), 0), 0) as
                               ScrollViewer);
            }
            set { _indefiniteScrollViewer = value; }
        }

        public Flyout FlyoutViews => ViewsFlyout;
        public Flyout FlyoutFilters => FiltersFlyout;
        public MenuFlyout FlyoutSorting => SortingFlyout;

        public async Task<ScrollViewer> GetIndefiniteScrollViewer()
        {
            if (!_loaded)
            {
                var retries = 15;
                while (retries-- > 0 && !_loaded)
                {
                    await Task.Delay(50);
                }
            }
            try
            {
                if (_loaded)
                    return IndefiniteScrollViewer;
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private object GetScrollingContainer()
        {
            switch (ViewModel.DisplayMode)
            {
                case AnimeListDisplayModes.IndefiniteList:
                    return AnimesItemsIndefinite;
                case AnimeListDisplayModes.IndefiniteGrid:
                    return AnimesGridIndefinite;
                case AnimeListDisplayModes.IndefiniteCompactList:
                    return AnimeCompactItemsIndefinite;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void FlyoutSeasonSelectionHide()
        {
            FlyoutSeasonSelection.Hide();
        }

        private async void AnimesItemsIndefinite_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            await Task.Delay(1);
            var args = MobileViewModelLocator.Main.GetCurrentListOrderParams();
            args.SelectedItemIndex = (sender as ListViewBase).SelectedIndex;
            (e.AddedItems.First() as AnimeItemViewModel).NavigateDetails(null, args);
        }

        private bool _loaded;
        private AnimeListPageNavigationArgs _lastArgs;

        public AnimeListPage()
        {
            InitializeComponent();
            Loaded += async (sender, args) =>
            {
                ViewModel.ScrollIntoViewRequested += ViewModelOnScrollRequest;
                ViewModel.CanAddScrollHandler = true;
                ViewModel.SortingSettingChanged += InitSortOptions;
                ViewModel.Init(_lastArgs);
                ViewModel.DimensionsProvider = this;
                _loaded = true;
                ViewModel.HideFiltersFlyout += ViewModelOnHideFiltersFlyout;
                ViewModel.HideSortingFlyout += ViewModelOnHideSortingFlyout;
                ViewModel.HideViewsFlyout += ViewModelOnHideViewsFlyout;
                ViewModel.ScrollToTopRequest += ViewModelOnScrollToTopRequest;
                ViewModel.AddScrollHandlerRequest += ViewModelOnAddScrollHandlerRequest;
                ViewModel.RemoveScrollHandlerRequest += ViewModelOnRemoveScrollHandlerRequest;
                ViewModel.RemoveScrollingConatinerReferenceRequest += ViewModelOnRemoveScrollingConatinerReferenceRequest;
                ViewModel.HideSeasonSelectionFlyout += ViewModelOnHideSeasonSelectionFlyout;
                _loaded = true;
                try
                {
                    await Task.Delay(100);
                    VisualStateManager.GoToState(this, ActualHeight > 700 ? "TallItems" : "ShortItems", false); //force update on startup
                }
                catch (Exception)
                {
                    //comexception
                }
                
            };
        }

        private void ViewModelOnHideSeasonSelectionFlyout()
        {
            FlyoutSeasonSelection.Hide();
        }

        private void ViewModelOnHideViewsFlyout()
        {
            FlyoutViews.Hide();
        }

        private void ViewModelOnRemoveScrollingConatinerReferenceRequest()
        {
            IndefiniteScrollViewer = null;
        }

        private async void ViewModelOnRemoveScrollHandlerRequest()
        {
            (await GetIndefiniteScrollViewer()).ViewChanging -= IndefiniteScrollViewerOnViewChanging;
        }

        private async void ViewModelOnAddScrollHandlerRequest()
        {
            (await GetIndefiniteScrollViewer()).ViewChanging += IndefiniteScrollViewerOnViewChanging;

        }

        private void IndefiniteScrollViewerOnViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            ViewModel.IndefiniteScrollViewerOnViewChanging(e.FinalView.VerticalOffset);
        }

        private async void ViewModelOnScrollToTopRequest()
        {
            (await GetIndefiniteScrollViewer()).ChangeView(null, 0, null);
            ViewModelLocator.GeneralMain.ScrollToTopButtonVisibility = false;
        }

        private void ViewModelOnHideSortingFlyout()
        {
            SortingFlyout.Hide();
        }

        private void ViewModelOnHideFiltersFlyout()
        {
            FiltersFlyout.Hide();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModelLocator.AnimeList.OnNavigatedFrom();
            base.OnNavigatedFrom(e);
        }

        private void ViewModelOnScrollRequest(AnimeItemViewModel item,bool select = false)
        {
            switch (ViewModel.DisplayMode)
            {
                case AnimeListDisplayModes.IndefiniteCompactList:
                    AnimeCompactItemsIndefinite.ScrollIntoView(item);
                    if (select)
                        AnimeCompactItemsIndefinite.SelectedItem = item;
                    break;
                case AnimeListDisplayModes.IndefiniteList:
                    AnimesItemsIndefinite.ScrollIntoView(item);
                    if (select)
                        AnimesItemsIndefinite.SelectedItem = item;
                    break;
                case AnimeListDisplayModes.IndefiniteGrid:
                    AnimesGridIndefinite.ScrollIntoView(item);
                    if (select)
                        AnimesGridIndefinite.SelectedItem = item;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _lastArgs = e.Parameter as AnimeListPageNavigationArgs;
        }

        private void SelectSortMode(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleMenuFlyoutItem;
            switch (btn.Text)
            {
                case "Title":
                    ViewModel.SortOption = SortOptions.SortTitle;
                    break;
                case "Score":
                    ViewModel.SortOption = SortOptions.SortScore;
                    break;
                case "Watched":
                case "Read":
                    ViewModel.SortOption = SortOptions.SortWatched;
                    break;
                case "Soonest airing":
                    ViewModel.SortOption = SortOptions.SortAirDay;
                    break;
                case "Last watched":
                    ViewModel.SortOption = SortOptions.SortLastWatched;
                    break;
                case "Start date":
                    ViewModel.SortOption = SortOptions.SortStartDate;
                    break;
                case "End date":
                    ViewModel.SortOption = SortOptions.SortEndDate;
                    break;
                default:
                    ViewModel.SortOption = SortOptions.SortNothing;
                    break;
            }
            SortTitle.IsChecked =
                SortScore.IsChecked =
                    Sort3.IsChecked =
                        SortAiring.IsChecked =
                            SortNone.IsChecked =
                                SortLastWatched.IsChecked =
                                    SortEndDate.IsChecked =
                                        SortStartDate.IsChecked = false;
            btn.IsChecked = true;
            ViewModel.RefreshList();
        }


        private void ChangeSortOrder(object sender, RoutedEventArgs e)
        {
            var chbox = sender as ToggleMenuFlyoutItem;
            ViewModel.SortDescending = chbox.IsChecked;
            ViewModel.RefreshList();
        }

        private async void ListSource_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((sender == null && e == null) || e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                TxtListSource.IsEnabled = false; //reset input
                TxtListSource.IsEnabled = true;
                FlyoutListSource.Hide();
                BottomCommandBar.IsOpen = false;              
                await ViewModel.FetchData();
            }
        }

        private void ShowListSourceFlyout(object sender, RoutedEventArgs e)
        {
            FlyoutListSource.ShowAt(sender as FrameworkElement);
        }

        private void FlyoutListSource_OnOpened(object sender, object e)
        {
            TxtListSource.SelectAll();
        }

        internal void InitSortOptions(SortOptions option, bool descending)
        {
            SortTitle.IsChecked =
                SortScore.IsChecked =
                    Sort3.IsChecked =
                        SortAiring.IsChecked =
                            SortNone.IsChecked =
                                SortLastWatched.IsChecked =
                                    SortEndDate.IsChecked =
                                        SortStartDate.IsChecked = false;
            switch (option)
            {
                case SortOptions.SortTitle:
                    SortTitle.IsChecked = true;
                    break;
                case SortOptions.SortScore:
                    SortScore.IsChecked = true;
                    break;
                case SortOptions.SortWatched:
                    Sort3.IsChecked = true;
                    break;
                case SortOptions.SortAirDay:
                    SortAiring.IsChecked = true;
                    break;
                case SortOptions.SortNothing:
                    SortNone.IsChecked = true;
                    break;
                case SortOptions.SortLastWatched:
                    SortLastWatched.IsChecked = true;
                    break;
                case SortOptions.SortStartDate:
                    SortStartDate.IsChecked = true;
                    break;
                case SortOptions.SortEndDate:
                    SortEndDate.IsChecked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
            BtnOrderDescending.IsChecked = descending;
        }

        private void ButtonCustomSeasonGo(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ViewModel.CurrentlySelectedCustomSeasonSeason) && !string.IsNullOrEmpty(ViewModel.CurrentlySelectedCustomSeasonYear))
                FlyoutSeasonSelection.Hide();
        }
    }
}