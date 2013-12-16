using System;
using System.Globalization;

namespace Microsoft.SharePointLearningKit.WebParts
{
    /// <summary>An observer connection.</summary>
    public interface IObserverConnection
    {
        /// <summary>The user id to display.</summary>
        int UserId { get; }

        /// <summary>Tells the provider the id of the ALWP web part.</summary>
        void AssignWebPartId(string id);
    }
}

