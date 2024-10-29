using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Nop.Plugin.Misc.NNBoxGenerator.Models.AlgorithmBase
{
	[DataContract]
	public enum AlgorithmType
	{
		/// <summary>
		/// The EB-AFIT packing algorithm type.
		/// </summary>
		[DataMember]
		EB_AFIT = 1
	}
}
