﻿#pragma checksum "..\..\..\Controls\LswControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "FFA9F2AAEC9743EE9910328743175A22"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.18051
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
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


namespace Prism.Units.Controls {
    
    
    /// <summary>
    /// LswControl
    /// </summary>
    public partial class LswControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 18 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock titleText;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock descText;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button onButton;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button offButton;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button spareButton;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button qsOnButton;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button qfOnButton;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button spareOnButton;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button qsOffButton;
        
        #line default
        #line hidden
        
        
        #line 95 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button qfOffButton;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button spareOffButton;
        
        #line default
        #line hidden
        
        
        #line 109 "..\..\..\Controls\LswControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid errorMessagBlock;
        
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
            System.Uri resourceLocater = new System.Uri("/u12;component/controls/lswcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\LswControl.xaml"
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
            
            #line 8 "..\..\..\Controls\LswControl.xaml"
            ((Prism.Units.Controls.LswControl)(target)).Loaded += new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.titleText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.descText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.onButton = ((System.Windows.Controls.Button)(target));
            return;
            case 5:
            this.offButton = ((System.Windows.Controls.Button)(target));
            return;
            case 6:
            this.spareButton = ((System.Windows.Controls.Button)(target));
            return;
            case 7:
            this.qsOnButton = ((System.Windows.Controls.Button)(target));
            
            #line 65 "..\..\..\Controls\LswControl.xaml"
            this.qsOnButton.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.qsOnButton_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 8:
            this.qfOnButton = ((System.Windows.Controls.Button)(target));
            
            #line 70 "..\..\..\Controls\LswControl.xaml"
            this.qfOnButton.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.qfOnButton_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 9:
            this.spareOnButton = ((System.Windows.Controls.Button)(target));
            
            #line 75 "..\..\..\Controls\LswControl.xaml"
            this.spareOnButton.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.spareOnButton_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 10:
            this.qsOffButton = ((System.Windows.Controls.Button)(target));
            
            #line 90 "..\..\..\Controls\LswControl.xaml"
            this.qsOffButton.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.qsOffButton_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 11:
            this.qfOffButton = ((System.Windows.Controls.Button)(target));
            
            #line 95 "..\..\..\Controls\LswControl.xaml"
            this.qfOffButton.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.qfOffButton_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 12:
            this.spareOffButton = ((System.Windows.Controls.Button)(target));
            
            #line 100 "..\..\..\Controls\LswControl.xaml"
            this.spareOffButton.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.spareOffButton_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 13:
            this.errorMessagBlock = ((System.Windows.Controls.Grid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

