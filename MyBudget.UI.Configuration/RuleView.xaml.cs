﻿using Microsoft.Practices.Prism.Regions;
using MyBudget.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyBudget.UI.Configuration
{
    /// <summary>
    /// Interaction logic for RuleView.xaml
    /// </summary>
    public partial class RuleView : UserControl, INavigationAware, IRegionMemberLifetime
    {
        public RuleView()
        {
        }

        public RuleView(RuleViewModel viewModel)
        {
            ViewModel = viewModel;
            this.DataContext = viewModel;
            InitializeComponent();
        }

        public RuleViewModel ViewModel { get; set; }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel.Journal = navigationContext.NavigationService.Journal;
            ClassificationDefinition selected = navigationContext.Parameters["selected"] as ClassificationDefinition;
            BankOperation patternParameter = navigationContext.Parameters["template"] as BankOperation;
            ViewModel.OnNavigatedTo(selected, patternParameter);
        }

        public bool KeepAlive
        {
            get { return false; }
        }
    }
}
