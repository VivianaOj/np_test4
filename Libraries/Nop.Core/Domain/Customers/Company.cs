using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public partial class Company : BaseEntity
    {
        private ICollection<CompanyAddresses> _companyAddressMappings;
        private ICollection<CompanyCustomerMapping> _companyCustomerMappings;
        private ICollection<ItemPricing> _itemsPricing;

        public int CompanyId { get; set; }

        public string CompanyName { get; set; }
        public decimal Balance { get; set; }
        public decimal OverdueBalance { get; set; }
        public int DaysOverdue { get; set; }
        public decimal ConsoleBalance { get; set; }

        
        public string PrimaryLocation { get; set; }
        public string Type { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string EmailsForBilling { get; set; }
        public string Terms { get; set; }

        public int? BillingAddressId { get; set; }

        public int? ShippingAddressId { get; set; }

        public string BillingTerms { get; set; }


        public string DefaultAddress { get; set; }

        public string EntityStatus { get; set; }
        public string EntityStatusId { get; set; }

        public string NetsuiteId { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public int? PriceLevelId { get; set; }

        public int Parent_Id { get; set; }

        public bool PONumberReq { get; set; }
        public bool custentity_tj_exempt_customer { get; set; }
        public string custentity_tj_exempt_customer_states { get; set; }

        #region NavigationProperties

        public virtual Address BillingAddress { get; set; }

        public virtual Address ShippingAddress { get; set; }

        public IList<Address> Addresses => CompanyAddressMappings.Select(mapping => mapping.Address).ToList();

        public virtual ICollection<CompanyAddresses> CompanyAddressMappings
        {
            get => _companyAddressMappings ?? (_companyAddressMappings = new List<CompanyAddresses>());
            protected set => _companyAddressMappings = value;
        }

        public IList<Customer> Customers => CompanyCustomerMappings.Select(mapping => mapping.Customer).ToList();

        public virtual ICollection<CompanyCustomerMapping> CompanyCustomerMappings
        {
            get => _companyCustomerMappings ?? (_companyCustomerMappings = new List<CompanyCustomerMapping>());
            protected set => _companyCustomerMappings = value;
        }

        public virtual ICollection<ItemPricing> ItemsPricing
        {
            get => _itemsPricing ?? (_itemsPricing = new List<ItemPricing>());
            protected set => _itemsPricing = value;
        }

        public virtual PriceLevel PriceLevel { get; set; }

        #endregion

    }
}
