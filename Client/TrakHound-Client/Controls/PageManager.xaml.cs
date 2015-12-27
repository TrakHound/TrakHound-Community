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


        public string CurrentPageName
        {
            get { return (string)GetValue(CurrentPageNameProperty); }
            set { SetValue(CurrentPageNameProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageNameProperty =
            DependencyProperty.Register("CurrentPageName", typeof(string), typeof(PageManager), new PropertyMetadata(null));



        public object PageContent
        {
            get { return (object)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.Register("PageContent", typeof(object), typeof(PageManager), new PropertyMetadata(null));


        public void AddPage(TH_Global.Page page)
        {
            ListButton lb = new ListButton();
            lb.Image = Image_Functions.SetImageSize(page.Image, 20);
            lb.Text = page.PageName;
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

            UserControl page = LB.DataObject as UserControl;

            CurrentPageName = LB.Text;

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

                UserControl page = lb.DataObject as UserControl;

                CurrentPageName = lb.Text;

                PageContent = page;
            }
        }


    }
}
