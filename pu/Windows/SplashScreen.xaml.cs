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
using System.Windows.Shapes;
using System.Reflection;

namespace Prism.Windows
{
    /// <summary>
    /// Логика взаимодействия для SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            var assemblyProductAttribute = ((AssemblyProductAttribute[])Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false)).Single();
            versionLabel.Content = String.Format("{0}, v.{1}", assemblyProductAttribute.Product.ToString(), Assembly.GetEntryAssembly().GetName().Version.ToString());
        }
    }
}
