// ReSharper disable InconsistentNaming

namespace MtApi5
{
    public class MqlTradeCheckResult
    {
        public uint Retcode { get; }               // Reply code
        public double Balance { get; }             // Balance after the execution of the deal
        public double Equity { get; }              // Equity after the execution of the deal
        public double Profit { get; }              // Floating profit
        public double Margin { get; }              // Margin requirements
        public double Margin_free { get; }         // Free margin
        public double Margin_level { get; }        // Margin level
        public string Comment { get; }             // Comment to the reply code (description of the error)

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

        public override string ToString()
        {
            return $"Retcode={Retcode}; Comment={Comment}; Balance={Balance}; Equity={Equity}; Profit={Profit}; Margin={Margin}; Margin_free={Margin_free}; Margin_level={Margin_level}";
        }
    }
}
