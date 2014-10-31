using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtApi5
{
    public class MqlTradeCheckResult
    {
        public uint Retcode { get; private set; }             // Reply code
        public double Balance { get; private set; }             // Balance after the execution of the deal
        public double Equity { get; private set; }              // Equity after the execution of the deal
        public double Profit { get; private set; }              // Floating profit
        public double Margin { get; private set; }              // Margin requirements
        public double Margin_free { get; private set; }         // Free margin
        public double Margin_level { get; private set; }        // Margin level
        public string Comment { get; private set; }             // Comment to the reply code (description of the error)

        public MqlTradeCheckResult(uint retcode
            , double balance
            , double equity
            , double profit
            , double margin
            , double margin_free
            , double margin_level
            , string comment)
        {
            Retcode = retcode;
            Balance = balance;
            Equity = equity;
            Profit = profit;
            Margin = margin;
            Margin_free = margin_free;
            Margin_level = margin_level;
            Comment = comment;
        }
    }
}
