/* Copyright (c) Microsoft Corporation. All rights reserved. */

// QueryResults.aspx.cs
//
// Renders the contents of the results frame (right pane) of ALWP.
//


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using Microsoft.LearningComponents.Frameset;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit.WebControls;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Alwp")]
	public partial class AlwpQueryResults : SlkAppBasePage
    {
        #region Private Variables
        /// <summary>
        /// SPWeb GUID-to-name mappings.
        /// </summary>
        Dictionary<Guid, WebNameAndUrl> m_spWebNameMap;
        /// <summary>
        /// Query Name 
        /// </summary>
        string m_queryName;
        /// <summary>
        /// Holds the Visibility 
        /// </summary>   
        string m_spWebScope;
        /// <summary>
        /// Holds the Sort
        /// </summary>
        string m_sort;
        /// <summary>
        /// Holds the Unknown Site Count  This is the number of SPWeb sites for which
        /// we cannot determine the site name.
        /// </summary>
        int m_unknownSiteCount;
        /// <summary>
        /// Holds the learner key which is used as the user for the LearningStore in case of the observer
        /// </summary>
        string m_observerRoleLearnerKey;
        /// <summary>
        /// Holds the learner store corresponding to the input user in the case of an Observer's role
        /// </summary>
        SlkStore m_observerRoleLearnerStore;

        /// <summary>
        /// Is true if the current user is an observer and false otherwise. Used to avoid repeated calls to
        /// SlkStore
        /// </summary>
        bool? m_isObserver;

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the name of the SLK query to execute.
        /// Throws Exception if empty or null.
        /// </summary>
        private string Query
        {
            get
            {
                if (m_queryName == null)
                    QueryString.Get(QueryStringKeys.Query, out m_queryName, false);
                return m_queryName;
            }
        }
        /// <summary>
        /// Gets the GUID of the SPWeb to filter on.  Returns null if it's absent.
        /// </summary>
        private string SPWebScope
        {
            get
            {
                if (m_spWebScope == null)
                    QueryString.Get(QueryStringKeys.SPWebScope, out m_spWebScope, true);
                return m_spWebScope;
            }
        }
        /// <summary>
        /// Gets zero-based index of the column to sort on.  Returns null if it's absent.
        /// </summary>
        private string Sort
        {
            get
            {
                if (m_sort == null)
                    QueryString.Get(QueryStringKeys.Sort, out m_sort, true);
                return m_sort;
            }
        }

        /// <summary>
        /// Gets the learnerKey session parameter which is used as the LearningStore user
        /// if the user is an SlkObserver
        /// </summary>
        private string ObserverRoleLearnerKey
        {
            get
            {
                if (m_observerRoleLearnerKey == null)
                {
                    if(Session["LearnerKey"] != null && IsObserver)
                        m_observerRoleLearnerKey = Session["LearnerKey"].ToString();
                }
                return m_observerRoleLearnerKey;
            }
        }

        /// <summary>
        /// Returns true if the current user is an observer
        /// and false otherwise
        /// </summary>
        private bool IsObserver
        {
            get
            {
                if (m_isObserver == null)
                {
                    if (base.SlkStore.IsObserver(SPWeb) == true)
                        m_isObserver = true;
                    else
                        m_isObserver = false;
                }
                return (bool) m_isObserver;
            }
        }

        public override SlkStore SlkStore
        {
            get
            {
                if (String.IsNullOrEmpty(ObserverRoleLearnerKey) == false)
                {
                    if(m_observerRoleLearnerStore == null)
                        m_observerRoleLearnerStore = SlkStore.GetStore(SPWeb, ObserverRoleLearnerKey);
                    return m_observerRoleLearnerStore;
                }
                return base.SlkStore;
            }
        }
        

        #endregion

        #region Private and Protected Methods

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
        ///  Page Load for AlwpQueryResults. 
        /// </summary> 
        /// <param name="sender">an object referencing the source of the event</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            String queryCount = String.Empty;
            // render the HTML for the page
            using (HtmlTextWriter hw = new HtmlTextWriter(Response.Output, "  "))
            {
                // render the "<html>" element and its contents
                using (new HtmlBlock(HtmlTextWriterTag.Html, 0, hw))
                {
                    // render the "<head>" element and its contents
                    using (new HtmlBlock(HtmlTextWriterTag.Head, 1, hw))
                    {
                        // create a link to "core.css";
                        // "/_layouts/1033/styles/core.css" except with "1033" replaced with the
                        // current SPWeb language code

                        hw.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                        hw.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                        hw.AddAttribute(HtmlTextWriterAttribute.Href,
                                        String.Format(CultureInfo.InvariantCulture , 
                                                      "/_layouts/{0}/styles/core.css", 
                                                      SPWeb.Language));
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 1, hw);

                        //Adds the Theme Css Url to Enable Theming in the frame.
                        hw.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                        hw.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                        hw.AddAttribute(HtmlTextWriterAttribute.Href, SPWeb.ThemeCssUrl);
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 0, hw);

                        // create a link to ALWP's "Styles.css"
                        hw.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                        hw.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                        hw.AddAttribute(HtmlTextWriterAttribute.Href, "Include/Styles.css");
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 0, hw);
                    }

                    // render the "<body>" element and its contents
                    hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 90%; overflow-y: auto;");
                    using (new HtmlBlock(HtmlTextWriterTag.Body, 0, hw))
                    {
                        // render the outer table -- this contains only one row and one column, which
                        // in turn contains the entire query results table
                        hw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                        hw.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                        hw.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                        hw.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                        using (new HtmlBlock(HtmlTextWriterTag.Table, 0, hw))
                        {
                            // render the single row and column of the outer table
                            using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, hw))
                            {
                                using (new HtmlBlock(HtmlTextWriterTag.Td, 0, hw))
                                {
                                    hw.WriteLine();

                                    try
                                    {
                                        // set <queryDef> to the QueryDefinition named <queryName>
                                        QueryDefinition queryDef 
                                                    = SlkStore.Settings.FindQueryDefinition(Query);

                                        if (queryDef == null)
                                        {
                                            throw new SafeToDisplayException
                                                               (AppResources.AlwpQuerySetNotFound, Query);
                                        }


                                        List<RenderedCell[]> renderedRows = PerformQuery(queryDef);

                                        //Set the QueryCount 
                                        queryCount = renderedRows.Count.ToString(CultureInfo.InvariantCulture);
                                        //Renders the Result
                                        RenderQueryResults(queryDef, renderedRows, hw);
                                    }
                                    catch (Exception ex)
                                    {
                                        queryCount = AppResources.AlwpQueryResultError;
                                        SlkError slkError;
                                        //Handles SqlException separate to capture the deadlock 
                                        //and treat it differently
                                        SqlException sqlEx = ex as SqlException;
                                        if (sqlEx != null)
                                        {
                                            WebParts.ErrorBanner.WriteException(sqlEx, out slkError);
                                        }
                                        else
                                        {
                                            SlkError.WriteException(ex, out slkError);
                                        }

                                        WebParts.ErrorBanner.RenderErrorItems(hw, slkError);
                                    }
                                    finally
                                    {
                                        //Renders the JavaScript to set the Query Count.
                                        RenderQueryCount(queryCount);
                                    }
                                }
                            }
                        }
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
        /// <returns>Query Resutl to Render</returns>
        private List<RenderedCell[]> PerformQuery(QueryDefinition queryDef)
        {

            // create a job for executing the query specified by <queryDef>
            LearningStoreJob job = SlkStore.LearningStore.CreateJob();

            Guid? spWebScopeMacro
                   = (SPWebScope == null) ? (Guid?)null : (new Guid(SPWebScope));

            // create a query based on <queryDef>
            LearningStoreQuery query = null;
            int[,] columnMap; // see QueryDefinition.CreateQuery
            try
            {
                query = CreateStandardQuery(queryDef, false, spWebScopeMacro, out columnMap);
            }
            catch (SlkSettingsException ex)
            {
                throw new SafeToDisplayException(ex.Message);
            }

            // add the query to the job
            job.PerformQuery(query);

            // execute the job
            DataTable queryResults = job.Execute<DataTable>();

            // render the query results into <renderedRows>
            List<RenderedCell[]> renderedRows
                        = new List<RenderedCell[]>(queryResults.Rows.Count);
            foreach (DataRow dataRow in queryResults.Rows)
            {
                RenderedCell[] renderedCells = queryDef.RenderQueryRowCells(
                    dataRow, columnMap, ResolveSPWebName);
                renderedRows.Add(renderedCells);
            }

            return renderedRows;

        }

        #endregion

        #region GetSortIndex
        /// <summary>
        /// If the "Sort" query parameter specifies a sort, set <c>sortColumnIndex</c> to the zero-based
        /// column index to sort on, and <c>sortAscending</c> to true for an ascending sort or false for
        /// a descending sort; otherwise, set <c>sortColumnIndex</c> to -1.
        /// </summary>
        /// <param name="sortColumnIndex">The zero-based column index that the query results are
        /// 	currently sorted on, or -1 if the results are not sorted.</param>
        /// <returns><c>true</c> if the query results are currently sorted in an
        /// 	     an ascending order, false for an descending-order sort.  
        ///          Irrelevant if	<paramref name="sortColumnIndex"/> is -1.</returns>
        private bool GetSortIndex(out int sortColumnIndex)
        {

            bool sortAscending;

            if (Sort != null)
            {
                sortColumnIndex = int.Parse(Sort, CultureInfo.InvariantCulture);
                if (sortColumnIndex < 0)
                {
                    sortAscending = false;
                    sortColumnIndex = -sortColumnIndex;
                }
                else
                    sortAscending = true;
                sortColumnIndex--;
            }
            else
            {
                sortColumnIndex = -1;
                sortAscending = false;
            }

            return sortAscending;
        }

        #endregion

        #region ResolveSPWebName
        /// <summary>
        /// Resolves an SPWeb GUID and an SPSite GUID into an SPWeb name.
        /// </summary>
        ///
        /// <param name="spWebGuid">The GUID of the SPWeb.</param>
        ///
        /// <param name="spSiteGuid">The GUID of the SPSite containing the SPWeb.</param>
		///
		/// <param name="spWebName">Where to store the name of the SPWeb, or <c>null</c> if the
		/// 	SPWeb cannot be found.</param>
		///
		/// <param name="spWebUrl">Where to store the URL of the SPWeb, or <c>null</c> if the SPWeb
		///     cannot be found.</param>
        ///
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        protected void ResolveSPWebName(Guid spWebGuid, Guid spSiteGuid, out string spWebName,
			out string spWebUrl)
        {

            //Restore previously assigned value 
            bool previousValue = SPSecurity.CatchAccessDeniedException;

            // To check the user permission to access this site.
            SPSecurity.CatchAccessDeniedException = false;

            // if <m_spWebNameMap> wasn't initialized, initialize it now
            if (m_spWebNameMap == null)
            {
                m_spWebNameMap = new Dictionary<Guid, WebNameAndUrl>(100);
            }

            WebNameAndUrl webNameAndUrl;

            // if the web name is in the dictionary, return it
            if (m_spWebNameMap.TryGetValue(spWebGuid, out webNameAndUrl))
            {
                spWebName = webNameAndUrl.Name;
                spWebUrl = webNameAndUrl.Url;
                return;
            }

            try
            {
                using(SPSite spSite = new SPSite(spSiteGuid,SPContext.Current.Site.Zone))
                {
                    using(SPWeb spWeb = spSite.OpenWeb(spWebGuid))
                    {
                        spWebName = spWeb.Title;
                        spWebUrl = spWeb.Url;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //The Site  does not exist : SPWeb not available. 
                //Add the site to unknown site collection
                m_unknownSiteCount++; //increment the Site Count by 1;
                spWebName = String.Format(CultureInfo.CurrentCulture,
                                          AppResources.AlwpUnknownSite, 
                                          m_unknownSiteCount);
                spWebUrl = null;
            }
            catch (UnauthorizedAccessException)
            {
                // the user doesn't have permission to access this site.
                //Set the SPWeb Title as Unknown Site #

                m_unknownSiteCount++; //increment the Site Count by 1;
                spWebName = String.Format(CultureInfo.CurrentCulture, 
                                          AppResources.AlwpUnknownSite, 
                                          m_unknownSiteCount);
                spWebUrl = null;
            }
            finally
            {
                //assign back previously assigned value
                SPSecurity.CatchAccessDeniedException = previousValue;
            }
            // update the collection
            webNameAndUrl = new WebNameAndUrl();
            webNameAndUrl.Name = spWebName;
            webNameAndUrl.Url = spWebUrl;
            m_spWebNameMap[spWebGuid] = webNameAndUrl;
        }
        #endregion

        #region RenderQueryResults
        /// <summary>
        /// Renders the entire query results in the inner table.
        /// </summary>
        /// <param name="queryDef">The definition of the query that was executed.</param> 
        /// <param name="renderedRows">Rendered query results.</param>
        /// <param name="hw">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        void RenderQueryResults(QueryDefinition queryDef,
                                List<RenderedCell[]> renderedRows,
                                HtmlTextWriter hw)
        {
            // get the column definitions
            IList<ColumnDefinition> columnDefs = queryDef.Columns;

            //Get the column index to sort on and sort order
            int sortColumnIndex;
            bool sortAscending = GetSortIndex(out sortColumnIndex);

            // sort <renderedRows> if so specified
            if ((sortColumnIndex >= 0) && (sortColumnIndex < queryDef.Columns.Count))
                QueryDefinition.SortRenderedRows(renderedRows, sortColumnIndex, sortAscending);

            // render the "<table>" element and its contents 
            hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-summarystandardbody");
            // skipped: id=TABLE1 dir=None
            hw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            hw.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "1");
            hw.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            hw.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, hw))
            {
                // render the header row 
                hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-viewheadertr");
                hw.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, hw))
                {
                    // render the column headers
                    int columnIndex = 0;
                    foreach (ColumnDefinition columnDef in columnDefs)
                    {
                        bool? ascendingSort;
                        if (sortColumnIndex == columnIndex)
                            ascendingSort = sortAscending;
                        else
                            ascendingSort = null;
                        RenderColumnHeader(columnDef, columnIndex, ascendingSort, hw);
                        columnIndex++;
                    }
                }

                //If No Items Found
                if (renderedRows.Count == 0)
                {
                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
                    {
                        hw.AddAttribute(HtmlTextWriterAttribute.Colspan,
                                        columnDefs.Count.ToString(CultureInfo.InvariantCulture));
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                        {
                            SlkError slkError
                                = new SlkError(ErrorType.Info,
                                                Constants.Space +
                                                Constants.Space +
                                                AppResources.AlwpNoItemFound);
                            WebParts.ErrorBanner.RenderErrorItems(hw, slkError);
                        }
                    }
                }
                else
                {
                    // render the rows
                    int rowIndex = 0;
                    foreach (RenderedCell[] renderedRow in renderedRows)
                    {
                        // render the "<tr>"; note that every other row is shaded ("ms-alternating")
                        hw.AddAttribute(HtmlTextWriterAttribute.Class,
                            (((rowIndex & 1) == 0) ? "ms-alternating" : ""));
                        using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
                        {
							// set <webNameRenderedCell> to any cell in the row which is of type
							// WebNameRenderedCell, i.e. which refers to a SharePoint Web site,
							// or null if none
                            WebNameRenderedCell webNameRenderedCell = null;
                            foreach (RenderedCell renderedCell in renderedRow)
                            {
								if (webNameRenderedCell == null)
									webNameRenderedCell = renderedCell as WebNameRenderedCell;
                            }

                            // render the cells in this row
                            int columnIndex = 0;
                            foreach (RenderedCell renderedCell in renderedRow)
                            {
                                ColumnDefinition columnDef = columnDefs[columnIndex];
                                hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb2");
                                if (!columnDef.Wrap)
                                    hw.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                                bool isObserver = IsObserver;
                                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                                    RenderColumnCell(renderedCell, webNameRenderedCell, hw, isObserver, SlkStore);
                                columnIndex++;
                            }
                        }
                        rowIndex++;
                    }
                }


            }
        }
        #endregion

        #region RenderColumnHeader
        /// <summary>
        /// Renders a column header, i.e. the label at the top of a column in the query results.
        /// </summary>
        ///
        /// <param name="columnDef">The <c>ColumnDefinition</c> of the column being rendered.</param>
        /// 
        /// <param name="columnIndex">The zero-based index of this column.</param>
        /// 
        /// <param name="ascendingSort"><c>true</c> if this column is currently being sorted on in an
        ///     ascending order; <c>false</c> if this column is currently being sorted on in a
        ///     descending order; <c>null</c> if this column is not being sorted on.</param>
        ///
        /// <param name="hw">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        private void RenderColumnHeader(ColumnDefinition columnDef, int columnIndex, bool? ascendingSort,
        HtmlTextWriter hw)
        {
            // render the "<th>" element for this column header
            hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vh2");
            hw.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
            hw.AddAttribute(HtmlTextWriterAttribute.Style, "border-left: none; padding-left: 6px;");
            using (new HtmlBlock(HtmlTextWriterTag.Th, 1, hw))
            {
                // render the "<div>" element containing the column header
                hw.WriteLine();
                hw.AddAttribute(HtmlTextWriterAttribute.Style,
                    "position: relative; left: 0px; top: 0px; width: 100%;");
                using (new HtmlBlock(HtmlTextWriterTag.Div, 1, hw))
                {
                    // render the "<table>" element containing the column header
                    hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-unselectedtitle");
                    hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 100%;");
                    hw.AddAttribute(HtmlTextWriterAttribute.Height, "100%");
                    hw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    using (new HtmlBlock(HtmlTextWriterTag.Table, 0, hw))
                    {
                        using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, hw))
                        {
                            hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                            hw.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                            hw.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 0, hw))
                            {
                                // write the "<a>" element which, when clicked, causes the query list
                                // to be re-sorted as follows: if the clicked-on column header is the
                                // one currently sorted on, then the sort is reversed, otherwise the
                                // results are sorted in ascending order on the clicked-on column
                                int newSort;
                                if (ascendingSort != null)
                                {
                                    if (ascendingSort.Value)
                                        newSort = -(columnIndex + 1); // switch to descending sort
                                    else
                                        newSort = columnIndex + 1; // switch to ascending sort
                                }
                                else
                                    newSort = columnIndex + 1;
                                hw.AddAttribute(HtmlTextWriterAttribute.Title,
                                    String.Format(CultureInfo.InvariantCulture,
                                                  "Sort by {0}", 
                                                  columnDef.Title));
                                hw.AddAttribute(HtmlTextWriterAttribute.Href,
                                    GetAdjustedQueryString(QueryStringKeys.Sort, 
                                                           newSort.ToString(CultureInfo.InvariantCulture)));
                                hw.AddAttribute(HtmlTextWriterAttribute.Style, "color:Gray;");
                                using (new HtmlBlock(HtmlTextWriterTag.A, 0, hw))
                                {
                                    // write the column title
                                    hw.WriteEncodedText(columnDef.Title);

                                    // write the sort ascending/descending icon, if we're currently
                                    // sorting on this column
                                    if (ascendingSort != null)
                                    {
                                        hw.AddAttribute(HtmlTextWriterAttribute.Src,
                                            (ascendingSort.Value ? "/_layouts/images/sort.gif"
                                                : "/_layouts/images/rsort.gif"));
                                        hw.AddAttribute(HtmlTextWriterAttribute.Alt, "Sort Ascending");
                                        hw.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Img, 0, hw);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region RenderColumnCell
        /// <summary>
        /// Renders a column cell, i.e. one column of one row in the query results.
        /// </summary>
        ///
        /// <param name="renderedCell">The value to render, from the query results.</param>
        ///
        /// <param name="webNameRenderedCell">If the row containing this cell also contains a cell
        ///     of type <c>WebNameRenderedCell</c>, i.e. a cell referring to a SharePoint Web site,
        ///     this parameter refers to that cell.  Otherwise, this parameter is <c>null</c>.
        ///     </param>
        ///
        /// <param name="hw">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        static void RenderColumnCell(RenderedCell renderedCell,
            WebNameRenderedCell webNameRenderedCell, HtmlTextWriter hw, bool isObserver, SlkStore slkStore)
        {
            // render the cell contents inside a "<span>" (not sure why SharePoint uses a "<span>"
            // here, but I'm copying what they do)
            if (renderedCell.ToolTip != null)
                hw.AddAttribute(HtmlTextWriterAttribute.Title, renderedCell.ToolTip);
            using (new HtmlBlock(HtmlTextWriterTag.Span, 0, hw))
            {
                if (renderedCell.Id != null)
                {
                    if (renderedCell.Id.ItemTypeName == Schema.AssignmentItem.ItemTypeName)
                    {
                        // render a link to the Instructor Assignment Properties page
                        string url = "Grading.aspx?AssignmentId={0}";
                        if ((webNameRenderedCell != null) &&
                            (webNameRenderedCell.SPWebUrl != null))
                            url = webNameRenderedCell.SPWebUrl + "/_layouts/SharePointLearningKit/" + url;
                        hw.AddAttribute(HtmlTextWriterAttribute.Target, "_parent");
                        hw.AddAttribute(HtmlTextWriterAttribute.Href,
                            String.Format(CultureInfo.InvariantCulture, url,
                                          renderedCell.Id.GetKey()));
                        using (new HtmlBlock(HtmlTextWriterTag.A, 0, hw))
                            hw.WriteEncodedText(renderedCell.ToString());
                    }
                    else
                    if (renderedCell.Id.ItemTypeName == Schema.LearnerAssignmentItem.ItemTypeName)
                    {
                        Guid learnerAssignmentGuidId = slkStore.GetLearnerAssignmentGuidId(renderedCell.Id);
                        if (isObserver)
                        {
                            // Display this cell as an url and clicking this url will launch frameset in StudentReview mode
                            string url = "Frameset/Frameset.aspx?SlkView=StudentReview&{0}={1}";
                            if ((webNameRenderedCell != null) &&
                                (webNameRenderedCell.SPWebUrl != null))
                                url = webNameRenderedCell.SPWebUrl + "/_layouts/SharePointLearningKit/" + url;
                            hw.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
                            hw.AddAttribute(HtmlTextWriterAttribute.Href,
                                String.Format(CultureInfo.InvariantCulture, url,
                                              FramesetQueryParameter.LearnerAssignmentId,
                                              learnerAssignmentGuidId.ToString()));
                            using (new HtmlBlock(HtmlTextWriterTag.A, 0, hw))
                                hw.WriteEncodedText(renderedCell.ToString());

                        }
                        else
                        {

                            // render a link to the Learner Assignment Properties page
                            string url = "Lobby.aspx?{0}={1}";
                            if ((webNameRenderedCell != null) &&
                                (webNameRenderedCell.SPWebUrl != null))
                                url = webNameRenderedCell.SPWebUrl + "/_layouts/SharePointLearningKit/" + url;
                            hw.AddAttribute(HtmlTextWriterAttribute.Target, "_parent");
                            hw.AddAttribute(HtmlTextWriterAttribute.Href,
                                String.Format(CultureInfo.InvariantCulture, url,
                                              FramesetQueryParameter.LearnerAssignmentId,
                                              learnerAssignmentGuidId.ToString()));
                            using (new HtmlBlock(HtmlTextWriterTag.A, 0, hw))
                                hw.WriteEncodedText(renderedCell.ToString());
                        }
                    }
                    else
                        hw.WriteEncodedText(renderedCell.ToString());
                }
                else
                    hw.WriteEncodedText(renderedCell.ToString());
            }
        }
        #endregion

        #region RenderQueryCount
        /// <summary>
        /// Renders the Javascript that has function to set the query result counts.
        /// Gets the Query Count from results Collections and sets the count and 
        /// invokes the QuerySummary Frame to Set the Count
        /// </summary>
        private void RenderQueryCount(string queryCount)
        {
            // render the HTML for the page
            using (HtmlTextWriter hw = new HtmlTextWriter(Response.Output, "  "))
            {
                // render the "<html>" element and its contents
                using (new HtmlBlock(HtmlTextWriterTag.Html, 0, hw))
                {
                    // write script code that contains the query result counts
                    using (new HtmlBlock(HtmlTextWriterTag.Script, 0, hw))
                    {   
                        hw.WriteLine(@"
                        var queryFrameName = window.frameElement.name + ""_AlwpQuerySummary"";
                        var queryFrame = parent.frames[queryFrameName];");
                        hw.WriteLine(@"
                        if(queryFrame != undefined)
                        {
                            if (queryFrame.SetQueryCount != undefined)
                            {");
                        hw.WriteLine(@"
                             " + String.Format(CultureInfo.InvariantCulture,
                                               "queryFrame.SetQueryCount(\"{0}\",\"{1}\");",
                                               Query, queryCount));
                        hw.WriteLine(@"
                            }
                        }");                             

                    }
                }
            }

        }
        #endregion
        
        #endregion
    }

    /// <summary>
    /// Contains the name and URL of a SharePoint SPWeb.
    /// </summary>
    internal class WebNameAndUrl
    {
        /// <summary>
        /// The name of an SPWeb.
        /// </summary>
        public string Name;

        /// <summary>
        /// The URL of an SPWeb.
        /// </summary>
        public string Url;
    }
}
