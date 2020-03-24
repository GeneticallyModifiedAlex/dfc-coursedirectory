﻿using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModel : IMptxState
    {
        public string ProviderMarketingInformation { get; set; }
        public StandardOrFramework ApprenticeshipStandardOrFramework { get; set; }
        public string ApprenticeshipMarketingInformation { get; set; }
        public string ApprenticeshipWebsite { get; set; }
        public string ApprenticeshipContactTelephone { get; set; }
        public string ApprenticeshipContactEmail { get; set; }
        public string ApprenticeshipContactWebsite { get; set; }
        public bool? ApprenticeshipIsNational { get; set; }

        public bool GotApprenticeshipDetails { get; set; }
        public bool GotProviderDetails { get; set; }

        public bool IsValid => GotProviderDetails && GotApprenticeshipDetails; // FIXME

        public void SetProviderDetails(string marketingInformation)
        {
            ProviderMarketingInformation = marketingInformation;
            GotProviderDetails = true;
        }

        public void SetApprenticeshipDetails(
            string marketingInformation,
            string website,
            string contactTelephone,
            string contactEmail,
            string contactWebsite)
        {
            ApprenticeshipMarketingInformation = marketingInformation;
            ApprenticeshipWebsite = website;
            ApprenticeshipContactTelephone = contactTelephone;
            ApprenticeshipContactEmail = contactEmail;
            ApprenticeshipContactWebsite = contactWebsite;
            GotApprenticeshipDetails = true;
        }
		
		public void SetApprenticeshipIsNational(bool national)
        {
            ApprenticeshipIsNational = national;
        }

        public void SetApprenticeshipStandardOrFramework(StandardOrFramework standardOrFramework) =>
            ApprenticeshipStandardOrFramework = standardOrFramework;
    }
}
