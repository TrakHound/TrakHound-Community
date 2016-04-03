﻿#pragma checksum "..\..\DeviceList.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "01F50447A6C2F085FA41A7ADCFD45D18"
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
using TH_DeviceManager.Controls;
using TH_WPF;
using TH_WPF.LoadingAnimation;


namespace TH_DeviceManager {
    
    
    /// <summary>
    /// DeviceList
    /// </summary>
    public partial class DeviceList : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 240 "..\..\DeviceList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid Devices_DG;
        
        #line default
        #line hidden
        
        
        #line 324 "..\..\DeviceList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid SharedDevices_DG;
        
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
            System.Uri resourceLocater = new System.Uri("/TH_DeviceManager;component/devicelist.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\DeviceList.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
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
            
            #line 119 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Add_Toolbar_Clicked);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 125 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Refresh_Toolbar_Clicked);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 155 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.MoveUp_Toolbar_Clicked);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 158 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.MoveDown_Toolbar_Clicked);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 178 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Remove_Toolbar_Clicked);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 184 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Edit_Toolbar_Clicked);
            
            #line default
            #line hidden
            return;
            case 7:
            this.Devices_DG = ((System.Windows.Controls.DataGrid)(target));
            
            #line 243 "..\..\DeviceList.xaml"
            this.Devices_DG.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.Devices_DG_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 300 "..\..\DeviceList.xaml"
            ((System.Windows.Controls.Grid)(target)).PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Grid_PreviewMouseDown);
            
            #line default
            #line hidden
            return;
            case 10:
            this.SharedDevices_DG = ((System.Windows.Controls.DataGrid)(target));
            
            #line 327 "..\..\DeviceList.xaml"
            this.SharedDevices_DG.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.SharedDevices_DG_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 382 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Add_Toolbar_Clicked);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 8:
            
            #line 264 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Edit_Clicked);
            
            #line default
            #line hidden
            break;
            case 11:
            
            #line 336 "..\..\DeviceList.xaml"
            ((TH_WPF.Button)(target)).Clicked += new TH_WPF.Button.Clicked_Handler(this.Edit_Clicked);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

