using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLogic.SharedBusinessLogic.Security;

namespace Axelerate.BusinessLogic.SharedBusinessLogic
{
    /// <summary>
    /// This interface provides a common method (Rank) that will allow a user to rank an object.
    /// Also gets the average rank the object has (AverageRank)
    /// </summary>
    public interface IRankeable
    {
        /// <summary>
        /// Ranks an object.  The implementer should save the ranking in its own tables.
        /// </summary>
        /// <param name="user">User who is ranking the object</param>
        /// <param name="rank">Rank value.  From 1 to 5</param>
        /// <param name="comment">Comment </param>
        void Rank(clsADUser user, int rank, String comment);

        /// <summary>
        /// Gets the average rank of the object
        /// </summary>
        /// <returns>the average rank</returns>
        float AverageRank();

        /// <summary>
        /// Gets the amount of ranks received by this object
        /// </summary>
        /// <returns>the amount of ranks received by this object</returns>
        int RankQuantity();

        /// <summary>
        /// Gets the actual rank given by user
        /// </summary>
        /// <param name="user">User who rated the object</param>
        /// <returns>The actual rank given by the user</returns>
        int ActualRank(clsADUser user);

    }
}
