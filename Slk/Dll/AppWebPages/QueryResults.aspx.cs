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
using System.Text;
using System.Configuration;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    /// <summary>The assignment list web part query results page.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Alwp")]
    public partial class AlwpQueryResults : QueryBasePage
    {
        #region Private Variables
        string queryName;
        /// <summary>
        /// SPWeb GUID-to-name mappings.
        /// </summary>
        Dictionary<Guid, WebNameAndUrl> m_spWebNameMap;
        /// <summary>
        /// Holds the Sort
        /// </summary>
        string m_sort;
        /// <summary>
        /// Holds the Unknown Site Count  This is the number of SPWeb sites for which
        /// we cannot determine the site name.
        /// </summary>
        int m_unknownSiteCount;

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
                if (queryName == null)
                {
                    queryName = Request[QueryStringKeys.Query];
                }
                return queryName;
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
                {
                    m_sort = Request[QueryStringKeys.Sort];
                }
                return m_sort;
            }
        }

        #endregion

        #region Private and Protected Methods

        #region Page_Load
        private void AddCoreCss(HtmlTextWriter writer, int lcid)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
            string cssUrl = String.Format(CultureInfo.InvariantCulture, "/_layouts/{0}/styles/core.css", lcid);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, cssUrl);
            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 1, writer);
        }

        /// <summary>
        ///  Page Load for AlwpQueryResults. 
        /// </summary> 
        /// <param name="sender">an object referencing the source of the event</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
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
                            // Include 1033 one as a back up
                            AddCoreCss(hw, 1033);
                            SlkCulture culture = new SlkCulture(SPWeb);
                            AddCoreCss(hw, culture.Culture.LCID);

#if SP2007
                            //Adds the Theme Css Url to Enable Theming in the frame.
                            hw.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                            hw.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                            hw.AddAttribute(HtmlTextWriterAttribute.Href, SPWeb.ThemeCssUrl);
                            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 0, hw);
