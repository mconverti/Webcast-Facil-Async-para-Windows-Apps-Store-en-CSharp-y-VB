using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MosaicMaker
{
    class MosaicModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _displayTitle = string.Empty;
        public string DisplayTitle
        {
            get
            {
                return this._displayTitle;
            }

            set
            {
                if (this._displayTitle != value)
                {
                    this._displayTitle = value;
                    this.OnPropertyChanged("DisplayTitle");
                }
            }
        }

        private string _query = string.Empty;
        public string Title
        {
            get
            {
                return this._query;
            }

            set
            {
                if (this._query != value)
                {
                    this._query = value;
                    this.OnPropertyChanged("Title");
                }
            }
        }

        private ImageSource _thumbnail;
        public ImageSource Image
        {
            get
            {
                return this._thumbnail;
            }

            set
            {
                if (this._thumbnail != value)
                {
                    this._thumbnail = value;
                    this.OnPropertyChanged("Image");
                }
            }
        }

        private ImageSource _original;
        public ImageSource OriginalImage
        {
            get
            {
                return this._original;
            }

            set
            {
                if (this._original != value)
                {
                    this._original = value;
                    this.OnPropertyChanged("OriginalImage");
                }
            }
        }

        private ImageSource _mosaic;
        public ImageSource MosaicImage
        {
            get
            {
                return this._mosaic;
            }

            set
            {
                if (this._mosaic != value)
                {
                    this._mosaic = value;
                    this.OnPropertyChanged("MosaicImage");
                }
            }
        }

        private string _status;
        public string StatusText
        {
            get
            {
                return this._status;
            }

            set
            {
                if (this._status != value)
                {
                    this._status = value;
                    this.OnPropertyChanged("StatusText");
                }
            }
        }

        public static List<MosaicModel> CreateSample(Uri baseUri)
        {
            return new List<MosaicModel> 
            {
                new MosaicModel
                {
                    DisplayTitle = "flor",
                    Title = "flower",
                    Image = new BitmapImage(new Uri(baseUri, "Pictures/Chrysanthemum.jpg")),
                    //OriginalImage = new BitmapImage(new Uri(baseUri, "Pictures/Tulips.jpg")),
                    //MosaicImage = new BitmapImage(new Uri(baseUri, "Pictures/Tulips.jpg"))
                },
                new MosaicModel
                {
                    DisplayTitle = "pájaro",
                    Title = "bird",
                    Image = new BitmapImage(new Uri(baseUri, "Pictures/Penguins.jpg")),
                    //OriginalImage = new BitmapImage(new Uri(baseUri, "Pictures/Toco Toucan (1).jpg")),
                    //MosaicImage = new BitmapImage(new Uri(baseUri, "Pictures/Toco Toucan (1).jpg"))
                },
                new MosaicModel
                {
                    DisplayTitle = "paisaje",
                    Title = "landscape",
                    Image = new BitmapImage(new Uri(baseUri, "Pictures/Waterfall.jpg")),
                    //OriginalImage = new BitmapImage(new Uri(baseUri, "Pictures/Desert Landscape (1).jpg")),
                    //MosaicImage = new BitmapImage(new Uri(baseUri, "Pictures/Desert Landscape (1).jpg"))
                },
                new MosaicModel
                {
                    DisplayTitle = "koala",
                    Title = "koala",
                    Image = new BitmapImage(new Uri(baseUri, "Pictures/Koala.jpg")),
                    //OriginalImage = new BitmapImage(new Uri(baseUri, "Pictures/Green Sea Turtle (1).jpg")),
                    //MosaicImage = new BitmapImage(new Uri(baseUri, "Pictures/Green Sea Turtle (1).jpg"))
                },
            };
        }
    }
}
