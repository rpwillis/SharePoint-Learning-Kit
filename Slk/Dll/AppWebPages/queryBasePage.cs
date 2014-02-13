using System;
using System.Globalization;
using System.Web;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    /// <summary>A base page for the query pages</summary>
    public class QueryBasePage : SlkAppBasePage
    {
        /// <summary>
        /// Holds the learner store corresponding to the input user in the case of an Observer's role
        /// </summary>
        SlkStore observerRoleLearnerStore;

#region constructors
#endregion constructors

#region properties
        /// <summary>See <see cref="SlkAppBasePage.OverrideMasterPage"/>.</summary>
        protected override bool OverrideMasterPage
        {
            get { return false ;}
        }

        /// <summary>The SlkStore to use.</summary>
        public override SlkStore SlkStore
        {
            get
            {
                if (String.IsNullOrEmpty(ObserverRoleLearnerKey) == false)
                {
                    if (observerRoleLearnerStore == null)
                    {
                        observerRoleLearnerStore = SlkStore.GetStore(SPWeb, ObserverRoleLearnerKey);
                    }
                    return observerRoleLearnerStore;
                }
                return base.SlkStore;
            }
        }

#endregion properties

#region public methods
#endregion public methods

#region protected methods
        /// <summary>
        ///  Page Init for AlwpQueryResults. 
        /// </summary> 
        /// <param name="sender">an object referencing the source of the event</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected void Page_Init(object sender, EventArgs e)
        {
            //Setting Cache-Control = "no-cache" to prevent caching 
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
        }

        /// <summary>
        /// Creates a query based on a given <c>QueryDefinition</c> and values for standard SLK
        /// macros.
        /// </summary>
        ///
        /// <param name="queryDef">The query definition to use to create the query.</param>
        ///
        /// <param name="countOnly">If <c>true</c>, the query will include minimal output columns,
        ///     since it will be assumed that the purpose of executing the query is purely to count the
        ///     rows.  If <c>false</c>, all columns specified by the query definition are included
        ///     in the query.</param>
        ///
        /// <param name="columnMap">See QueryDefinition.CreateQuery.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        protected LearningStoreQuery CreateStandardQuery(QueryDefinition queryDef, bool countOnly, out int[,] columnMap)
        {
            Guid[] spWebScopeMacro = CreateWebScopeMacro();

            return queryDef.CreateQuery(SlkStore.LearningStore, countOnly, new MacroResolver(SlkStore, spWebScopeMacro), out columnMap);
        }

#endregion protected methods

#region private methods
        /// <summary>Creates the web scope macro.</summary>
        private Guid[] CreateWebScopeMacro()
        {
            string spWebScope = QueryString.ParseStringOptional(QueryStringKeys.SPWebScope);
            if (spWebScope == null)
            {
                return null;
            }
            else
            {
                string[] webs = spWebScope.Split(',');
                Guid[] guids = new Guid[webs.Length];
                for (int i = 0; i < webs.Length; i++)
                {
                    string webId = webs[i];
                    if (webId != null)
                    {
                        try
                        {
                            guids[i] = new Guid(webId);
                        }
                        catch (FormatException) { }
                        catch (OverflowException) { }
                    }
                }

                return guids;
            }
        }

#endregion private methods

#region static members
#endregion static members
    }
}

