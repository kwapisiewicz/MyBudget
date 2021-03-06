﻿using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using MyBudget.Core;
using MyBudget.OperationsLoading.Configuration;
using MyBudget.UI.Accounts;
using MyBudget.UI.Accounts.UnityConfig;
using MyBudget.UI.Configuration.UnityConfig;
using MyBudget.UI.Main;
using MyBudget.UI.Operations.UnityConfig;
using MyBudget.XmlPersistance.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyBudget.UI
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override System.Windows.DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            App.Current.MainWindow = (Window)Shell;
            App.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            RegisterModule(typeof(MainModule));
            RegisterModule(typeof(AccountsModule));
            RegisterModule(typeof(OperationsModule));
            RegisterModule(typeof(ConfigurationModule));
        }

        protected override void ConfigureContainer()
        {
            new OperationsLoadingUnityConfiguration().Configure(base.Container);
            new XmlPersistanceUnityConfiguration().Configure(base.Container);
            base.ConfigureContainer();
        }

        private void RegisterModule(Type mainModuleType)
        {
            ModuleCatalog.AddModule(new ModuleInfo()
            {
                ModuleName = mainModuleType.Name,
                ModuleType = mainModuleType.AssemblyQualifiedName,
                InitializationMode = InitializationMode.WhenAvailable
            });
        }
    }
}
