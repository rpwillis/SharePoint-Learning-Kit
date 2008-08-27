using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using System.Collections;
using Axelerate.BusinessLayerUITools.WebParts;

namespace Axelerate.BusinessLayerUITools
{
    public class tlpBusinessSelector : EditorPart
    {
        Hashtable hashtable;

        public tlpBusinessSelector()
        {
            this.ID = "BodyEditor";
            this.Title = Resources.LocalizationUIToolsResource.strLoadedAscxProp;
        }

        private Hashtable hash
        {
            get
            {
                if (hashtable == null)
                {
                    hashtable = new Hashtable();
                    return hashtable;
                }
                else
                {
                    return hashtable;
                }
            }
            
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            regenerateControls();
        }

        internal void regenerateControls()
        {
            //hash.Clear();
            wptBOGenericWebPart part = WebPartToEdit as wptBOGenericWebPart;
            if (part != null)
            {
                if (part.WPart != null)
                {
                    System.Reflection.PropertyInfo[] Properites = part.WPart.GetType().GetProperties();//(System.Reflection.BindingFlags.Public);
                    foreach (System.Reflection.PropertyInfo pt in Properites)
                    {
                        if (hash[pt.Name] == null)
                        {
                            if (pt.IsDefined(typeof(WebBrowsableAttribute), true))
                            {
                                WebBrowsableAttribute[] wbA = (WebBrowsableAttribute[])pt.GetCustomAttributes(typeof(WebBrowsableAttribute), true);
                                if (wbA[0].Browsable)
                                {
                                    if (pt.PropertyType.IsEnum)
                                    {
                                        Literal lit = new Literal();
                                        lit.Text = "</br><hr/>";
                                        DropDownList dropdown = new DropDownList();
                                        dropdown.Text = pt.Name + "</br>";
                                        dropdown.ID = pt.Name;
                                        String[] names = Enum.GetNames(pt.GetType());
                                        foreach (String name in names)
                                        {
                                            Object proper = Enum.Parse(pt.GetType(), name);
                                            dropdown.Items.Add(new ListItem(name));
                                        }
                                        hash.Add(dropdown.ID, dropdown);
                                        dropdown.SelectedValue = Enum.GetName(pt.GetType(), pt.GetValue(part.WPart, null));
                                        Controls.Add(dropdown);
                                        Controls.Add(lit);
                                    }
                                    else if (typeof(bool).Equals(pt.PropertyType))
                                    {
                                        Literal lit = new Literal();
                                        lit.Text = "</br><hr/>";
                                        CheckBox check = new CheckBox();
                                        check.Text = pt.Name;
                                        check.ID = pt.Name;
                                        check.Checked = (bool)pt.GetValue(part.WPart, null);
                                        hash.Add(check.ID, check);
                                        Controls.Add(check);
                                        Controls.Add(lit);
                                    }
                                    else
                                    {
                                        Label label = new Label();
                                        label.Text = pt.Name + "</br>";
                                        TextBox textbox = new TextBox();
                                        Literal lit = new Literal();
                                        lit.Text = "</br><hr/>";
                                        textbox.ID = pt.Name;
                                        textbox.Text = pt.GetValue(part.WPart, null).ToString();
                                        hash.Add(textbox.ID, textbox);
                                        Controls.Add(label);
                                        Controls.Add(textbox);
                                        Controls.Add(lit);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override bool ApplyChanges()
        {
            EnsureChildControls();            
            wptBOGenericWebPart part = WebPartToEdit as wptBOGenericWebPart;
            if (part != null)
            {            
                if (part.WPart == null)
                {
                    return false;
                }
                else
                {
                    if (part.WPart.TemplateControl.AppRelativeVirtualPath != part.ClassPath)
                    {
                        regenerateControls();
                        return false;
                    }
                    System.Reflection.PropertyInfo[] Properites = part.WPart.GetType().GetProperties();// (System.Reflection.BindingFlags.Public);
                    foreach (System.Reflection.PropertyInfo pt in Properites)
                    {
                        if (pt.IsDefined(typeof(WebBrowsableAttribute), true))
                        {
                            WebBrowsableAttribute[] wbA = (WebBrowsableAttribute[])pt.GetCustomAttributes(typeof(WebBrowsableAttribute), true);
                            if (wbA[0].Browsable)
                            {
                                if (pt.PropertyType.IsEnum)
                                {
                                    DropDownList dropdown = (DropDownList)hash[pt.Name];
                                    pt.SetValue(part.WPart, Enum.Parse(pt.GetType(), dropdown.SelectedValue), null);
                                }
                                else if (typeof(bool).Equals(pt))
                                {
                                    CheckBox check = (CheckBox)hash[pt.Name];
                                    pt.SetValue(part.WPart, check.Checked, null);
                                }
                                else
                                {
                                    TextBox textbox = (TextBox)hash[pt.Name];
                                    pt.SetValue(part.WPart, textbox.Text, null);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public override void SyncChanges()
        {
            EnsureChildControls(); 
            wptBOGenericWebPart part = WebPartToEdit as wptBOGenericWebPart;
            if (part != null)
            {
                if (part.WPart == null)
                {
                    return;
                }
                else
                {
                    if (part.WPart.TemplateControl.AppRelativeVirtualPath != part.ClassPath)
                    {
                        regenerateControls();
                        return;
                    }
                    System.Reflection.PropertyInfo[] Properites = part.WPart.GetType().GetProperties();// (System.Reflection.BindingFlags.Public);
                    foreach (System.Reflection.PropertyInfo pt in Properites)
                    {
                        if (pt.IsDefined(typeof(WebBrowsableAttribute), true))
                        {
                            WebBrowsableAttribute[] wbA = (WebBrowsableAttribute[])pt.GetCustomAttributes(typeof(WebBrowsableAttribute), true);
                            if (wbA[0].Browsable)
                            {
                                if (pt.PropertyType.IsEnum)
                                {
                                    DropDownList dropdown = (DropDownList)hash[pt.Name];
                                    pt.SetValue(part.WPart, Enum.Parse(pt.GetType(), dropdown.SelectedValue), null);
                                }
                                else if (typeof(bool).Equals(pt))
                                {
                                    CheckBox check = (CheckBox)hash[pt.Name];
                                    pt.SetValue(part.WPart, check.Checked, null);
                                }
                                else
                                {
                                    TextBox textbox = (TextBox)hash[pt.Name];
                                    pt.SetValue(part.WPart, textbox.Text, null);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
