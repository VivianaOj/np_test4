using Nop.Core.Data;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Common
{
    public partial class AnywhereSliderService : IAnywhereSliderService
    {
        #region Fields

        private readonly IRepository<SS_AS_AnywhereSlider> _anywhereSliderRepository;
        private readonly IRepository<SS_AS_SliderImage> _sliderImageReposiroty;

        #endregion

        #region Ctor

        public AnywhereSliderService(IRepository<SS_AS_AnywhereSlider> anywhereSliderRepository, IRepository<SS_AS_SliderImage> sliderImageReposiroty)
        {
            _anywhereSliderRepository = anywhereSliderRepository;
            _sliderImageReposiroty = sliderImageReposiroty;
        }


        #endregion
        #region Methods

        public SS_AS_AnywhereSlider GetRegiusterById(string SystemName)
        {
            if (SystemName == null)
                throw new ArgumentNullException(nameof(SystemName));

            var query = _anywhereSliderRepository.Table.Where(x => x.SystemName == SystemName).FirstOrDefault();
            SS_AS_AnywhereSlider AnywhereSlider = new SS_AS_AnywhereSlider();
            AnywhereSlider.Id = query.Id;
            AnywhereSlider.SystemName = query.SystemName;
            AnywhereSlider.LanguageId = query.LanguageId;
            AnywhereSlider.SliderType = query.SliderType;
            AnywhereSlider.LimitedToStores = query.LimitedToStores;

            return AnywhereSlider;

                        
        }

        public SS_AS_SliderImage GetSliderById(int SliderId)
        {
            if (SliderId == 0)
                throw new ArgumentNullException(nameof(SliderId));

            var query = _sliderImageReposiroty.Table.Where(x => x.SliderId == SliderId).FirstOrDefault();
            SS_AS_SliderImage sliderImage = new SS_AS_SliderImage();
            sliderImage.Id = query.Id;
            sliderImage.DisplayText = query.DisplayText;
            sliderImage.Url = query.Url;
            sliderImage.Alt = query.Alt;
            sliderImage.Visible = query.Visible;
            sliderImage.DisplayOrder = query.DisplayOrder;
            sliderImage.PictureId = query.PictureId;
            sliderImage.MobilePictureId = query.MobilePictureId;
            sliderImage.SliderId = query.SliderId;

            return sliderImage;
        }


        #endregion
    }
}
