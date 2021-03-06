﻿using MyBudget.Core.DataContext;
using MyBudget.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.XmlPersistance
{
    public class BankStatementXmlRepository : AbstractXmlRepository<BankStatement, int>
    {
        public override void Add(BankStatement obj)
        {
            obj.Id = LastKey + 1;
            base.Add(obj);
        }
    }
}
