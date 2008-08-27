using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace BusinessLayerUITools
{
    /// <summary>
    /// collection class that serves as a container
    /// for the clsBOValidatorProperty
    /// </summary>
    public class clsBOValidatorProperties : CollectionBase
    {
        private clsBusinessObjectErrorValidator m_Control;

        public clsBOValidatorProperties(clsBusinessObjectErrorValidator control)
        {
            m_Control = control;
        }

        // Gets and sets validator at the specified position
        public clsBOValidatorProperty this[int index]
        {
            get
            {
                return (clsBOValidatorProperty)InnerList[index];
            }
            set
            {
                InnerList[index] = value;
            }
        }

        /// <summary>
        /// this method is overriden to ensure that each
        /// validator at design time has a reference
        /// to its parent validator control
        /// </summary>
        /// <param name="value"></param>
        protected override void OnValidate(object value)
        {
            (value as clsBOValidatorProperty).HostingControl = m_Control;
        }


        public void Add(clsBOValidatorProperty validator)
        {
            validator.HostingControl = m_Control;
            InnerList.Add(validator);
        }


        public void AddAt(int index, clsBOValidatorProperty validator)
        {
            validator.HostingControl = m_Control;
            InnerList.Insert(index, validator);
        }
    }
}
