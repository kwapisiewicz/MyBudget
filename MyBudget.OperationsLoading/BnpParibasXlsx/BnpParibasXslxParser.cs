using MyBudget.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyBudget.OperationsLoading.BnpParibasXlsx
{
    public class BnpParibasXslxParser : IParser
    {
        public string Name => Resources.BnpParibasXslxName;

        public string SupportedFileExtensions => Resources.BnpParibasXslxFilter;

        private IRepositoryHelper _repositoryHelper;
        private IOperationHandler _operationHandler;

        public BnpParibasXslxParser(IRepositoryHelper repositoryHelper, IOperationHandler operationHandler)
        {
            if (repositoryHelper == null)
                throw new ArgumentNullException("repositoryHelper");
            if(operationHandler==null)
                throw new ArgumentNullException("operationHandler");
            _repositoryHelper = repositoryHelper;
            _operationHandler = operationHandler;
        }

        public IEnumerable<BankOperation> Parse(string inputString)
        {
            return Parse(ToStream(inputString));
        }

        public Stream ToStream(string text)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public Dictionary<string, int> version1 = new Dictionary<string, int>()
        {
            { "OrderDate",1 },
            { "ExecutionDate",2 },
            {"Amount",3 },
            {"Description",6 },
            {"Type",8 },
            {"BankAccountProduct",7 },
            {"CounterpartyInfo",5 }
        };

        public Dictionary<string, int> version2 = new Dictionary<string, int>()
        {
            { "OrderDate",1 },
            { "ExecutionDate",2 },
            {"Amount",4 },
            {"Description",7 },
            {"Type",9 },
            {"BankAccountProduct",8 },
            {"CounterpartyInfo",6 }
        };

        public IEnumerable<BankOperation> Parse(Stream stream)
        {
            List<BankOperation> ops = new List<BankOperation>();
            using (ExcelPackage xlPackage = new ExcelPackage(stream))
            {
                var myWorksheet = xlPackage.Workbook.Worksheets.First(); //select sheet here
                var totalRows = myWorksheet.Dimension.End.Row;
                var totalColumns = myWorksheet.Dimension.End.Column;

                var sb = new StringBuilder(); //this is your data
                for (int rowNum = 2; rowNum <= totalRows; rowNum++) //select starting row here
                {
                    var bankOperation = new BankOperation();
                    bankOperation.LpOnStatement = rowNum;
                    bankOperation.OrderDate = GetDateFromExcelRange(myWorksheet.Cells[rowNum, version2["OrderDate"]]);
                    bankOperation.ExecutionDate = GetDateFromExcelRange(myWorksheet.Cells[rowNum, version2["ExecutionDate"]]);
                    bankOperation.Amount = Convert.ToDecimal(myWorksheet.Cells[rowNum, version2["Amount"]].Value);
                    bankOperation.Description = myWorksheet.Cells[rowNum, version2["Description"]].Value.ToString();
                    bankOperation.Type = _repositoryHelper.GetOrAddOperationType(myWorksheet.Cells[rowNum, version2["Type"]].Value.ToString());
                    bankOperation.Cleared = true;

                    var bankAccountProduct = myWorksheet.Cells[rowNum, version2["BankAccountProduct"]].Value.ToString();                    
                    var accountNumber = bankAccountProduct.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[1];
                    bankOperation.BankAccount = _repositoryHelper.GetOrAddAccount(accountNumber);

                    //Parsing title and other details for different transactions
                    var counterpartyInfo = myWorksheet.Cells[rowNum, version2["CounterpartyInfo"]].Value.ToString();
                    _operationHandler.Handle(bankOperation, bankOperation.Description, counterpartyInfo);

                    ops.Add(bankOperation);
                }
            }

            return ops;
        }

        public DateTime GetDateFromExcelRange(ExcelRange excelRange)
        {
            double dateValue = (double) excelRange.Value;
            DateTime date = DateTime.FromOADate(dateValue);
            return date;
        }
    }
}
