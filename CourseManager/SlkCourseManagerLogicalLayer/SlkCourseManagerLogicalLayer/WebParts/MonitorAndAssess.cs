using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axelerate.BusinessLayerUITools.WebParts;
using Microsoft.SharePoint;

namespace Axelerate.SlkCourseManagerLogicalLayer.WebParts
{
    /// <summary>
    /// This class represents the Monitor and Assess Web Part.
    /// </summary>
    public class MonitorAndAssess: Microsoft.SharePoint.WebPartPages.WebPart
    {
        /// <summary>
        /// Main Grid of the web part.
        /// </summary>
        wptHyperGrid MainGrid = new wptHyperGrid();        

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MonitorAndAssess()
            : base()
        {
            MainGrid.ID = this.ID + "MAINGRID";
            MainGrid.LayoutName = "MonitorAndAssessGrid";
            MainGrid.FactoryMethod = "GetLearners";
            MainGrid.FactoryParameters = "";
            MainGrid.ClassName = "Axelerate.SlkCourseManagerLogicalLayer.clsUsers, Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4";
            MainGrid.DefaultLock = "locked";
            MainGrid.LockOnColumns = true;
            MainGrid.AllowEdit = true;
            this.AllowEdit = true;            
        }

        /// <summary>
        /// Gets if the current user has Instructor permissions.
        /// </summary>
        private bool CurrentUserIsInstructor
        {
            get
            {
                try                {
                    
                    return clsUser.IsInstructor();
                    //return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Override to the CreateChilControls method to handle the rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            try
            {              
                base.CreateChildControls();
                if (CurrentUserIsInstructor)
                {
                    //MainGrid.ResetDatasource();
                    Controls.Add(MainGrid);
                }
                else
                {
                    System.Web.UI.WebControls.Label lbl = new System.Web.UI.WebControls.Label();
                    lbl.Text = Resources.ErrorMessages.UserNotInstructorOnSiteError;
                    Controls.Add(lbl);
                }
            }
            catch(Exception e)
            {
                System.Web.UI.WebControls.Label lbl = new System.Web.UI.WebControls.Label();
                lbl.Text = e.Message;
                lbl.ForeColor = System.Drawing.Color.Red;
                Controls.Add(lbl);
            }
        }

        /// <summary>
        /// Override to the OnPreRender method to handle the rendering.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {                               
            Controls.Clear();
            base.OnPreRender(e);
            CreateChildControls();
        }
        
    }
}
