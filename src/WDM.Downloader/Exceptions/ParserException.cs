using System;
using System.Collections.Generic;
using System.Text;

namespace WDM.Downloaders.Exceptions
{
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message) { }
    }
}
