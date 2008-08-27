using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;


namespace Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties
{
    [Serializable()]
    public class clsUILayout : GUIDNameBusinessTemplate<clsUILayout>
    {
        #region "DataLayer Overrides"

        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsUILayout), "UILayouts", "_ply", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }

        #endregion

        #region "Business Object Data"

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_ObjectType = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_LayoutXML = "";

        #endregion

        #region "Business Properties and Methods"

        public string ObjectType
        {
            get { return m_ObjectType; }
            set
            {
                m_ObjectType = value;
                PropertyHasChanged();
            }
        }

        public string LayoutXML
        {
            get { return m_LayoutXML; }
            set
            {
                m_LayoutXML = value;
                PropertyHasChanged();
            }
        }
        #endregion

        #region "Factory Methods"
        public static clsUILayout PropertyLayout(Type ObjectType, string Name)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsUILayout));
            Criteria.AddBinaryExpression("ObjectType_ply", "ObjectType", "=", ObjectType.FullName, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            Criteria.AddBinaryExpression("Name_ply", "Name", "=", Name, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
            clsUILayouts Layouts = clsUILayouts.GetCollection(Criteria);
            if (Layouts.Count > 0)                
                return Layouts[0];
            clsUILayout Layout = new clsUILayout();         
            Layout.ObjectType = ObjectType.FullName;
            return Layout;
        }
        #endregion

    }
}
