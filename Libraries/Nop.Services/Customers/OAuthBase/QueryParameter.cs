using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Customers.OAuthBase
{
    /// <summary>
    /// Provides an internal structure to sort the query parameter
    /// </summary>
    public class QueryParameter
    {
        private string name = null;
        private string value = null;

        public QueryParameter(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name
        {
            get { return name; }
        }

        public string Value
        {
            get { return value; }
        }
    }
}
