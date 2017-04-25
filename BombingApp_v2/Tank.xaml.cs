using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BombingApp_v2
{
    public sealed partial class Tank : UserControl
    {
        public double LocationX { get; set; }
        public double LocationY { get; set; }

        public Tank()
        {
            this.InitializeComponent();
            Width = 70;
            Height = 70;
        }
        public void Move()
        {
            LocationX += 1;
            SetLocation();
        }

        public void SetLocation()
        {
            SetValue(Canvas.LeftProperty, LocationX);
            SetValue(Canvas.TopProperty, LocationY);

        }
    }
}
