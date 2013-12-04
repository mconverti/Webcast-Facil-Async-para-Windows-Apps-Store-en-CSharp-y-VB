//// The Split Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234234

namespace MosaicMaker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using MosaicMaker.Data;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// A page that displays a group title, a list of items within the group, and details for the
    /// currently selected item.
    /// </summary>
    public sealed partial class SplitPage : MosaicMaker.Common.LayoutAwarePage
    {
        public SplitPage()
        {
            this.InitializeComponent();
        }

        private async void itemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var model = e.ClickedItem as MosaicModel;
            if (model == null) return;
            itemListView.SelectedItem = model;

            model.StatusText = "Eligiendo imagen...";
            var picker = new FileOpenPicker
            {
                FileTypeFilter = { ".jpg", ".jpeg", ".png" },
                ViewMode = PickerViewMode.Thumbnail
            };

            var originalFile = await picker.PickSingleFileAsync();

            using (var originalStream = await originalFile.OpenAsync(FileAccessMode.Read))
            {
                var originalBitmap = new BitmapImage();
                await originalBitmap.SetSourceAsync(originalStream);
                model.OriginalImage = originalBitmap;
            }

            model.StatusText = "Descargando imágenes para el mosaico...";
            var uri = FlickrTileProvider.GetFlickrUri(model.Title);
            var response = await new HttpClient().GetAsync(uri);
            var xml = await response.Content.ReadAsStringAsync();
            var photos = FlickrTileProvider.ParsePhotosFromXML(xml);
            var tileList = await FlickrTileProvider.FetchImagesAsync(photos);

            model.StatusText = "Calculando mosaico...";
            var mosaic = await MosaicBuilder.CreateMosaicAsync(originalFile, tileList);

            using (var mosaicStream = await mosaic.OpenAsync(FileAccessMode.Read))
            {
                var mosaicBitmap = new BitmapImage();
                await mosaicBitmap.SetSourceAsync(mosaicStream);
                model.MosaicImage = mosaicBitmap;
            }

            model.StatusText = "Terminado!";
        }

        protected override void GoBack(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property provides the group to be displayed.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //var group = (SampleDataGroup)e.Parameter;
            //this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = e.Parameter;

            // Select the first item automatically unless logical page navigation is being used
            // (see the logical page navigation #region below.)
            if (!this.UsingLogicalPageNavigation()) this.itemsViewSource.View.MoveCurrentToFirst();
        }

        #region Page state management

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            //var group = SampleDataSource.GetGroup((String)navigationParameter);
            //this.DefaultViewModel["Group"] = group;
            //this.DefaultViewModel["Items"] = group.Items;
            this.DefaultViewModel["Items"] = navigationParameter;

            if (pageState == null)
            {
                // When this is a new page, select the first item automatically unless logical page
                // navigation is being used (see the logical page navigation #region below.)
                if (!this.UsingLogicalPageNavigation() && this.itemsViewSource.View != null)
                {
                    this.itemsViewSource.View.MoveCurrentToFirst();
                }
            }
            else
            {
                // Restore the previously saved state associated with this page
                if (pageState.ContainsKey("SelectedItem") && this.itemsViewSource.View != null)
                {
                    var selectedItem = SampleDataSource.GetItem((String)pageState["SelectedItem"]);
                    this.itemsViewSource.View.MoveCurrentTo(selectedItem);
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            if (this.itemsViewSource.View != null)
            {
                var selectedItem = (SampleDataItem)this.itemsViewSource.View.CurrentItem;
                if (selectedItem != null) pageState["SelectedItem"] = selectedItem.UniqueId;
            }
        }

        #endregion

        #region Logical page navigation

        // Visual state management typically reflects the four application view states directly
        // (full screen landscape and portrait plus snapped and filled views.)  The split page is
        // designed so that the snapped and portrait view states each have two distinct sub-states:
        // either the item list or the details are displayed, but not both at the same time.
        //
        // This is all implemented with a single physical page that can represent two logical
        // pages.  The code below achieves this goal without making the user aware of the
        // distinction.

        /// <summary>
        /// Invoked to determine whether the page should act as one logical page or two.
        /// </summary>
        /// <param name="viewState">The view state for which the question is being posed, or null
        /// for the current view state.  This parameter is optional with null as the default
        /// value.</param>
        /// <returns>True when the view state in question is portrait or snapped, false
        /// otherwise.</returns>
        private bool UsingLogicalPageNavigation(ApplicationViewState? viewState = null)
        {
            if (viewState == null) viewState = ApplicationView.Value;
            return viewState == ApplicationViewState.FullScreenPortrait ||
                viewState == ApplicationViewState.Snapped;
        }

        /// <summary>
        /// Invoked when an item within the list is selected.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is Snapped)
        /// displaying the selected item.</param>
        /// <param name="e">Event data that describes how the selection was changed.</param>
        void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Invalidate the view state when logical page navigation is in effect, as a change
            // in selection may cause a corresponding change in the current logical page.  When
            // an item is selected this has the effect of changing from displaying the item list
            // to showing the selected item's details.  When the selection is cleared this has the
            // opposite effect.
            if (this.UsingLogicalPageNavigation()) this.InvalidateVisualState();
        }

        /// <summary>
        /// Invoked when the page's back button is pressed.
        /// </summary>
        /// <param name="sender">The back button instance.</param>
        /// <param name="e">Event data that describes how the back button was clicked.</param>
        //protected override void GoBack(object sender, RoutedEventArgs e)
        //{
        //    if (this.UsingLogicalPageNavigation() && itemListView.SelectedItem != null)
        //    {
        //        // When logical page navigation is in effect and there's a selected item that
        //        // item's details are currently displayed.  Clearing the selection will return
        //        // to the item list.  From the user's point of view this is a logical backward
        //        // navigation.
        //        this.itemListView.SelectedItem = null;
        //    }
        //    else
        //    {
        //        // When logical page navigation is not in effect, or when there is no selected
        //        // item, use the default back button behavior.
        //        base.GoBack(sender, e);
        //    }
        //}

        /// <summary>
        /// Invoked to determine the name of the visual state that corresponds to an application
        /// view state.
        /// </summary>
        /// <param name="viewState">The view state for which the question is being posed.</param>
        /// <returns>The name of the desired visual state.  This is the same as the name of the
        /// view state except when there is a selected item in portrait and snapped views where
        /// this additional logical page is represented by adding a suffix of _Detail.</returns>
        protected override string DetermineVisualState(ApplicationViewState viewState)
        {
            // Update the back button's enabled state when the view state changes
            var logicalPageBack = this.UsingLogicalPageNavigation(viewState) && this.itemListView.SelectedItem != null;
            var physicalPageBack = this.Frame != null && this.Frame.CanGoBack;
            this.DefaultViewModel["CanGoBack"] = logicalPageBack || physicalPageBack;

            // Determine visual states for landscape layouts based not on the view state, but
            // on the width of the window.  This page has one layout that is appropriate for
            // 1366 virtual pixels or wider, and another for narrower displays or when a snapped
            // application reduces the horizontal space available to less than 1366.
            if (viewState == ApplicationViewState.Filled ||
                viewState == ApplicationViewState.FullScreenLandscape)
            {
                var windowWidth = Window.Current.Bounds.Width;
                if (windowWidth >= 1366) return "FullScreenLandscapeOrWide";
                return "FilledOrNarrow";
            }

            // When in portrait or snapped start with the default visual state name, then add a
            // suffix when viewing details instead of the list
            var defaultStateName = base.DetermineVisualState(viewState);
            return logicalPageBack ? defaultStateName + "_Detail" : defaultStateName;
        }

        #endregion

        // If demoing live at a conference, we're not going to rely on having internet.
        public class HttpClient
        {
            public async Task<HttpResponse> GetAsync(string uri) { await Task.Delay(1); return new HttpResponse() { uri = uri }; }
            public async Task<HttpResponse> GetAsync(string uri, CancellationToken cancel) { await Task.Delay(1); return new HttpResponse() { uri = uri }; }
        }

        public class HttpResponse
        {
            public string uri;
            public HttpContent Content { get { return new HttpContent { uri = uri }; } }
        }

        public class HttpContent
        {
            public string uri;
            public async Task<string> ReadAsStringAsync()
            {
                var fn = new Uri(uri).Query.Replace("?", "").Replace("&", "").Replace("=", "").Replace(".", "") + ".xml";
                var folder = MosaicBuilder.DownloadFolder;
                try
                {
                    var file = await folder.GetFileAsync(fn);
                    using (var stream = await file.OpenStreamForReadAsync())
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                }

                var r = await new System.Net.Http.HttpClient().GetAsync(uri);
                var s = await r.Content.ReadAsStringAsync();

                var file2 = await folder.CreateFileAsync(fn);
                using (var stream = await file2.OpenStreamForWriteAsync())
                using (var writer = new System.IO.StreamWriter(stream))
                {
                    await writer.WriteAsync(s);
                }

                return s;
            }
        }
    }
}
