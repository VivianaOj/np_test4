using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Nop.Core.Domain.Shipping;
using Nop.Services.Shipping.NNBoxGenerator;

namespace Nop.Services.Shipping
{
    /// <summary>
    /// Represents a response of getting shipping rate options
    /// </summary>
    public partial class GetShippingOptionResponse
    {
        public GetShippingOptionResponse()
        {
            Errors = new List<string>();
            ShippingOptions = new List<ShippingOption>();
            ResultFinal = new List<ContainerPackingResult>();
            Package = new List<Package>();
        }

        /// <summary>
        /// Gets or sets a list of shipping options
        /// </summary>
        public IList<ShippingOption> ShippingOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shipping is done from multiple locations (warehouses)
        /// </summary>
        public bool ShippingFromMultipleLocations { get; set; }

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

        public List<ContainerPackingResult> ResultFinal { get; set; }
        public bool IsCommercial { get; set; }
        public List<Package> Package { get; set; }


        public string NetsuiteLocationName { get; set; }

    }

    public class Package
    {
        public PackageType PackageType { get; set; }
        public string Description { get; set; }

        public DimensionsType DimensionsType { get; set; }
        public PackageWeight PackageWeight { get; set; }
        
    }
    public class PackageType
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class DimensionsType
    {
        public UnitOfMeasurement UnitOfMeasurement { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }

    public class UnitOfMeasurement
    {
        public string Code { get; set; }
    }
    public class PackageWeight
    {
        public UnitOfMeasurement UnitOfMeasurement { get; set; }
        public string Weight { get; set; }
    }
}