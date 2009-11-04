/* Copyright (c) Microsoft Corporation. All rights reserved. */

// QuerySummary.aspx.cs
//
// Renders the contents of the hidden frame loaded within QuerySet.aspx (the left pane) of ALWP.
// This hidden frame page computes the count of rows produced by each query in a given query set.
//
// URL example:
//
// ALWP/QuerySummary.aspx?QuerySet=LearnerQuerySet&CurrentUser=328/KEY:Chris Ashton&Today=2006-04-06
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Schema;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.SharePointLearningKit.Schema;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Microsoft.SharePointLearningKit.WebControls;
using Microsoft.SharePointLearningKit.WebParts;
using Microsoft.SharePointLearningKit.Localization;


namespace Microsoft.SharePointLearningKit.ApplicationPages
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Alwp")]
	public partial class AlwpQuerySummary : SlkAppBasePage
    {
        #region Private Variable
        /// <summary>
        /// This is a parameter of CreateStandardQuery.  It's specifies the SPWeb that this
        /// query set is scoped to, or null if there's no scope (i.e. retrieve results for
        /// all SPWebs).
        /// </summary>   
        Guid? m_spWebScopeMacro;

        SlkStore m_observerRoleLearnerStore;

        #endregion

        public override SlkStore SlkStore
        {
            get
            {
                if (String.IsNullOrEmpty(ObserverRoleLearnerKey) == false)
                {
                    if (m_observerRoleLearnerStore == null)
                    {
                        m_observerRoleLearnerStore = SlkStore.GetStore(SPWeb, ObserverRoleLearnerKey);
                    }
                    return m_observerRoleLearnerStore;
                }
                return base.SlkStore;
            }
        }

        #region Page_Init
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
        #endregion

        #region Page_Load
        /// <summary>
        ///  Page Load for AlwpQuerySummary Hidden frame. 
        /// </summary> 
        /// <param name="sender">an object referencing the source of the event</param>
        /// <param name="e">An EventArgs that contains the event data.</param>	
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                AppResources.Culture = LocalizationManager.GetCurrentCulture();
                //Get the QuerySet Name 
                string querySetName;

                QueryString.Get(QueryStringKeys.QuerySet, out querySetName, false);

                //Get the Visibility 
                string spWebScope;
                QueryString.Get(QueryStringKeys.SPWebScope, out spWebScope, true);
                m_spWebScopeMacro = (spWebScope == null) ? (Guid?)null : (new Guid(spWebScope));

                // create a job for executing the queries specified by <querySetDef>
                LearningStoreJob job = SlkStore.LearningStore.CreateJob();

                // set <querySetDef> to the QuerySetDefinition named <querySetName>
                QuerySetDefinition querySetDef = SlkStore.Settings.FindQuerySetDefinition(querySetName, true);

                if (querySetDef == null)
                {
                    throw new SafeToDisplayException
                                            (AppResources.AlwpQuerySetNotFound, querySetName);
                }

                // create queries corresponding to <querySetDef> and add them to <job>;
                // set <numberOfQueries> to the number of queries in the query set
                int numberOfQueries = 0;
                foreach (QueryDefinition queryDef in querySetDef.Queries)
                {
                    PerformQuery(queryDef, job);
                    numberOfQueries++;
                }

                // execute the job
                ReadOnlyCollection<object> results = null;
                try
                {
                    // execute the job
                    results = job.Execute();
                }
                catch (SqlException sqlEx)
                {
                    //check whether deadlock occured and throw the exception back 
                    if (sqlEx.Number == 1205)
                    {
                        throw;
                    }

                    // don't need SlkError.WriteToEventLog(ex) because we'll write to the event log again
                    // in RenderQueryCounts
                    results = null;
                }
                finally
                {
                    RenderQueryCounts(querySetDef, numberOfQueries, results);
                }

            }
            catch (Exception ex)
            {
                SlkError.WriteToEventLog(ex);
            }

        }
        #endregion

        #region RenderQueryCounts
        /// <summary>
        /// Renders the Javascript that contains the query result counts.
        /// Gets the Query Count from results Collections and sets the count.
        /// If results collection is null Execute each Query individually
        /// and get the count. Set the Query count as "ERROR" for Failed one.
        /// </summary>
        private void RenderQueryCounts(QuerySetDefinition querySetDef,
                               int numberOfQueries,
                               ReadOnlyCollection<object> results)
        {
            AppResources.Culture = LocalizationManager.GetCurrentCulture();
            // render the HTML for the page
            using (HtmlTextWriter hw = new HtmlTextWriter(Response.Output, "  "))
            {
                // render the "<html>" element and its contents
                using (new HtmlBlock(HtmlTextWriterTag.Html, 0, hw))
                {
                    // write script code that contains the query result counts
                    using (new HtmlBlock(HtmlTextWriterTag.Script, 0, hw))
                    {
                        hw.Write(String.Format(CultureInfo.InvariantCulture,
                                               "var a = new Array({0});", 
                                               numberOfQueries));
                        hw.WriteLine();

                        //To Store all the Query Counts
                        //string queryCounts = String.Empty;
                        StringBuilder queryCounts = new StringBuilder(100);
                        //Query Index
                        int queryIndex = 0;

                        //isError will be set to false if *any* query succeeded -- in this case, if
                        //To Verify is there is an Error Processing QuerySet and setting QueryCounts
                        bool isError = true;

                        foreach (QueryDefinition queryDef in querySetDef.Queries)
                        {
                            int queryResultCount;

                            if (results != null)
                            {
                                queryResultCount = ((DataTable)results[queryIndex]).Rows.Count;
                                isError = false;
                            }
                            else
                            {
                                //One of the QueryExecution in the QuerySet failed. 
                                //while executing the Job. Execute each Query individually.
                                queryResultCount = ExecuteQuery(queryDef);

                                //Check for Query Result Count.
                                //queryResultCount must return -1 for any one of the Query
                                //as  QuerySet execution failed.
                                if (queryResultCount == -1)
                                {
                                    isError = false;
                                }
                            }

                            if (queryResultCount != -1)
                            {
                                queryCounts.AppendLine(
                                            String.Format(CultureInfo.InvariantCulture,
                                                          "a[{0}] = {1};",
                                                          queryIndex,
                                                          queryResultCount));
                            }
                            else
                            {
                                queryCounts.AppendLine(
                                            String.Format(CultureInfo.CurrentCulture,
                                                          "a[{0}] = \"{1}\";",
                                                          queryIndex,
                                                          AppResources.AlwpQueryResultError));
                            }
                            queryIndex++;

                        }

                        //isError Can't be true. if QuerySet execution failed 
                        //then any of the  Query Execution should have Failed. 
                        //If isError true means individual Execution of each
                        //Query succeeds and QuerySet Execution failed always. 
                        //Which affects performance. indicate the impact in 
                        //result count by setting the count to ERROR for all results.
                        if (isError)
                        {
                            // in this situation, the first time we ran the "batch" query (to get
                            // counts for the entire query set), some query failed, but we didn't know
                            // *which* query failed so we executed each query one at a time, and this
                            // time, all of them succeeded -- which shouldn't have happened, so we
                            // display "ERROR" for all counts so that the user knows something weird/bad
                            // happened
                            queryCounts.Length = 0;
                            for (int i = 0; i < queryIndex; i++)
                            {
                                queryCounts.AppendLine(
                                                    String.Format(CultureInfo.CurrentCulture,
                                                                  "a[{0}] = \"{1}\";", i,
                                                                  AppResources.AlwpQueryResultError));
                            }
                        }

                        hw.Write(queryCounts);
                        hw.WriteLine();

                        hw.Write("if (parent.SetQueryCounts != undefined)");
                        hw.WriteLine();
                        hw.Write("  parent.SetQueryCounts(a);");
                    }
                }
            }

        }
        #endregion

        #region PerformQuery
        /// <summary>
        /// Performs the Query Execution.
        /// </summary>
        /// <param name="queryDef">QueryDefinition</param>
        /// <param name="job">LearningStoreJob instance to perform Job</param>
        private void PerformQuery(QueryDefinition queryDef, LearningStoreJob job)
        {

            // create a query based on <queryDef>
            LearningStoreQuery query;
            int[,] columnMap; // see QueryDefinition.CreateQuery

            query = CreateStandardQuery(queryDef, true, m_spWebScopeMacro, out columnMap);

            // add the query to the job
            job.PerformQuery(query);

        }
        #endregion

        #region ExecuteQuery
        /// <summary>
        /// Performs the Query Execution and Execute
        /// </summary>
        /// <param name="queryDef"></param>
        /// <returns>Query Count</returns>
        private int ExecuteQuery(QueryDefinition queryDef)
        {
            try
            {
                // create a job for executing the queries specified by <querySetDef>
                LearningStoreJob job = SlkStore.LearningStore.CreateJob();

                PerformQuery(queryDef, job);

                // execute the job
                DataTable queryResults = job.Execute<DataTable>();
                return queryResults != null ? queryResults.Rows.Count : -1;
            }
            catch (SqlException ex)
            {
                SlkError.WriteToEventLog(ex);
                return -1;
            }
        }

        #endregion

    }
}
