﻿using Moq;
using MyBudget.Core.DataContext;
using MyBudget.Model;
using MyBudget.OperationsLoading.BgzBnpParibas;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.OperationsLoading.Tests.BgzBnpParibas
{
    [TestFixture]
    public class BgzBnpParibasParserTests
    {
        private Mock<IRepository<BankAccount, string>> accountRepo;
        private Mock<IRepository<BankOperationType, string>> typeRepo;
        private BgzBnpParibasParser parser;

        [SetUp]
        public void SetUp()
        {
            //this.accountRepo = new Mock<IRepository<BankAccount, string>>();
            //this.typeRepo = new Mock<IRepository<BankOperationType, string>>();
            //this.parser = new MilleniumParser(new ParseHelper(), new RepositoryHelper(accountRepo.Object, typeRepo.Object));
            this.parser = new BgzBnpParibasParser();
        }

        [Test]
        public void BnpParibasParser_FilterCSVFiles()
        {
            //When
            var extensions = this.parser.SupportedFileExtensions;

            //Then
            Assert.AreEqual("BGZ BNP Paribas operations" + " " + "(.csv)|*.csv", extensions);
        }
    }
}