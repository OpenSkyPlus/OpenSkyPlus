using System;

namespace OpenSkyPlus
{
    public class OpenSkyPlusApiException : Exception
    {
        public OpenSkyPlusApiException() { }

        public OpenSkyPlusApiException(string message)
            : base(message) { }

        public OpenSkyPlusApiException(string message, Exception inner)
            : base(message, inner) { }
    }
}