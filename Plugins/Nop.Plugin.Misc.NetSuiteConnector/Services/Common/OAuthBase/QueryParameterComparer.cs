using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common.OAuthBase
{
    /// <summary>
    /// Comparer class used to perform the sorting of the query parameters
    /// </summary>
    public class QueryParameterComparer : IComparer<QueryParameter>
    {

        #region IComparer<QueryParameter> Members

        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
            {
                return string.Compare(x.Value, y.Value);
            }
            else
            {
                return string.Compare(x.Name, y.Name);
            }
        }

        #endregion
    }
}
