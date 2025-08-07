using System;

namespace OneXMM.Referential.DataFeeds.Events
{
    public sealed class QuoteEventArgs : EventArgs
    {
        public Quote[] Quotes { get; }

        public QuoteEventArgs(Quote[] quotes) 
        {
            Quotes = quotes;
        }
    }
}
