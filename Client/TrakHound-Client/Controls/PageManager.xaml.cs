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

using TH_Global;
using TH_WPF;

namespace TrakHound_Client.Controls
{
    /// <summary>
    /// Interaction logic for PageManager.xaml
    /// </summary>
    public partial class PageManager : UserControl, IPage
    {

        public PageManager()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Title { get { return TabTitle; } }

        public ImageSource Image { get { return TabImage; } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }


        public string TabTitle { get; set; }

        public ImageSource TabImage { get; set; }


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

        public IPage PageContent
        {
            get { return (IPage)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.Register("PageContent", typeof(IPage), typeof(PageManager), new PropertyMetadata(null));


        public void AddPage(IPage page)
        {
            ListButton lb = new ListButton();

            // Bind ListButton.Text to PageName property
            var pageImageBinding = new Binding();
            pageImageBinding.Source = page;
            pageImageBinding.Path = new PropertyPath("Image");
            BindingOperations.SetBinding(lb, ListButton.ImageProperty, pageImageBinding);

            // Bind ListButton.Text to PageName property
            var pageTitleBinding = new Binding();
            pageTitleBinding.Source = page;
            pageTitleBinding.Path = new PropertyPath("Title");
            BindingOperations.SetBinding(lb, ListButton.TextProperty, pageTitleBinding);

            lb.Selected += ListButton_Selected;
            lb.DataObject = page;

            Pages.Add(lb);
        }

        public void RemovePage(IPage page)
        {
            foreach (ListButton lb in Pages)
            {
                if (lb.Text.ToUpper() == page.Title.ToUpper())
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

            var page = LB.DataObject as TH_Global.IPage;
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

                var page = lb.DataObject as IPage;
                PageContent = page;
            }
        }

    }
}
