using System.Collections.Generic;
using System.Runtime.Serialization;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Shipping.NNBoxGenerator;

namespace Nop.Services.Shipping
{
    /// <summary>
    /// Represents a request for getting shipping rate options
    /// </summary>
    public partial class GetShippingOptionRequest
    {
        #region Ctor

        public GetShippingOptionRequest()
        {
            Items = new List<PackageItem>();
			ResultFinal = new List<ContainerPackingResult>();
			Package = new List<Package>();
		}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets a shopping cart items
        /// </summary>
        public IList<PackageItem> Items { get; set; }

        /// <summary>
        /// Gets or sets a shipping address (where we ship to)
        /// </summary>
        public Address ShippingAddress { get; set; }

        /// <summary>
        /// Shipped from warehouse
        /// </summary>
        public Warehouse WarehouseFrom { get; set; }

        /// <summary>
        /// Shipped from country
        /// </summary>
        public Country CountryFrom { get; set; }

        /// <summary>
        /// Shipped from state/province
        /// </summary>
        public StateProvince StateProvinceFrom { get; set; }

        /// <summary>
        /// Shipped from zip/postal code
        /// </summary>
        public string ZipPostalCodeFrom { get; set; }

        /// <summary>
        /// Shipped from county
        /// </summary>
        public string CountyFrom { get; set; }

        /// <summary>
        /// Shipped from city
        /// </summary>
        public string CityFrom { get; set; }

        /// <summary>
        /// Shipped from address
        /// </summary>
        public string AddressFrom { get; set; }

        /// <summary>
        /// Limit to store (identifier)
        /// </summary>
        public int StoreId { get; set; }

        public List<ContainerPackingResult> ResultFinal { get; set; }

		public bool Packed { get; set; }

		public List<Package> Package { get; set; }

		#endregion

		#region Nested classes

		/// <summary>
		/// Package item
		/// </summary>
		public class PackageItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="sci">Shopping cart item</param>
            /// <param name="qty">Override "Quantity" property of shopping cart item</param>
            public PackageItem(ShoppingCartItem sci, int? qty = null)
            {
                ShoppingCartItem = sci;
                OverriddenQuantity = qty;
            }

            /// <summary>
            /// Shopping cart item
            /// </summary>
            public ShoppingCartItem ShoppingCartItem { get; set; }

            /// <summary>
            /// If specified, override "Quantity" property of "ShoppingCartItem
            /// </summary>
            public int? OverriddenQuantity { get; set; }

            /// <summary>
            /// Get quantity
            /// </summary>
            /// <returns></returns>
            public int GetQuantity()
            {
                if (OverriddenQuantity.HasValue)
                    return OverriddenQuantity.Value;

                return ShoppingCartItem.Quantity;
            }
        }

