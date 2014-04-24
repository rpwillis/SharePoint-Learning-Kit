using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.PowerShell;
using System.Management.Automation;

namespace Microsoft.SharePointLearningKit.PowerShell
{
    /// <summary>Gets SLK mappings</summary>
    [Cmdlet(VerbsCommon.Get, "SlkSPSiteMapping"),
    SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    public class SPCmdletGetSlkMapping : SPGetCmdletBase<SlkSPSiteMapping>
    {
        /// <summary></summary>
        protected override void InternalValidate()
        {
            if (this.Identity != null)
            {
                base.DataObject = this.Identity.Read();
                if (base.DataObject == null)
                {
                    base.WriteError(new PSArgumentException("The mapping does not exist."), ErrorCategory.InvalidArgument, this.Identity);
                    base.SkipProcessCurrentRecord();
                }
            }
        }

        /// <summary>Retrieve all items.</summary>
        protected override IEnumerable<SlkSPSiteMapping> RetrieveDataObjects()
        {
            if (base.DataObject != null)
            {
                List<SlkSPSiteMapping> list = new List<SlkSPSiteMapping>();
                list.Add(base.DataObject);
                return list;
            }

            return SlkSPSiteMapping.GetMappings();
        }

        /// <summary>The identity.</summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 0), Alias(new string[] { "Url" })]
        public SlkSPSiteMappingPipeBind Identity
        {
            get;
            set;
        }
    }

    /// <summary>A pipe bind for SlkSPSiteMapping.</summary>
    public sealed class SlkSPSiteMappingPipeBind : SPCmdletPipeBind<SlkSPSiteMapping>
    {
        /// <summary>The ID.</summary>
        public Guid Id { get; private set; }
        /// <summary>The Url.</summary>
        public string Url { get; private set; }

        /// <summary>Initializes a new instance of <see cref="SlkSPSiteMappingPipeBind"/>.</summary>
        public SlkSPSiteMappingPipeBind(SlkSPSiteMapping instance) : base(instance)
        {
        }

        /// <summary>Initializes a new instance of <see cref="SlkSPSiteMappingPipeBind"/>.</summary>
        public SlkSPSiteMappingPipeBind(Guid guid)
        {
            this.Id = guid;
        }

        /// <summary>Initializes a new instance of <see cref="SlkSPSiteMappingPipeBind"/>.</summary>
        public SlkSPSiteMappingPipeBind(string inputString)
        {
            if (inputString != null)
            {
                inputString = inputString.Trim();
                try
                {
                    this.Id = new Guid(inputString);
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
                if (this.Id.Equals(Guid.Empty))
                {
                    this.Url = inputString;
                    /*
                    if (this.url.StartsWith("http", true, CultureInfo.CurrentCulture))
                    {
                        this.m_IsAbsoluteUrl = true;
                    }
                    */
                }
            }
        }

        /// <summary>Initializes a new instance of <see cref="SlkSPSiteMappingPipeBind"/>.</summary>
        public SlkSPSiteMappingPipeBind(Uri uri)
        {
            this.Url = uri.ToString();
        }

        /// <summary></summary>
        protected override void Discover(SlkSPSiteMapping instance)
        {
            this.Id = instance.SPSiteGuid;
        }

        /// <summary></summary>
        public override SlkSPSiteMapping Read()
        {
            SlkSPSiteMapping mapping = null;
            string parameterDetails = string.Format(CultureInfo.CurrentCulture, "Id or Url : {0}", new object[] { "Empty or Null" });

            try
            {
                if (Id != Guid.Empty)
                {
                    mapping = SlkSPSiteMapping.GetMappingById(Id);
                }
                else
                {
                    try
                    {
                        using (SPSite spSite = new SPSite(Url))
                        {
                            mapping = SlkSPSiteMapping.GetMapping(spSite.ID);
                        }
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        SPFarm farm = SPFarm.Local;
                        SPWebApplication webApp = SPWebApplication.Lookup(new Uri(Url));
                        mapping = SlkSPSiteMapping.GetMapping(webApp.Id);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new SPCmdletPipeBindException(string.Format("The SlkSPSiteMapping Pipebind object could not be found ({0}).", parameterDetails), exception);
            }

            if (mapping == null)
            {
                throw new SPCmdletPipeBindException(string.Format("The SlkSPSiteMapping Pipebind object could not be found ({0}).", parameterDetails));
            }

            return mapping;
        }

    }
}