#endif

                            // create a link to ALWP's "Styles.css"
                            hw.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                            hw.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                            hw.AddAttribute(HtmlTextWriterAttribute.Href, "Include/Styles.css");
                            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 0, hw);
                        }

                        // render the "<body>" element and its contents
                        hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 100%; overflow-y: auto; overflow-x: auto;");

                        using (new HtmlBlock(HtmlTextWriterTag.Body, 0, hw))
                        {
                            Microsoft.SharePointLearningKit.WebParts.AssignmentListWebPart.DumpCultures(hw);
                            // render the outer table -- this contains only one row and one column, which
                            // in turn contains the entire query results table
                            hw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                            hw.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
                            hw.AddAttribute(HtmlTextWriterAttribute.Border, "0");

                            RenderForm(hw);
                            RenderSortFunction(hw);

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
                                            QueryDefinition queryDef = null;
                                            if (Query != null)
                                            {
                                                queryDef = SlkStore.Settings.FindQueryDefinition(Query);
                                            }

                                            if (queryDef == null)
                                            {
                                                throw new SafeToDisplayException (PageCulture.Resources.AlwpQuerySetNotFound, Query);
                                            }


                                            List<RenderedCell[]> renderedRows = PerformQuery(queryDef);

                                            //Set the QueryCount 
                                            queryCount = renderedRows.Count.ToString(CultureInfo.InvariantCulture);
                                            //Renders the Result
                                            RenderQueryResults(queryDef, renderedRows, hw);
                                        }
                                        catch (Exception ex)
                                        {
                                            queryCount = PageCulture.Resources.AlwpQueryResultError;
                                            SlkError slkError;
                                            //Handles SqlException separate to capture the deadlock 
                                            //and treat it differently
                                            SqlException sqlEx = ex as SqlException;
                                            if (sqlEx != null)
                                            {
                                                slkError = WebParts.ErrorBanner.WriteException(SlkStore, sqlEx);
                                            }
                                            else
                                            {
                                                slkError = SlkError.WriteException(SlkStore, ex);
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
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
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

            // create a query based on <queryDef>
            LearningStoreQuery query = null;
            int[,] columnMap; // see QueryDefinition.CreateQuery
            try
            {
                query = CreateStandardQuery(queryDef, false, out columnMap);
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
            List<RenderedCell[]> renderedRows = new List<RenderedCell[]>(queryResults.Rows.Count);
            foreach (DataRow dataRow in queryResults.Rows)
            {
                RenderedCell[] renderedCells = queryDef.RenderQueryRowCells(dataRow, columnMap, ResolveSPWebName, SPWeb.RegionalSettings.TimeZone);
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
        ///     currently sorted on, or -1 if the results are not sorted.</param>
        /// <returns><c>true</c> if the query results are currently sorted in an
        ///          an ascending order, false for an descending-order sort.  
        ///          Irrelevant if    <paramref name="sortColumnIndex"/> is -1.</returns>
        private bool GetSortIndex(out int sortColumnIndex)
        {

            bool sortAscending;

            if (string.IsNullOrEmpty(Sort) == false)
            {
                try
                {
                    sortColumnIndex = int.Parse(Sort, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    sortColumnIndex = -1;
                    return true;
                }

                if (sortColumnIndex < 0)
                {
                    sortAscending = false;
                    sortColumnIndex = -sortColumnIndex;
                }
                else
                {
                    sortAscending = true;
                }

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
        ///     SPWeb cannot be found.</param>
        ///
        /// <param name="spWebUrl">Where to store the URL of the SPWeb, or <c>null</c> if the SPWeb
        ///     cannot be found.</param>
        ///
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        protected void ResolveSPWebName(Guid spWebGuid, Guid spSiteGuid, out string spWebName, out string spWebUrl)
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
            catch (SqlException)
            {
                //The database the site is in is not accessible
                //Add the site to unknown site collection
                m_unknownSiteCount++; //increment the Site Count by 1;
                spWebName = PageCulture.Format(PageCulture.Resources.AlwpUnknownSite, m_unknownSiteCount);
                spWebUrl = null;
            }
            catch (FileNotFoundException)
            {
                //The Site  does not exist : SPWeb not available. 
                //Add the site to unknown site collection
                m_unknownSiteCount++; //increment the Site Count by 1;
                spWebName = PageCulture.Format(PageCulture.Resources.AlwpUnknownSite, m_unknownSiteCount);
                spWebUrl = null;
            }
            catch (UnauthorizedAccessException)
            {
                // the user doesn't have permission to access this site.
                //Set the SPWeb Title as Unknown Site #
                m_unknownSiteCount++; //increment the Site Count by 1;
                spWebName = PageCulture.Format(PageCulture.Resources.AlwpUnknownSite, m_unknownSiteCount);
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
                queryDef.SortRenderedRows(renderedRows, sortColumnIndex, sortAscending);

            // render the "<table>" element and its contents 
            hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-summarystandardbody");
            // skipped: id=TABLE1 dir=None
            hw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            hw.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "1");
            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
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
                                                PageCulture.Resources.AlwpNoItemFound);
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
                            Guid learnerAssignmentGUID = Guid.Empty;
                            foreach (RenderedCell renderedCell in renderedRow)
                            {
                                if (webNameRenderedCell == null)
                                {
                                    webNameRenderedCell = renderedCell as WebNameRenderedCell;
                                }

                                if (learnerAssignmentGUID == Guid.Empty)
                                {
                                    if (renderedCell.Id != null)
                                    {
                                        if (renderedCell.Id.ItemTypeName == Schema.LearnerAssignmentItem.ItemTypeName)
                                        {
                                            learnerAssignmentGUID = SlkStore.GetLearnerAssignmentGuidId(renderedCell.Id);
                                        }
                                    }
                                }

                                if (webNameRenderedCell != null && learnerAssignmentGUID != Guid.Empty)
                                {
                                    break;
                                }
                            }

                            // render the cells in this row
                            int columnIndex = 0;
                            foreach (RenderedCell renderedCell in renderedRow)
                            {
                                ColumnDefinition columnDef = columnDefs[columnIndex];
                                hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb2");
                                if (!columnDef.Wrap)
                                {
                                    hw.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                                }
                                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                                {
                                    if (columnDef.Title.Equals(PageCulture.Resources.AlwpFileSubmissionColumnTitle))
                                    {
                                        RenderFileSubmissionCell(renderedCell, webNameRenderedCell, learnerAssignmentGUID, hw);
                                    }
                                    else    
                                    {
                                        RenderColumnCell(renderedCell, webNameRenderedCell, hw, SlkStore);
                                    }
                                }

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
                    hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 100%;height: 100%;");
                    hw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    using (new HtmlBlock(HtmlTextWriterTag.Table, 0, hw))
                    {
                        using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, hw))
                        {
                            hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                            hw.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
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
                                hw.AddAttribute(HtmlTextWriterAttribute.Title, PageCulture.Format(PageCulture.Resources.QueryResultsSortBy, columnDef.Title));
                                hw.AddAttribute(HtmlTextWriterAttribute.Href,
                                    GetAdjustedQueryString(QueryStringKeys.Sort, newSort.ToString(CultureInfo.InvariantCulture)));
                                hw.AddAttribute(HtmlTextWriterAttribute.Style, "color:Gray;");
                                hw.AddAttribute("onclick", string.Format(CultureInfo.InvariantCulture, "sort({0}); return false;", newSort));
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

        private void RenderSortFunction(TextWriter writer)
        {
            string function = @"function sort(col)
            {
                var sortInput = document.getElementById('alwpSort');
                sortInput.value = col;
                var formInput = document.getElementById('resultsForm');
                formInput.submit();

                if(queryFrame != undefined)
                {
                    if (queryFrame.SetSort != undefined)
                    {
                        queryFrame.SetSort(col);
                    }
                }
            }
            ";
            writer.WriteLine("<script  type=\"text/javascript\">");
            writer.WriteLine(function);
            writer.WriteLine("</script>");
        }

        private void RenderForm(TextWriter writer)
        {
            writer.Write("<form id=\"resultsForm\" method=\"POST\" >");
            AddInputLine(writer, QueryStringKeys.QuerySet);

            writer.Write("<input id=\"alwp{0}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.Query, HttpUtility.HtmlEncode(Query));

            // RawSourceUrl is already sanitized in SlkAppBasePage.
            writer.Write("<input id=\"alwp{0}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.Source, RawSourceUrl);
            AddInputLine(writer, QueryStringKeys.SPWebScope);

            // FrameId sanitized in QueryBasePage
            writer.Write("<input id=\"alwp{0}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.FrameId, FrameId);

            // Sort is an int and so doesn't need encoding
            writer.Write("<input id=\"alwp{0}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.Sort, Sort);
            AddInputLine(writer, QueryStringKeys.ForObserver, QueryStringKeys.SPWebScope);

            writer.Write("</form>");
        }

        private void AddInputLine(TextWriter writer, string key)
        {
            AddInputLine(writer, key, key);
        }

        private void AddInputLine(TextWriter writer, string idKey, string requestKey)
        {
            writer.Write("<input id=\"alwp{0}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", idKey, HttpUtility.HtmlEncode(Request[requestKey]));
        }

        #region RenderFileSubmissionCell

        /// <summary>
        /// Renders the File Submission column cell
        /// </summary>
        /// <param name="renderedCell">The value to render, from the query results.</param>
        /// <param name="webNameRenderedCell"></param>
        /// <param name="learnerAssignmentGUID">The GUID of the current assignment</param>
        /// <param name="hw">The HtmlTextWriter to write to.</param>
        void RenderFileSubmissionCell(RenderedCell renderedCell, WebNameRenderedCell webNameRenderedCell, Guid learnerAssignmentGUID, HtmlTextWriter hw)
        {
            // This is a bit of a hack since the query returns an unlocalized string. Next time the schema is changed would be best
            // to change to return integer values representing these. No point making a sql change just for this though.
            if (renderedCell.ToString() == "Submit File(s)")
            {
                string baseUrl = "{0}{1}FilesUploadPage.aspx?LearnerAssignmentId={2}&Source={3}";
                // Creating a format string from a format string
                baseUrl = string.Format(CultureInfo.InvariantCulture, baseUrl, "{0}", Constants.SlkUrlPath, "{1}", RawSourceUrl);
                RenderFileSubmissionCellAsSubmitLink(baseUrl, webNameRenderedCell.SPWebUrl, learnerAssignmentGUID,
                                                        PageCulture.Format(PageCulture.Resources.AlwpFileSubmissionSubmitText), hw);
            }
            else if (renderedCell.ToString() == "Submitted LINK")
            {
                RenderFileSubmissionCellAsSubmittedLink(
                    "{0}" + Constants.SlkUrlPath + "SubmittedFiles.aspx?LearnerAssignmentId={1}",
                    webNameRenderedCell,
                    learnerAssignmentGUID,
                    PageCulture.Format(PageCulture.Resources.LearnerAssignmentStatusCompleted),
                    hw);
            }
            else if (renderedCell.ToString() == "Submitted")
            {
                hw.WriteEncodedText(PageCulture.Format(PageCulture.Resources.LearnerAssignmentStatusCompleted));
            }
            else if (renderedCell.ToString() == "Not Available")
            {
                hw.WriteEncodedText(PageCulture.Format(PageCulture.Resources.GradingFileSubmissionNA));
            }
            else
            {
                hw.WriteEncodedText(renderedCell.ToString());
            }
        }

        #endregion

        #region RenderFileSubmissionCellAsSubmittedLink

        /// <summary>
        /// Renders The File Submission column cell as "Submitted" link
        /// </summary>
        /// <param name="fileURL">The URL of the file to be redirected to when the cell link is clicked</param>
        /// <param name="webNameRenderedCell"></param>
        /// <param name="learnerAssignmentGUID">The GUID of the current assignment</param>
        /// <param name="renderedCellValue">The text to be displayed in the cell</param>
        /// <param name="hw">The HtmlTextWriter to write to.</param>
        void RenderFileSubmissionCellAsSubmittedLink(string fileURL, WebNameRenderedCell webNameRenderedCell, Guid learnerAssignmentGUID, 
            string renderedCellValue, HtmlTextWriter hw)
        {
            // Optimise to always to to submitted files. The check involves
            // 1.   Loading the assignment properties which required opening the web.
            // 2.   Loading all the submitted files via drop box manager which required opening the web.
            // 3.   Then if there's only 1 of them, having a link direct to that.
            // That's a lot of work just to create a link.
            string url = string.Empty;
            //string url = CheckSubmittedFilesNumber(learnerAssignmentGUID);

            if (url.Equals(string.Empty))
            {
                StringBuilder pageURL = new StringBuilder();
                pageURL.AppendFormat(fileURL, webNameRenderedCell.SPWebUrl, learnerAssignmentGUID.ToString());

                url = pageURL.ToString();
            }

            StringBuilder anchorOnClick = new StringBuilder();
            anchorOnClick.AppendFormat("{0}{1}{2}", "window.open('", url, 
                                "','popupwindow','width=400,height=300,scrollbars,resizable');return false; ");

            hw.AddAttribute(HtmlTextWriterAttribute.Onclick, anchorOnClick.ToString());
            hw.AddAttribute(HtmlTextWriterAttribute.Href, url);

            using (new HtmlBlock(HtmlTextWriterTag.A, 0, hw))
            {
                hw.WriteEncodedText(renderedCellValue);
            }
        }

        #endregion

        #region RenderFileSubmissionCellAsSubmitLink

        /// <summary>
        /// Renders The File Submission column cell as "Submit File(s)" link
        /// </summary>
        /// <param name="fileURL">The URL of the file to be redirected to when the cell link is clicked</param>
        /// <param name="webUrl">The web url</param>
        /// <param name="learnerAssignmentGUID">The GUID of the current assignment</param>
        /// <param name="renderedCellValue">The text to be displayed in the cell</param>
        /// <param name="hw">The HtmlTextWriter to write to.</param>
        void RenderFileSubmissionCellAsSubmitLink(string fileURL, string webUrl, Guid learnerAssignmentGUID, string renderedCellValue, HtmlTextWriter hw)
        {

            StringBuilder url = new StringBuilder();
            url.AppendFormat(fileURL, webUrl, learnerAssignmentGUID.ToString());

            hw.AddAttribute(HtmlTextWriterAttribute.Target, "_top");
            hw.AddAttribute(HtmlTextWriterAttribute.Href, url.ToString());

            using (new HtmlBlock(HtmlTextWriterTag.A, 0, hw))
            {
                hw.WriteEncodedText(renderedCellValue);
            }
        }

        #endregion

        #region RenderColumnCell
        /// <summary>
        /// Renders a column cell, i.e. one column of one row in the query results.
        /// </summary>
        /// <param name="renderedCell">The value to render, from the query results.</param>
        /// <param name="webNameRenderedCell">If the row containing this cell also contains a cell
        ///     of type <c>WebNameRenderedCell</c>, i.e. a cell referring to a SharePoint Web site,
        ///     this parameter refers to that cell.  Otherwise, this parameter is <c>null</c>.
        ///     </param>
        /// <param name="hw">The <c>HtmlTextWriter</c> to write to.</param>
        /// <param name="slkStore">The SlkStore to use to get assignment information from.</param>
        void RenderColumnCell(RenderedCell renderedCell, WebNameRenderedCell webNameRenderedCell, HtmlTextWriter hw, SlkStore slkStore)
        {
            // render the cell contents inside a "<span>" (not sure why SharePoint uses a "<span>"
            // here, but I'm copying what they do)
            if (renderedCell.ToolTip != null)
                hw.AddAttribute(HtmlTextWriterAttribute.Title, renderedCell.ToolTip);
            using (new HtmlBlock(HtmlTextWriterTag.Span, 0, hw))
            {
                if (webNameRenderedCell == renderedCell && webNameRenderedCell.RenderAsLink)
                {
                    RenderCellWithLink(webNameRenderedCell, hw, renderedCell.ToString(), "#", "_parent");
                }
                else if (renderedCell.Id != null)
                {
                    if (renderedCell.Id.ItemTypeName == Schema.AssignmentItem.ItemTypeName)
                    {
                        // render a link to the Instructor Assignment Properties page
                        string url = "Grading.aspx?AssignmentId={0}";
                        RenderCellWithLink(webNameRenderedCell, hw, renderedCell.ToString(), url, "_parent", renderedCell.Id.GetKey().ToString(CultureInfo.InvariantCulture));
                    }
                    else if (renderedCell.Id.ItemTypeName == Schema.LearnerAssignmentItem.ItemTypeName)
                    {
                        Guid learnerAssignmentGuidId = slkStore.GetLearnerAssignmentGuidId(renderedCell.Id);
                        if (IsObserver)
                        {
                            // Display this cell as an url and clicking this url will launch frameset in StudentReview mode
                            string url = "Frameset/Frameset.aspx?SlkView=StudentReview&{0}={1}";
                            RenderCellWithLink(webNameRenderedCell, hw, renderedCell.ToString(), url, "_blank", FramesetQueryParameter.LearnerAssignmentId, learnerAssignmentGuidId.ToString());
                        }
                        else
                        {
                            // render a link to the Learner Assignment Properties page
                            string url = "Lobby.aspx?{0}={1}&{2}={3}";
                            RenderCellWithLink(webNameRenderedCell, hw, renderedCell.ToString(), url, "_parent", 
                                    FramesetQueryParameter.LearnerAssignmentId, learnerAssignmentGuidId.ToString(), QueryStringKeys.Source, RawSourceUrl);
                        }
                    }
                    else
                    {
                        hw.WriteEncodedText(renderedCell.ToString());
                    }
                }
                else
                {
                    hw.WriteEncodedText(renderedCell.ToString());
                }
            }
        }
        #endregion

        private void RenderCellWithLink(WebNameRenderedCell webNameRenderedCell, HtmlTextWriter writer, string text, string url, string target, params string[] urlArguments)
        {
            if ((webNameRenderedCell != null) && (webNameRenderedCell.SPWebUrl != null))
            {
                if (url == "#")
                {
                    url = webNameRenderedCell.SPWebUrl;
                }
                else
                {
                    url = webNameRenderedCell.SPWebUrl + Constants.SlkUrlPath + url;
                }
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Target, target);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, String.Format(CultureInfo.InvariantCulture, url, urlArguments));
            using (new HtmlBlock(HtmlTextWriterTag.A, 0, writer))
            {
                writer.WriteEncodedText(text);
            }
        }

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

        #region CheckSubmittedFilesNumber

        /// <summary>
        /// Checks the number of the assignment submitted files. 
        /// </summary>
        /// <param name="learnerAssignmentGUID">The assignment GUID</param>
        /// <returns> If one assignment submitted, returns its URL.
        /// If more than one, returns an empty string.</returns>
        private string CheckSubmittedFilesNumber(Guid learnerAssignmentGUID)
        {
            AssignmentProperties properties = SlkStore.LoadAssignmentPropertiesForLearner(learnerAssignmentGUID, SlkRole.Learner);
            return PerformFilesNumberChecking(properties);
        }

        #endregion

        #region PerformFilesNumberChecking

        /// <summary>
        /// Checks the number of the assignment submitted files. 
        /// </summary>
        /// <param name="assignmentProperties">The AssignmentProperties</param>
        /// <returns> If one assignment submitted, returns its URL.
        /// If more than one, returns an empty string.</returns>
        private string PerformFilesNumberChecking(AssignmentProperties assignmentProperties)
        {
            DropBoxManager dropBox = new DropBoxManager(assignmentProperties);
            AssignmentFile[] assignmentFiles = dropBox.LastSubmittedFiles(assignmentProperties.Results[0].User.SPUser, false);

            if (assignmentFiles.Length != 1)
            {
                return string.Empty;
            }
            else
            {
                return assignmentFiles[0].Url;
            }
        }
        #endregion


        #region ApplyObserverReadAccessPermissions

        /// <summary>
        /// Gives the observer read access on the assignment folder and the sub folder of his child.
        /// </summary>
        /// <param name="assignmentSubFolder">The assignment subfolder</param>
        private void ApplyObserverReadAccessPermissions(SPListItem assignmentSubFolder)
        {
            SPListItem assignmentFolder = assignmentSubFolder.Folder.ParentFolder.Item;
            SPWeb web = assignmentSubFolder.Web;

            SPRoleDefinition roleDefinition = web.RoleDefinitions["Read"];
            SPRoleAssignment roleAssignment = new SPRoleAssignment(SPContext.Current.Web.CurrentUser);
            roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

            web.AllowUnsafeUpdates = true;

            assignmentFolder.RoleAssignments.Add(roleAssignment);
            assignmentFolder.Update();

            assignmentSubFolder.RoleAssignments.Add(roleAssignment);
            assignmentSubFolder.Update();

            web.AllowUnsafeUpdates = false;
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
