// ReSharper disable InconsistentNaming

namespace MtApi5
{
    public class MqlTradeCheckResult
    {
        public uint Retcode { get; set; }               // Reply code
        public double Balance { get; set; }             // Balance after the execution of the deal
        public double Equity { get; set; }              // Equity after the execution of the deal
        public double Profit { get; set; }              // Floating profit
        public double Margin { get; set; }              // Margin requirements
        public double Margin_free { get; set; }         // Free margin
        public double Margin_level { get; set; }        // Margin level
        public string Comment { get; set; }             // Comment to the reply code (description of the error)

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

        public MqlTradeCheckResult()
        { }

        public override string ToString()
        {
            return $"Retcode={Retcode}; Comment={Comment}; Balance={Balance}; Equity={Equity}; Profit={Profit}; Margin={Margin}; Margin_free={Margin_free}; Margin_level={Margin_level}";
        }
    }
}
