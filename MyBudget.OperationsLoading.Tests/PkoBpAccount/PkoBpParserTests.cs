﻿using Moq;
using MyBudget.Core.DataContext;
using MyBudget.Model;
using MyBudget.OperationsLoading;
using MyBudget.OperationsLoading.PkoBpAccount;
using MyBudget.OperationsLoading.Tests.Resources;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyBudget.OperationsLoading.Tests.PkoBpAccount
{
    [TestFixture]
    public class PkoBpParserTests
    {
        Mock<IRepository<BankAccount, string>> accountRepo;
        Mock<IRepository<BankOperationType, string>> typeRepo;
        Mock<IRepository<Card, string>> cardRepo;
        PkoBpParser parser;

        [SetUp]
        public void SetUp()
        {
            this.accountRepo = new Mock<IRepository<BankAccount, string>>();
            this.typeRepo = new Mock<IRepository<BankOperationType, string>>();
            this.cardRepo = new Mock<IRepository<Card, string>>();
            this.parser = new PkoBpParser(new ParseHelper(), new RepositoryHelper(accountRepo.Object, typeRepo.Object, cardRepo.Object));
        }

        [Test]
        public void GivenEmptyRepositoriesAndPkoBpXmlTextWith1Entry_WhenParse_ThenBankAccountAddedAndListOf1EntryReturned()
        {
            //Given            
            string pkoBpList = TestFiles.PkoBpParser_1Entry;
            
            //When
            var list = this.parser.Parse(pkoBpList);

            //Then
            Assert.AreEqual(1, list.Count());
            Assert.IsTrue(list.Any(a => a.Title == "SomeTitle"));
            accountRepo.Verify(a => a.Add(It.IsAny<BankAccount>()));
            typeRepo.Verify(a => a.Add(It.IsAny<BankOperationType>()));
        }

        [Test]
        public void GivenEmptyRepositoriesAndPkoBpXmlFileWith1Entry_WhenParse_ThenAccountAndTypeAddedAndListOf1EntryReturned()
        {
            //Given
            using (Stream pkoBpList = TestBankData.ToStream(TestFiles.PkoBpParser_1Entry))
            {
                //When
                var list = this.parser.Parse(pkoBpList).ToArray();

                //Then
                Assert.AreEqual(1, list.Count());
                accountRepo.Verify(a => a.Add(It.IsAny<BankAccount>()));
                typeRepo.Verify(a => a.Add(It.IsAny<BankOperationType>()));
            }
        }

        [Test]
        public void GivenWithdrawalAndCardOperation_WhenParse_TitleIsTakenFromLocation()
        {
            //Given            
            string pkoBpList = TestFiles.PkoBpParser_ZerosInTitle;

            //When
            var list = this.parser.Parse(pkoBpList).ToArray();

            //Then
            Assert.AreEqual(2, list.Count());
            Assert.IsTrue(list.Any(a => a.Title == "U AAA 222 Miasto: CityOfSth Kraj: POLSKA"));
            Assert.IsTrue(list.Any(a => a.Title == "Supermarket ABC Miasto: CityOfSth Kraj: POLSKA"));
        }

        [Test]
        public void GivenSamplePkoBpXmlWithOneOperation_WhenParsed_ThenSingleOperationParsedWithProperValuesInFields()
        {
            //When
            var list = this.parser.Parse(TestFiles.PkoBpParser_1Entry);

            //Then
            var op = list.Single();
            Assert.AreEqual(TestBankData.PKOBP_Belchatow_TestAccount1.Compact(), op.BankAccount.Number);
            Assert.AreEqual(new DateTime(2013, 2, 2), op.OrderDate);
            Assert.AreEqual(new DateTime(2013, 2, 2), op.ExecutionDate);
            Assert.AreEqual(-100.00, op.Amount);
            Assert.AreEqual("Przelew z rachunku", op.Type.Name);
            Assert.AreEqual(true, op.Cleared);
            string expectedDesc = $"Nr rach. przeciwst.: {TestBankData.ExternalAccount_TestAccount1}\r\nDane adr. rach. przeciwst.: Name\r\nTytuł: SomeTitle";
            Assert.AreEqual(expectedDesc, op.Description);
            Assert.AreEqual(TestBankData.ExternalAccount_TestAccount1.Compact(), op.CounterAccount);
        }
    }
}
