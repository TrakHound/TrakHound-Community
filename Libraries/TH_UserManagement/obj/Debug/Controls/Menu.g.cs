﻿#pragma checksum "..\..\..\Controls\Menu.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B01CA07EC40C5DC3237AA52548200F2B"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TH_WPF;
using TH_WPF.LoadingAnimation;


namespace TH_UserManagement {
    
    
    /// <summary>
    /// Menu
    /// </summary>
    public partial class Menu : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 31 "..\..\..\Controls\Menu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid Root_GRID;
        
        #line default
        #line hidden
        
        
        #line 209 "..\..\..\Controls\Menu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox username_TXT;
        
        #line default
        #line hidden
        
        
        #line 245 "..\..\..\Controls\Menu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox password_TXT;
        
        #line default
        #line hidden
        
        
        #line 260 "..\..\..\Controls\Menu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox rememberme_CHK;
        
        #line default
        #line hidden
        
        
        #line 311 "..\..\..\Controls\Menu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border profileimage_BD;
        
        #line default
        #line hidden
        
        
        #line 344 "..\..\..\Controls\Menu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle uploadphoto_image;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TH_UserManagement;component/controls/menu.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\Menu.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 16 "..\..\..\Controls\Menu.xaml"
            ((TH_UserManagement.Menu)(target)).PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.UserControl_PreviewKeyDown);
            
            #line default
            #line hidden
            
            #line 17 "..\..\..\Controls\Menu.xaml"
            ((TH_UserManagement.Menu)(target)).Loaded += new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Root_GRID = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.username_TXT = ((System.Windows.Controls.TextBox)(target));
            
            #line 209 "..\..\..\Controls\Menu.xaml"
            this.username_TXT.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.username_TXT_TextChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.password_TXT = ((System.Windows.Controls.PasswordBox)(target));
            
            #line 245 "..\..\..\Controls\Menu.xaml"
            this.password_TXT.PasswordChanged += new System.Windows.RoutedEventHandler(this.password_TXT_PasswordChanged);
            
            #line default
            #line hidden
            
            #line 245 "..\..\..\Controls\Menu.xaml"
            this.password_TXT.GotFocus += new System.Windows.RoutedEventHandler(this.password_TXT_GotFocus);
            
            #line default
            #line hidden
            return;
            case 5:
            this.rememberme_CHK = ((System.Windows.Controls.CheckBox)(target));
            
            #line 260 "..\..\..\Controls\Menu.xaml"
            this.rememberme_CHK.Checked += new System.Windows.RoutedEventHandler(this.CheckBox_Checked);
            
            #line default
            #line hidden
            
            #line 260 "..\..\..\Controls\Menu.xaml"
            this.rememberme_CHK.Unchecked += new System.Windows.RoutedEventHandler(this.CheckBox_Unchecked);
            
            #line default
            #line hidden
            return;
            case 6:
            this.profileimage_BD = ((System.Windows.Controls.Border)(target));
            return;
            case 7:
            
            #line 320 "..\..\..\Controls\Menu.xaml"
            ((System.Windows.Controls.Border)(target)).PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.ProfileImage_PreviewMouseDown);
            
            #line default
            #line hidden
            return;
            case 8:
            this.uploadphoto_image = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 9:
            
            #line 597 "..\..\..\Controls\Menu.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Create_Clicked);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 652 "..\..\..\Controls\Menu.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Login_Clicked);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 713 "..\..\..\Controls\Menu.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.SignOut_Clicked);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 732 "..\..\..\Controls\Menu.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.MyAccount_Clicked);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

