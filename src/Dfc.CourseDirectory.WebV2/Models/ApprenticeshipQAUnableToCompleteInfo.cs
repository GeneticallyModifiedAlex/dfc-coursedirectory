﻿using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    public class ApprenticeshipQAUnableToCompleteInfo
    {
        public Guid ProviderId { get; set; }
        public ApprenticeshipQAUnableToCompleteReasons UnableToCompleteReasons { get; set; }
        public string Comments { get; set; }
        public DateTime AddedOn { get; set; }
        public UserInfo AddedByUser { get; set; }
    }
}
