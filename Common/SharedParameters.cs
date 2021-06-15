using System;

namespace Common
{
    public class SharedParameters
    {
        public static readonly TimeSpan SessionDuration = TimeSpan.FromDays(30);
        public const string VideoClientName = "video-api";
        public const string AuthHeaderName = "x-video-apikey";
        public const string VideoListPath = "videos";
        public const string VideoPath = "video";
    }
}