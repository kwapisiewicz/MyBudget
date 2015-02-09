﻿using MyBudget.Core.DataContext;
using MyBudget.Core.Model;
using MyBudget.Core.Persistance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MyBudget.Core.InMemoryPersistance
{
    public class XmlContext : IContext
    {
        private XmlRepositoryFactory _repositoryFactory;

        public T GetRepository<T>() where T : IRepository
        {
            return _repositoryFactory.GetRepository<T>();
        }

        IXmlSaveHandler _saveHandler;

        public XmlContext(IXmlSaveHandler saveHandler,
            XmlRepositoryFactory repositoryFactory)
        {
            if (saveHandler == null)
                throw new ArgumentNullException("saveHandler");
            if (repositoryFactory == null)
                throw new ArgumentNullException("repositoryFactory");

            _saveHandler = saveHandler;
            _repositoryFactory = repositoryFactory;

            XElement dataToLoad = _saveHandler.Load();       
            XElement accountsElement = dataToLoad.Element("ArrayOfBankAccount");           
            if (accountsElement != null)
            {
                _repositoryFactory.GetRepository<BankAccountXmlRepository>().Load(accountsElement);
            }
            XElement statementsElement = dataToLoad.Element("ArrayOfBankStatement");
            if (statementsElement != null)
            {
                _repositoryFactory.GetRepository<BankStatementXmlRepository>().Load(statementsElement);
            }
            XElement operationTypesElement = dataToLoad.Element("ArrayOfBankOperationType");
            if (operationTypesElement != null)
            {
                _repositoryFactory.GetRepository<BankOperationTypeXmlRepository>().Load(operationTypesElement);
            }
            XElement operationsElement = dataToLoad.Element("ArrayOfBankOperation");
            if (operationsElement != null)
            {
                _repositoryFactory.GetRepository<BankOperationXmlRepository>().Load(operationsElement);
            }
            XElement rulesElement = dataToLoad.Element("ArrayOfClassificationRule");
            if (rulesElement != null)
            {
                _repositoryFactory.GetRepository<ClassificationRuleXmlRepository>().Load(rulesElement);
            }
        }

        public bool SaveChanges()
        {
            XElement el = new XElement("savedData");
            el.Add(_repositoryFactory.GetRepository<BankAccountXmlRepository>().Save());
            el.Add(_repositoryFactory.GetRepository<BankStatementXmlRepository>().Save());
            el.Add(_repositoryFactory.GetRepository<BankOperationTypeXmlRepository>().Save());
            el.Add(_repositoryFactory.GetRepository<BankOperationXmlRepository>().Save());
            el.Add(_repositoryFactory.GetRepository<ClassificationRuleXmlRepository>().Save());
            _saveHandler.Save(el);

            return true;
        }

    }
}
