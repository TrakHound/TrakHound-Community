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

using System.Collections.ObjectModel;

using TH_WPF;

namespace TrakHound_Client.Controls
{
    /// <summary>
    /// Interaction logic for PageManager.xaml
    /// </summary>
    public partial class PageManager : UserControl
    {

        public PageManager()
        {
            InitializeComponent();
            DataContext = this;
        }

        ObservableCollection<ListButton> pages;
        public ObservableCollection<ListButton> Pages
        {
            get
            {
                if (pages == null)
                    pages = new ObservableCollection<ListButton>();
                return pages;
            }

            set
            {
                pages = value;
            }
        }

        public TH_Global.Page PageContent
        {
            get { return (TH_Global.Page)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.Register("PageContent", typeof(TH_Global.Page), typeof(PageManager), new PropertyMetadata(null));


        public void AddPage(TH_Global.Page page)
        {
            ListButton lb = new ListButton();

            // Bind ListButton.Text to PageName property
            var pageImageBinding = new Binding();
            pageImageBinding.Source = page;
            pageImageBinding.Path = new PropertyPath("Image");
            BindingOperations.SetBinding(lb, ListButton.ImageProperty, pageImageBinding);

            // Bind ListButton.Text to PageName property
            var pageNameBinding = new Binding();
            pageNameBinding.Source = page;
            pageNameBinding.Path = new PropertyPath("PageName");
            BindingOperations.SetBinding(lb, ListButton.TextProperty, pageNameBinding);

            lb.Selected += ListButton_Selected;
            lb.DataObject = page;

            Pages.Add(lb);
        }

        public void RemovePage(TH_Global.Page page)
        {
            foreach (ListButton lb in Pages)
            {
                if (lb.Text.ToUpper() == page.PageName.ToUpper())
                {
                    Pages.Remove(lb);
                }
            } 
        }

        private void ListButton_Selected(ListButton LB)
        {
            foreach (ListButton oLB in Pages)
            {
                if (oLB == LB) oLB.IsSelected = true;
                else oLB.IsSelected = false;
            }

            var page = LB.DataObject as TH_Global.Page;
            PageContent = page;
        }

        private void pagemanager_Loaded(object sender, RoutedEventArgs e)
        {
            if (Pages.Count > 0)
            {
                ListButton lb = (ListButton)Pages[0];

                foreach (ListButton oLB in Pages)
                {
                    if (oLB == lb) oLB.IsSelected = true;
                    else oLB.IsSelected = false;
                }

                var page = lb.DataObject as TH_Global.Page;
                PageContent = page;
            }
        }


    }
}
