namespace Engine.Core
{
    public class Transaction
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Amount { get; set; }


        public Transaction(string from, string to, int amount)
        {
            From = from;
            To = to;
            Amount = amount;
        }
    }
}