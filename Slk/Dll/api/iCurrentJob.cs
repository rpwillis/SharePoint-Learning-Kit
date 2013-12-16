using System;
using System.Globalization;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>An object representing the current job/transaction.</summary>
    public interface ICurrentJob : IDisposable
    {
        /// <summary>Completed the transaction.</summary>
        void Complete();

        /// <summary>Cancel the transaction.</summary>
        void Cancel();
    }
}

