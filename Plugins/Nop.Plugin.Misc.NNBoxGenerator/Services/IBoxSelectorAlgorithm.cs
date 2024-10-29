using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.NNBoxGenerator.Models.AlgorithmBase;
using Nop.Services.Shipping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NNBoxGenerator.Services
{
    public interface IBoxSelectorAlgorithm
    {
		/// <summary>
		/// Runs the algorithm on the specified container and items.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="items">The items to pack.</param>
		/// <returns>The algorithm packing result.</returns>
		AlgorithmPackingResult Run(Container container, List<Item> items);

		/// <summary>
		/// Runs the algorithm on the specified container and items per volumen.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="items">The items to pack.</param>
		/// <returns>The algorithm packing result.</returns>
		AlgorithmPackingResult RunVolumen(Container container, List<Item> items);

	}
}
