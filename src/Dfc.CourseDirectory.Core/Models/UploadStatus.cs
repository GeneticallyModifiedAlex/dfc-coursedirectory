﻿namespace Dfc.CourseDirectory.Core.Models
{
    public enum UploadStatus
    {
        /// <summary>
        /// A file has been uploaded but processing its contents has not started.
        /// </summary>
        Created = 0,

        /// <summary>
        /// Processing of the upload's contents is currently in progress.
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Processing the upload's contents is completed.
        /// </summary>
        Processed = 2,

        /// <summary>
        /// The contents of the upload have been published.
        /// </summary>
        Published = 3,

        /// <summary>
        /// The upload has been replaced with another without its contents published.
        /// </summary>
        Abandoned = 4
    }

    public static class UploadStatusExtensions
    {
        public static bool IsComplete(this UploadStatus status) => status switch
        {
            UploadStatus.Published => true,
            UploadStatus.Abandoned => true,
            _ => false
        };
    }
}