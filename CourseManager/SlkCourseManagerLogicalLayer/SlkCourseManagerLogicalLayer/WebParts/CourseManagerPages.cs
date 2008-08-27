using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axelerate.BusinessLayerUITools.WebParts;
using Microsoft.SharePoint;

namespace Axelerate.SlkCourseManagerLogicalLayer.WebParts
{
    /// <summary>
    /// This class represents the Course Manager Pages Web Part.
    /// </summary>
    public class CourseManagerPages : Microsoft.SharePoint.WebPartPages.WebPart
    {
        /// <summary>
        /// Main content of the Web Part.
        /// </summary>
        wptBOFact MainContent = new wptBOFact();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CourseManagerPages()
            : base()
        {
            MainContent.ID = this.ID + "MAINCONTENT";
            MainContent.FactoryMethod = "";
            MainContent.FactoryParameters = "";
            MainContent.ClassName = "Axelerate.SlkCourseManagerLogicalLayer.clsActivityGroup, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4";
            MainContent.ContentType = wptBOFact.FactContentType.HTML;
            MainContent.Content = "<a href=\"" + SPContext.Current.Web.Url + "/CourseManagerPages/Forms/Plan_and_assign.aspx\">Plan and Assign</a><br><br><a href=\"" + SPContext.Current.Web.Url + "/CourseManagerPages/Forms/Monitor_and_assess.aspx\">Monitor and Assess</a>";
        }

        /// <summary>
        /// Override of the Create Child Control method
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Add(MainContent);
        }        
    }
}
