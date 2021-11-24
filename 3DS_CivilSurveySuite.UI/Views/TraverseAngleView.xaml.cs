﻿using System.Windows;
using _3DS_CivilSurveySuite.UI.ViewModels;

namespace _3DS_CivilSurveySuite.UI.Views
{
    /// <summary>
    /// Interaction logic for TraverseAngleView.xaml
    /// </summary>
    public partial class TraverseAngleView : Window
    {
        public TraverseAngleView(TraverseAngleViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}