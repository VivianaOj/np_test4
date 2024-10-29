using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Common
{
    public partial class ItemCollectionServices : IItemCollectionServices
    {
        #region Fields
        private readonly IRepository<ItemCollection> _itemcollection;
        private readonly IRepository<ItemCollectionCompany> _itemCollectionCompany;
        private readonly IRepository<ItemCollectionProduc> _itemCollectionProduc;

        #endregion

        #region Constructor
        public ItemCollectionServices(IRepository<ItemCollection> itemcollection, IRepository<ItemCollectionCompany> itemCollectionCompany, IRepository<ItemCollectionProduc> itemCollectionProduc)
        {
            _itemcollection = itemcollection;
            _itemCollectionCompany = itemCollectionCompany;
            _itemCollectionProduc = itemCollectionProduc;
        }

        public void DeleteItemCollection(ItemCollection itemCollection)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Methods

        #region Item Collection
        public ItemCollection GetItemCollectionById(int itemCollectionId)
        {
            if (itemCollectionId == 0)
                return null;

            var query = _itemcollection.Table.Where(x => x.NetsuiteId == itemCollectionId).ToList();
            var item = query.FirstOrDefault();
            return item;
        }

        public ItemCollection GetItemCollectionByIdTable(int itemCollectionId)
        {
            if (itemCollectionId == 0)
                return null;

            var query = _itemcollection.Table.Where(x => x.Id == itemCollectionId).ToList();
            var item = query.FirstOrDefault();
            return item;
        }

        public void InsertItemCollection(ItemCollection itemCollection)
        {
            if (itemCollection == null)
                throw new ArgumentNullException(nameof(itemCollection));

            _itemcollection.Insert(itemCollection);

        }
        #endregion

        public void UpdateIItemCollection(ItemCollection itemCollection)
        {
            if (itemCollection == null)
                throw new ArgumentNullException(nameof(itemCollection));

            _itemcollection.Update(itemCollection);
        }

        #region Item Collection Product 

        public void InsertItemCollectionProduct(ItemCollectionProduc itemCollection)
        {
            if (itemCollection == null)
                throw new ArgumentNullException(nameof(itemCollection));

            _itemCollectionProduc.Insert(itemCollection);
        }

        public List<ItemCollectionProduc> GetItemCollectionProductById(int productId)
        {
            if (productId == 0)
                return null;

            var query = _itemCollectionProduc.Table.Where(x => x.ProductId == productId).ToList();
            return query;
        }

        public List<ItemCollectionProduc> GetItemCollectionProductByCollectionId(int productId, int itemCollectionId)
        {
            
            var query = _itemCollectionProduc.Table.Where(x => x.ProductId == productId && x.CollectionId== itemCollectionId).ToList();
            return query;
        }

        public List<ItemCollectionCompany> GetItemCollectionCompanyById(int companyId)
        {
            if (companyId == 0)
                return null;

            var query = _itemCollectionCompany.Table.Where(x => x.CustomerId == companyId).ToList();
            return query;
        }

        public List<ItemCollectionCompany> GetItemCollectionCompanyByIdCollection(int companyId, int collectionId)
        {
            if (companyId == 0)
                return null;

            var query = _itemCollectionCompany.Table.Where(x => x.CustomerId == companyId && x.CollectionId== collectionId).ToList();
            return query;
        }
        public List<ItemCollectionCompany> GetItemCollectionCompanyByNetsuiteId(int companyId)
        {
            if (companyId == 0)
                return null;

            var query = _itemCollectionCompany.Table.Where(x => x.CustomerNetsuiteId == companyId).ToList();
            return query;
        }

        public List<ItemCollectionCompany> GetItemCollectionCompanyByCustomerId(int companyId)
        {
            if (companyId == 0)
                return null;

            var query = _itemCollectionCompany.Table.Where(x => x.CustomerId == companyId).ToList();
            return query;
        }
        #endregion


        #region Item Collection Customer 
        public void InsertItemCollectionCompany(ItemCollectionCompany itemCollection)
        {
            if (itemCollection == null)
                throw new ArgumentNullException(nameof(itemCollection));

            _itemCollectionCompany.Insert(itemCollection);
        }

        public void UpdateIItemCollectionCompany(ItemCollectionCompany itemCollection)
        {
            if (itemCollection == null)
                throw new ArgumentNullException(nameof(itemCollection));

            _itemCollectionCompany.Update(itemCollection);
        }
        #endregion
        #endregion
    }
}
