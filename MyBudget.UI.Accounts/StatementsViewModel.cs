﻿using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using MyBudget.Core;
using MyBudget.Core.DataContext;
using MyBudget.Core.ImportData;
using MyBudget.Core.Model;
using MyBudget.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace MyBudget.UI.Accounts
{
    public class StatementsViewModel : BindableBase
    {
        IEnumerable<IParser> _supportedParsers;
        IRepository<BankOperation> _operationRepository;
        IRepository<BankStatement> _statementsRepository;
        OperationsImporter _importer;
        IContext _context;

        public StatementsViewModel(IContext context, IParser[] supportedParsers)
        {
            _context = context;
            _supportedParsers = supportedParsers.OrderBy(a => a.Name);
            ChosenParser = SupportedParsers.FirstOrDefault();
            _operationRepository = context.GetRepository<IRepository<BankOperation>>();
            _statementsRepository = context.GetRepository<IRepository<BankStatement>>();
            _importer = new OperationsImporter(_operationRepository, _statementsRepository);
            ResetListData();
            LoadFileCommand = new DelegateCommand(LoadFromFile);
            LoadRawTextCommand = new DelegateCommand(LoadFromRawText, CanLoadFromRawText);
        }

        public IEnumerable<IParser> SupportedParsers
        {
            get
            {
                return _supportedParsers;
            }
        }

        private IParser _supportedParser;
        public IParser ChosenParser
        {
            get
            {
                return _supportedParser;
            }
            set
            {
                _supportedParser = value;
                OnPropertyChanged(() => SupportedParsers);
            }
        }

        private void ResetListData()
        {
            var list = new ListCollectionView(_statementsRepository.GetAll().ToList());
            Data = list;
        }

        private ListCollectionView _data;
        public ListCollectionView Data
        {
            get
            {

                return _data;
            }
            set
            {
                _data = value;
                OnPropertyChanged(() => Data);
            }
        }

        public DelegateCommand LoadFileCommand { get; set; }

        public void LoadFromFile()
        {
            using (OpenFileResult file = new FileDialogService().OpenFile(ChosenParser.SupportedFileExtensions))
            {
                if (file.Stream == null)
                    return;

                _importer.ImportOperations(file.FileName, ChosenParser.Parse(file.Stream));
                
                _context.SaveChanges();
            }

            ResetListData();
        }

        public DelegateCommand LoadRawTextCommand { get; set; }

        private string _rawStatementText;
        public string RawStatementText
        {
            get
            {
                return _rawStatementText;
            }
            set
            {
                _rawStatementText = value;
                OnPropertyChanged(() => RawStatementText);
                LoadRawTextCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanLoadFromRawText()
        {
            return !string.IsNullOrWhiteSpace(RawStatementText);
        }

        public void LoadFromRawText()
        {                        
            string statementName = "FromText" + DateTime.Now;
            _importer.ImportOperations(statementName, ChosenParser.Parse(RawStatementText));
            _context.SaveChanges();
            ResetListData();
        }
    }
}
