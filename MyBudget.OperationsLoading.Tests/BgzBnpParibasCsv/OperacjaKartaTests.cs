﻿using Moq;
using MyBudget.Core.DataContext;
using MyBudget.Model;
using MyBudget.OperationsLoading.BgzBnpParibasCsv;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.OperationsLoading.Tests.BgzBnpParibasCsv
{
    [TestFixture]
    public class OperacjaKartaTests
    {
        private Mock<IRepository<BankAccount, string>> accountRepo;
        private Mock<IRepository<BankOperationType, string>> typeRepo;
        private Mock<IRepository<Card, string>> cardRepo;
        private Mock<IFillOperationFromDescriptionChain> fillMock;

        OperacjaKarta parser;

        [SetUp]
        public void SetUp()
        {
            this.accountRepo = new Mock<IRepository<BankAccount, string>>();
            this.typeRepo = new Mock<IRepository<BankOperationType, string>>();
            this.cardRepo = new Mock<IRepository<Card, string>>();
            this.fillMock = new Mock<IFillOperationFromDescriptionChain>();
            this.parser = new OperacjaKarta(
                new RepositoryHelper(accountRepo.Object, typeRepo.Object, cardRepo.Object),
                new ParseHelper(),
                fillMock.Object);
        }

        [Test]
        public void GivenValidDescription_WhenParsed_OperationIsFilledWithDataAndCardIsAdded()
        {
            //Given
            string description = $"OPERACJA KARTĄ ZLOTA {TestBankData.CardNo1} 000002 TRAN SAKCJA BEZGOTOWKOWA SKLEP SPOZYWCZY   23.00PLN D=04.10.2016   ";
            var operation = new BankOperation();

            //When
            this.parser.Match(operation, description);

            //Then
            Assert.AreEqual("SKLEP SPOZYWCZY", operation.Description);
            Assert.AreEqual("TRANSAKCJA KARTĄ PŁATNICZĄ", operation.Type.Name);
            Assert.AreEqual(new DateTime(2016, 10, 4), operation.OrderDate);
            cardRepo.Verify(repo => repo.Add(
                It.Is<Card>(card =>
                card.CardNumber == TestBankData.CardNo1)));
            Assert.AreEqual(TestBankData.CardNo1, operation.Card.CardNumber);
        }
    }
}
