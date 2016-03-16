using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TH_UserManagement.Management;

namespace TH_DeviceManager.AddDevice.Controls
{
    /// <summary>
    /// Interaction logic for SharedList.xaml
    /// </summary>
    public partial class CatalogItem : UserControl
    {
        public CatalogItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string TableName { get; set; }

        public Shared.SharedListItem SharedListItem
        {
            get { return (Shared.SharedListItem)GetValue(SharedListItemProperty); }
            set { SetValue(SharedListItemProperty, value); }
        }

        public static readonly DependencyProperty SharedListItemProperty =
            DependencyProperty.Register("SharedListItem", typeof(Shared.SharedListItem), typeof(CatalogItem), new PropertyMetadata(null));


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(CatalogItem), new PropertyMetadata(false));


        #region "Images"

        public ImageSource FullSizeImage
        {
            get { return (ImageSource)GetValue(FullSizeImageProperty); }
            set { SetValue(FullSizeImageProperty, value); }
        }

        public static readonly DependencyProperty FullSizeImageProperty =
            DependencyProperty.Register("FullSizeImage", typeof(ImageSource), typeof(CatalogItem), new PropertyMetadata(null));


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(CatalogItem), new PropertyMetadata(null));

        #endregion

        #region "Device Description"

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(CatalogItem), new PropertyMetadata(null));

        public string Manufacturer
        {
            get { return (string)GetValue(ManufacturerProperty); }
            set { SetValue(ManufacturerProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerProperty =
            DependencyProperty.Register("Manufacturer", typeof(string), typeof(CatalogItem), new PropertyMetadata(null));

        public string DeviceType
        {
            get { return (string)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }

        public static readonly DependencyProperty DeviceTypeProperty =
            DependencyProperty.Register("DeviceType", typeof(string), typeof(CatalogItem), new PropertyMetadata(null));

        public string Model
        {
            get { return (string)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(string), typeof(CatalogItem), new PropertyMetadata(null));

        public string Controller
        {
            get { return (string)GetValue(ControllerProperty); }
            set { SetValue(ControllerProperty, value); }
        }

        public static readonly DependencyProperty ControllerProperty =
            DependencyProperty.Register("Controller", typeof(string), typeof(CatalogItem), new PropertyMetadata(null));


        #endregion

        #region "Author Information"

        public bool IsOwner
        {
            get { return (bool)GetValue(IsOwnerProperty); }
            set { SetValue(IsOwnerProperty, value); }
        }

        public static readonly DependencyProperty IsOwnerProperty =
            DependencyProperty.Register("IsOwner", typeof(bool), typeof(CatalogItem), new PropertyMetadata(false));


        public string Author
        {
            get { return (string)GetValue(AuthorProperty); }
            set { SetValue(AuthorProperty, value); }
        }

        public static readonly DependencyProperty AuthorProperty =
            DependencyProperty.Register("Author", typeof(string), typeof(CatalogItem), new PropertyMetadata(null));


        public string LastUpdated
        {
            get { return (string)GetValue(LastUpdatedProperty); }
            set { SetValue(LastUpdatedProperty, value); }
        }

        public static readonly DependencyProperty LastUpdatedProperty =
            DependencyProperty.Register("LastUpdated", typeof(string), typeof(CatalogItem), new PropertyMetadata(null));

        #endregion


        public delegate void Clicked_Handler(CatalogItem item);
        public event Clicked_Handler AddClicked;

        private void Add_Clicked(TH_WPF.Button bt)
        {
            if (AddClicked != null) AddClicked(this);
        }

        public event Clicked_Handler Clicked;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }

    }
}
