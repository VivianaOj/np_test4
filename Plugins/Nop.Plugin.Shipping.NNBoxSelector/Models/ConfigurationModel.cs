using Nop.Core.Configuration;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Shipping.NNBoxSelector.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AddBoxModel = new BSBoxModel();
            BoxesSelectorSearchModel = new BoxesSelectorSearchModel();
        }

        public BSBoxModel AddBoxModel { get; set; }

        public BoxesSelectorSearchModel BoxesSelectorSearchModel { get; set; }

        public NNBoxselectorSettings NNBoxSettings { get; set; }
    }

  }