        #endregion
    }


	/// <summary>
	/// The container packing result.
	/// </summary>
	[DataContract]
	public class ContainerPackingResult
	{
		#region Constructors

		public ContainerPackingResult()
		{
			this.AlgorithmPackingResults = new List<AlgorithmPackingResult>();
		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the container ID.
		/// </summary>
		/// <value>
		/// The container ID.
		/// </value>
		[DataMember]
		public int ContainerID { get; set; }

		[DataMember]
		public string ContainerName { get; set; }

		[DataMember]
		public List<AlgorithmPackingResult> AlgorithmPackingResults { get; set; }

		#endregion Public Properties
	}

	[DataContract]
	public class AlgorithmPackingResult
	{
		#region Constructors

		public AlgorithmPackingResult()
		{
			this.PackedItems = new List<Item>();
			this.UnpackedItems = new List<Item>();
		}

		#endregion Constructors

		#region Public Properties

		[DataMember]
		public int AlgorithmID { get; set; }
		public int Id { get; set; }


		[DataMember]
		public string AlgorithmName { get; set; }

		public string ContainerName { get; set; }

		public bool Active { get; set; }

		public Container Container { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether all of the items are packed in the container.
		/// </summary>
		/// <value>
		/// True if all the items are packed in the container; otherwise, false.
		/// </value>
		[DataMember]
		public bool IsCompletePack { get; set; }

		/// <summary>
		/// Gets or sets the list of packed items.
		/// </summary>
		/// <value>
		/// The list of packed items.
		/// </value>
		[DataMember]
		public List<Item> PackedItems { get; set; }

		/// <summary>
		/// Gets or sets the elapsed pack time in milliseconds.
		/// </summary>
		/// <value>
		/// The elapsed pack time in milliseconds.
		/// </value>
		[DataMember]
		public long PackTimeInMilliseconds { get; set; }

		/// <summary>
		/// Gets or sets the percent of container volume packed.
		/// </summary>
		/// <value>
		/// The percent of container volume packed.
		/// </value>
		[DataMember]
		public decimal PercentContainerVolumePacked { get; set; }



		/// <summary>
		/// Gets or sets the percent of item volume packed.
		/// </summary>
		/// <value>
		/// The percent of item volume packed.
		/// </value>
		[DataMember]
		public decimal PercentItemVolumePacked { get; set; }


		/// <summary>
		/// Gets or sets the percent of item volume packed.
		/// </summary>
		/// <value>
		/// The percent of item volume packed.
		/// </value>
		[DataMember]
		public decimal PercentItemWeightPacked { get; set; }


		/// <summary>
		/// Gets or sets the percent of item volume packed.
		/// </summary>
		/// <value>
		/// The percent of item volume packed.
		/// </value>
		[DataMember]
		public decimal TotalVolumePacked { get; set; }
		/// <summary>
		/// Gets or sets the list of unpacked items.
		/// </summary>
		/// <value>
		/// The list of unpacked items.
		/// </value>
		[DataMember]
		public List<Item> UnpackedItems { get; set; }

		public List<ShippingOption> ShippingOption { get; set; }

		public bool IsAsShip { get; set; }

		public string PercentBoxVolumePacked { get; set; }

		#endregion Public Properties
	}

	/// <summary>
	/// An item to be packed. Also used to hold post-packing details for the item.
	/// </summary>
	[DataContract]
	public class Item
	{
		#region Private Variables

		private decimal volume;

		#endregion Private Variables

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the Item class.
		/// </summary>
		/// <param name="id">The item ID.</param>
		/// <param name="dim1">The length of one of the three item dimensions.</param>
		/// <param name="dim2">The length of another of the three item dimensions.</param>
		/// <param name="dim3">The length of the other of the three item dimensions.</param>
		/// <param name="itemQuantity">The item quantity.</param>
		public Item(int id, decimal dim1, decimal dim2, decimal dim3, decimal WeightPacked, int quantity, string name)
		{
			this.ID = id;
			this.Dim1 = dim1;
			this.Dim2 = dim2;
			this.Dim3 = dim3;
			this.volume = dim1 * dim2 * dim3;
			this.WeightPacked = WeightPacked;
			this.Quantity = quantity;
			this.ProductName = name;

		}

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the item ID.
		/// </summary>
		/// <value>
		/// The item ID.
		/// </value>
		[DataMember]
		public int ID { get; set; }

		public string ProductName { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this item has already been packed.
		/// </summary>
		/// <value>
		///   True if the item has already been packed; otherwise, false.
		/// </value>
		[DataMember]
		public bool IsPacked { get; set; }

		/// <summary>
		/// Gets or sets the length of one of the item dimensions.
		/// </summary>
		/// <value>
		/// The first item dimension.
		/// </value>
		[DataMember]
		public decimal Dim1 { get; set; }

		/// <summary>
		/// Gets or sets the length another of the item dimensions.
		/// </summary>
		/// <value>
		/// The second item dimension.
		/// </value>
		[DataMember]
		public decimal Dim2 { get; set; }

		/// <summary>
		/// Gets or sets the third of the item dimensions.
		/// </summary>
		/// <value>
		/// The third item dimension.
		/// </value>
		[DataMember]
		public decimal Dim3 { get; set; }

		/// <summary>
		/// Gets or sets the x coordinate of the location of the packed item within the container.
		/// </summary>
		/// <value>
		/// The x coordinate of the location of the packed item within the container.
		/// </value>
		[DataMember]
		public decimal CoordX { get; set; }

		/// <summary>
		/// Gets or sets the y coordinate of the location of the packed item within the container.
		/// </summary>
		/// <value>
		/// The y coordinate of the location of the packed item within the container.
		/// </value>
		[DataMember]
		public decimal CoordY { get; set; }

		/// <summary>
		/// Gets or sets the z coordinate of the location of the packed item within the container.
		/// </summary>
		/// <value>
		/// The z coordinate of the location of the packed item within the container.
		/// </value>
		[DataMember]
		public decimal CoordZ { get; set; }

		/// <summary>
		/// Gets or sets the item quantity.
		/// </summary>
		/// <value>
		/// The item quantity.
		/// </value>
		public int Quantity { get; set; }

		/// <summary>
		/// Gets or sets the x dimension of the orientation of the item as it has been packed.
		/// </summary>
		/// <value>
		/// The x dimension of the orientation of the item as it has been packed.
		/// </value>
		[DataMember]
		public decimal PackDimX { get; set; }

		/// <summary>
		/// Gets or sets the y dimension of the orientation of the item as it has been packed.
		/// </summary>
		/// <value>
		/// The y dimension of the orientation of the item as it has been packed.
		/// </value>
		[DataMember]
		public decimal PackDimY { get; set; }

		/// <summary>
		/// Gets or sets the z dimension of the orientation of the item as it has been packed.
		/// </summary>
		/// <value>
		/// The z dimension of the orientation of the item as it has been packed.
		/// </value>
		[DataMember]
		public decimal PackDimZ { get; set; }

		/// <summary>
		/// Gets the item volume.
		/// </summary>
		/// <value>
		/// The item volume.
		/// </value>
		[DataMember]
		public decimal Volume
		{
			get
			{
				return volume;
			}
		}

		/// <summary>
		/// Gets or sets the percent of container volume packed.
		/// </summary>
		/// <value>
		/// The percent of container volume packed.
		/// </value>
		[DataMember]
		public decimal WeightPacked { get; set; }

		#endregion Public Properties
	}

	public class EmployeeComparer : IComparer<Item>
	{
		public enum sortBy
		{
			dim1,
			dim2,
			dim3,
			quantity
		}

		//Sort two employee Ages  
		public sortBy compareByFields = sortBy.dim1;
		public int Compare(Item x, Item y)
		{
			switch (compareByFields)
			{
				case sortBy.dim1:
					return x.Dim1.CompareTo(y.Dim2);
				case sortBy.dim2:
					return x.Dim1.CompareTo(y.Dim2);
				case sortBy.dim3:
					return x.Dim1.CompareTo(y.Dim2);
				default: break;

			}
			return x.Dim1.CompareTo(y.Dim2);

		}
	}

	public class Container
	{
		#region Private Variables

		private decimal volume;

		#endregion Private Variables

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the Container class.
		/// </summary>
		/// <param name="id">The container ID.</param>
		/// <param name="length">The container length.</param>
		/// <param name="width">The container width.</param>
		/// <param name="height">The container height.</param>
		public Container(decimal length, decimal width, decimal height, decimal weight, int id, string name, decimal weightBox)
		{
			this.ID = id;
			this.Length = length;
			this.Width = width;
			this.Height = height;
			this.Weight = weight;
			this.Name = name;
			this.Volume = length * width * height;
			this.WeightBox = weightBox;
		}

		#endregion Constructors

		#region Public Properties
		/// <summary>
		/// Gets or sets the container ID.
		/// </summary>
		/// <value>
		/// The container ID.
		/// </value>
		public int ID { get; set; }
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the container length.
		/// </summary>
		/// <value>
		/// The container length.
		/// </value>
		public decimal Length { get; set; }

		/// <summary>
		/// Gets or sets the container width.
		/// </summary>
		/// <value>
		/// The container width.
		/// </value>
		public decimal Width { get; set; }

		/// <summary>
		/// Gets or sets the container height.
		/// </summary>
		/// <value>
		/// The container height.
		/// </value>
		public decimal Height { get; set; }

		public decimal Weight { get; set; }

		public decimal WeightBox { get; set; }
		/// <summary>
		/// Gets or sets the volume of the container.
		/// </summary>
		/// <value>
		/// The volume of the container.
		/// </value>
		public decimal Volume
		{
			get
			{
				return this.volume;
			}
			set
			{
				this.volume = value;
			}
		}

		#endregion Public Properties
	}
}