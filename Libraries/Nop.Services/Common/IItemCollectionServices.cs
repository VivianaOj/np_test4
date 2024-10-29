using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Common
{
    /// <summary>
    /// Address service interface
    /// </summary>
    public partial interface IItemCollectionServices
    {

        /// <summary>
        /// Inserts itemCollection
        /// </summary>
        /// <param name="itemCollection">InsertItemCollection</param>
        void InsertItemCollection(ItemCollection itemCollection);
        
        /// <summary>
        /// Inserts itemCollection
        /// </summary>
        /// <param name="itemCollection">InsertItemCollection</param>
        ItemCollection GetItemCollectionById(int itemCollectionId);

        ItemCollection GetItemCollectionByIdTable(int itemCollectionId);

        /// <summary>
        /// Inserts itemCollection
        /// </summary>
        /// <param name="itemCollection">InsertItemCollection</param>
        void UpdateIItemCollection(ItemCollection itemCollection);

        /// <summary>
        /// Inserts itemCollection
        /// </summary>
        /// <param name="itemCollection">InsertItemCollection</param>
        void DeleteItemCollection(ItemCollection itemCollection);


        /// <summary>
        /// Inserts itemCollection
        /// </summary>
        /// <param name="itemCollection">InsertItemCollection</param>
        void InsertItemCollectionProduct(ItemCollectionProduc itemCollection);

        /// <summary>
        /// Inserts itemCollection
        /// </summary>
        /// <param name="itemCollection">InsertItemCollection</param>
        List<ItemCollectionProduc> GetItemCollectionProductById(int productId);

        List<ItemCollectionProduc> GetItemCollectionProductByCollectionId(int productId, int itemCollectionId);

        /// <summary>
        /// Inserts itemCollection
        /// </summary>
        /// <param name="itemCollection">InsertItemCollection</param>
        List<ItemCollectionCompany> GetItemCollectionCompanyById(int companyId);
        List<ItemCollectionCompany> GetItemCollectionCompanyByIdCollection(int companyId, int collectionId);
        List<ItemCollectionCompany> GetItemCollectionCompanyByNetsuiteId(int companyId);
        List<ItemCollectionCompany> GetItemCollectionCompanyByCustomerId(int companyId);
        void InsertItemCollectionCompany(ItemCollectionCompany itemCollection);

        void UpdateIItemCollectionCompany(ItemCollectionCompany itemCollection);
    }
}