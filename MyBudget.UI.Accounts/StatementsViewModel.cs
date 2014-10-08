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
        PkoBpParser _parser;
        IRepository<BankOperation> _operationRepository;
        IRepository<BankStatement> _statementsRepository;
        IContext _context;

        public StatementsViewModel(IContext context, PkoBpParser parser)
        {
            _context = context;
            _parser = parser;
            _operationRepository = context.GetRepository<IRepository<BankOperation>>();
            _statementsRepository = context.GetRepository<IRepository<BankStatement>>();
            ResetListData();
            LoadFileCommand = new DelegateCommand(LoadFromFile);
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

        public ICommand LoadFileCommand { get; set; }

        public void LoadFromFile()
        {
            using (OpenFileResult file = new FileDialogService().OpenFile())
            {
                if (file.Stream == null)
                    return;

                BankStatement statement = new BankStatement()
                {
                    FileName = file.FileName,
                    LoadTime = DateTime.UtcNow,
                    Operations = new List<BankOperation>(),
                };

                _statementsRepository.Add(statement);

                foreach (var item in OnlyNew(
                    _parser.Parse(file.Stream), 
                    _operationRepository.GetAll()))
                {
                    statement.Operations.Add(item);
                    _operationRepository.Add(item);
                }
                _context.SaveChanges();
            }

            ResetListData();
        }

        public IEnumerable<BankOperation> OnlyNew(
            IEnumerable<BankOperation> toAdd, 
            IEnumerable<BankOperation> existing)
        {
            foreach (var item in toAdd)
            {
                var alreadyExist = existing.Any(a =>
                    a.OrderDate == item.OrderDate &&
                    a.ExecutionDate == item.ExecutionDate &&
                    a.Amount == item.Amount &&
                    a.EndingBalance == item.EndingBalance &&
                    a.Description == item.Description);
                if (!alreadyExist)
                {
                    yield return item;
                }
            }
        }
    }
}
