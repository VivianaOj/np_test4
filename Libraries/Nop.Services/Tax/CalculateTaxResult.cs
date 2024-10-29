using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Tax
{
    /// <summary>
    /// Represents a result of tax calculation
    /// </summary>
    public partial class CalculateTaxResult
    {
        public CalculateTaxResult()
        {
            Errors = new List<string>();
            breakdown = new Breakdown();
        }

        /// <summary>
        /// Gets or sets a tax rate
        /// </summary>
        public decimal TaxRate { get; set; }

        public bool HasNexus { get; set; }
        /// <summary>
        /// Gets or sets errors
        /// </summary>
        public IList<string> Errors { get; set; }

        /// <summary>
        /// Gets a value indicating whether request has been completed successfully
        /// </summary>
        public bool Success => !Errors.Any();

        /// <summary>
        /// Add error
        /// </summary>
        /// <param name="error">Error</param>
        public void AddError(string error)
        {
            Errors.Add(error);
        }


        public decimal order_total_amount { get; set; }
        public decimal shipping { get; set; }
        public decimal taxable_amount { get; set; }
        public decimal amount_to_collect { get; set; }
        public bool freight_taxable { get; set; }
        public string tax_source { get; set; }
        public Breakdown breakdown { get; set; }

    }

    public class Breakdown
    {
        public Breakdown()
        {
            line_items = new List<LineItem>();
        }

        public decimal taxable_amount { get; set; }
        public decimal tax_collectable { get; set; }
        public decimal combined_tax_rate { get; set; }
        public decimal state_taxable_amount { get; set; }
        public decimal state_tax_rate { get; set; }
        public decimal state_tax_collectable { get; set; }
        public decimal county_taxable_amount { get; set; }
        public decimal county_tax_rate { get; set; }
        public decimal county_tax_collectable { get; set; }
        public decimal city_taxable_amount { get; set; }
        public decimal city_tax_rate { get; set; }
        public decimal city_tax_collectable { get; set; }
        public decimal special_district_taxable_amount { get; set; }
        public decimal special_tax_rate { get; set; }
        public decimal special_district_tax_collectable { get; set; }

        public List<LineItem> line_items { get; set; }
    }

    public class LineItem
    {
        public int Id { get; set; }
        public decimal taxable_amount { get; set; }
        public decimal tax_collectable { get; set; }
        public decimal combined_tax_rate { get; set; }
        public decimal state_taxable_amount { get; set; }
        public decimal state_sales_tax_rate { get; set; }
        public decimal state_amount { get; set; }
        public decimal county_taxable_amount { get; set; }
        public decimal county_tax_rate { get; set; }
        public decimal county_amount { get; set; }
        public decimal city_taxable_amount { get; set; }
        public decimal city_tax_rate { get; set; }
        public decimal city_amount { get; set; }
        public decimal special_district_taxable_amount { get; set; }
        public decimal special_tax_rate { get; set; }
        public decimal special_district_amount { get; set; }
    }
}