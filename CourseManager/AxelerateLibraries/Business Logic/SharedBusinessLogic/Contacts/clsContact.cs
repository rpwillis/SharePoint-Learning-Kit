using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using System.Drawing;
using System.Drawing.Imaging;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Contacts
{
    /// <summary>
    /// Define an external contact's information attributes. It inherits two base properties from GUIDNameBusinessTemplate, 
    /// a GUID property that identifies the instance and a Name property that describes the instance.
    /// </summary>
    [Serializable(), SecurityToken("clsContact", "clsContacts", "MIPCustom")]
    public class clsContact : GUIDNameBusinessTemplate<clsContact>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsContact), "Contacts", "_ctc", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"
      
        /// <summary>
        /// Contact's email address
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Email = "";

        /// <summary>
        /// Contact's phone
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Phone = "";

        /// <summary>
        /// Contact's cellphone number
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_CellPhone = "";
        
        /// <summary>
        /// Contact's Address
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Address = "";

        /// <summary>
        /// Contact's country
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Country = "";

        /// <summary>
        /// Biography
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Biography = "";

        /// <summary>
        /// Contact's picture.  The binary information for the picture bitmap
        /// </summary>
        private byte[] m_Picture = null;

        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or Sets the Contact's email address
        /// </summary>
        public string Email
        {
            get { return m_Email; }
            set
            {
                m_Email = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Contact's phone string
        /// </summary>
        public string Phone
        {
            get { return m_Phone; }
            set
            {
                m_Phone = value;
                PropertyHasChanged();
            }
        }

        public string CellPhone
        {
            get { return m_CellPhone; }
            set
            {
                m_CellPhone = value;
                PropertyHasChanged();
            }
        }

        public string Address
        {
            get { return m_Address; }
            set
            {
                m_Address = value;
                PropertyHasChanged();
            }
        }

        public string Country
        {
            get { return m_Country; }
            set
            {
                m_Country = value;
                PropertyHasChanged();
            }
        }

        public string Biography
        {
            get { return m_Biography; }
            set
            {
                m_Biography = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Contact's byte buffer for his/her picture
        /// </summary>
        public Bitmap Picture
        {
            get
            {
                if (m_Picture == null)
                {
                    DataLayerParameter[] Parameters = new DataLayerParameter[1];
                    Parameters[0] = new DataLayerParameter("ContactGUID", m_GUID);
                    BLCommandBase<SQLDataLayer> Command = new BLCommandBase<SQLDataLayer>("SP_GetContactPictureData", Parameters, "Shared");
                    Command.Execute();
                    if (Command.Result.Tables[0].Rows.Count > 0)    
                    {
                        object ResultValue =Command.Result.Tables[0].Rows[0]["Picture_ctc"]; 
                        if ((ResultValue == null) || (ResultValue == DBNull.Value ))
                            m_Picture = null;
                        else
                            m_Picture = (byte[])ResultValue;
                    };
                };

                if (m_Picture != null)
                {
                    System.IO.MemoryStream MemStream = new System.IO.MemoryStream();
                    MemStream.Write(m_Picture, 0, m_Picture.Length);
                    MemStream.Flush();
                    MemStream.Seek(0, System.IO.SeekOrigin.Begin);
                    Bitmap bmp = new Bitmap(MemStream);
                    return bmp;
                };
                return null;
            }
                
            set
            {   
                if (value == null)
                {
                    m_Picture = null;
                } else
                {                
                    //Max with and height of the image must be 128x128
                    System.Drawing.Bitmap ImgBmp;
                    ImgBmp = value;
                    if (value.Width > 128 || value.Height > 128)
                    {
                        //need a resize
                        double widhtScale = 0;
                        if (value.Width > 128)
                        {
                            widhtScale = (double)value.Width / 128.0;
                        }
                        double heigthScale = 0;
                        if (value.Height  > 128)
                        {
                            heigthScale = (double)value.Height / 128.0;
                        }
                        if (widhtScale > heigthScale && widhtScale > 0)
                        {
                            //ImgBmp = new Bitmap(value, new Size((int)(value.Width / widhtScale), (int)(value.Height / widhtScale)));
                            ImgBmp = new Bitmap((int)(value.Width / widhtScale), (int)(value.Height / widhtScale), PixelFormat.Format32bppArgb);
                            Graphics gr = Graphics.FromImage(ImgBmp);
                            gr.DrawImage(value, 0, 0, (int)(value.Width / widhtScale), (int)(value.Height / widhtScale));
                            gr.Flush();
                            gr.Dispose();
                        }
                        else
                        {
                            if (heigthScale > 0)
                            {
                                //ImgBmp = new Bitmap(value, new Size((int)(value.Width / heigthScale), (int)(value.Height / heigthScale)));
                                ImgBmp = new Bitmap((int)(value.Width / heigthScale), (int)(value.Height / heigthScale), PixelFormat.Format32bppArgb);
                                Graphics gr = Graphics.FromImage(ImgBmp);
                                gr.DrawImage(value, 0, 0, (int)(value.Width / heigthScale), (int)(value.Height / heigthScale));
                                gr.Flush();
                                gr.Dispose();
                            }
                        }
                    }
                    System.IO.MemoryStream MemStream = new System.IO.MemoryStream();
                    ImgBmp.Save(MemStream, ImageFormat.Jpeg);
                    MemStream.Flush();
                    m_Picture = new byte[MemStream.Length];
                    MemStream.Seek(0, System.IO.SeekOrigin.Begin);
                    MemStream.Read(m_Picture, (int)0, (int)MemStream.Length);
                    MemStream.Dispose();
                };                
                PropertyHasChanged();
            }
        }



        #endregion

        #region "Data Access"
        public override void BLInsert(BLBusinessBase ParentObject)
        {
            base.BLInsert(ParentObject);

            //Inserts the object file in the database
            if (m_Picture != null)
            {
                DataLayerParameter[] Parameters = new DataLayerParameter[2];
                Parameters[0] = new DataLayerParameter("ContactGUID", m_GUID);
                Parameters[1] = new DataLayerParameter("PictureData", m_Picture);
                BLCommandBase<SQLDataLayer> Command = new BLCommandBase<SQLDataLayer>("SP_SetContactPictureData", Parameters, "Shared");
                Command.BLExecuteCommand();
            }
        }

        public override void BLUpdate(BLBusinessBase ParentObject)
        {
            m_Name = Name;
            base.BLUpdate(ParentObject);
            //Inserts the object file in the database
            //Inserts the object file in the database
            if (m_Picture != null)
            {
                DataLayerParameter[] Parameters = new DataLayerParameter[2];
                Parameters[0] = new DataLayerParameter("ContactGUID", m_GUID);
                Parameters[1] = new DataLayerParameter("PictureData", m_Picture);
                BLCommandBase<SQLDataLayer> Command = new BLCommandBase<SQLDataLayer>("SP_SetContactPictureData", Parameters, "Shared");
                Command.BLExecuteCommand();
            }
        }
        #endregion



    }
}