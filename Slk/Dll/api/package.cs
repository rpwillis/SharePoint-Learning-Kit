using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>A learning Package, either an e-learning package or a simple electronic document.</summary>
    public class Package : IDisposable
    {
        PackageReader reader;
        ISlkStore store;
        SPFile file;

#region constructors
        /// <summary>Initializes a new instance of <see cref="Package"/>.</summary>
        /// <param name="store">The ISlkStore to use.</param>
        /// <param name="file">The package file in SharePoint.</param>
        /// <param name="web">The SPWeb containing the file. SP 2007 SPFile does not expose this as a property.</param>
        public Package(ISlkStore store, SPFile file, SPWeb web) : this (store, file, CreateFileLocation(web, file))
        {
        }

        /// <summary>Initializes a new instance of <see cref="Package"/>.</summary>
        /// <param name="store">The ISlkStore to use.</param>
        /// <param name="file">The package file in SharePoint.</param>
        /// <param name="location">The location of the file.</param>
        public Package(ISlkStore store, SPFile file, SharePointFileLocation location)
        {
            this.store = store;
            this.file = file;
            Location = location;
            try
            {
                reader = store.CreatePackageReader(file, location);
                Initialize();
            }
            catch (CacheException ex)
            {
                throw new SafeToDisplayException(ex.Message);
            }
            catch (InvalidPackageException ex)
            {
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentUICulture, Resources.Properties.AppResources.PackageNotValid, ex.Message));
            }
        }
#endregion constructors

#region properties
        /// <summary>The ID of the package.</summary>
        public PackageItemIdentifier Id { get; private set; }
        /// <summary>The location of the file.</summary>
        public SharePointFileLocation Location { get; private set; }
        /// <summary>Any warnings for the package.</summary>
        public LearningStoreXml Warnings { get; private set; }
        /// <summary>The title of the package.</summary>
        public string Title { get; private set; }
        /// <summary>The description of the package.</summary>
        public string Description { get; private set; }
        /// <summary>The organizations in the package.</summary>
        public IList<OrganizationNodeReader> Organizations { get; private set; }
        /// <summary>The ID of the default organization.</summary>
        public string DefaultOrganizationId { get; private set; }
        /// <summary>Shows if the package is e-learning or not.</summary>
        public bool IsNonELearning { get; private set; }
        /// <summary>The format of the package.</summary>
        public PackageFormat PackageFormat { get; private set; }
#endregion properties

#region public methods
        /// <summary>Finds the root activity of a package for a particular organization.</summary>
        /// <param name="organizationIndex">The index of the organization.</param>
        /// <returns>The root identifier.</returns>
        public ActivityPackageItemIdentifier FindRootActivity(int organizationIndex)
        {
            return store.FindRootActivity(Id, organizationIndex);
        }

        /// <summary>Register the package if required.</summary>
        public void Register()
        {
            if (Id == null)
            {
                PackageDetails package = store.RegisterAndValidatePackage(reader);
                Id = package.PackageId;
            }
        }

        /// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
        public void Dispose()
        {
            reader.Dispose();
        }
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        void Initialize()
        {
            IsNonELearning = PackageValidator.ValidateIsELearning(reader).HasErrors;

            if (IsNonELearning == false)
            {
                ValidatePackage();
                    
                ManifestReader manifestReader = new ManifestReader(reader, new ManifestReaderSettings(true, true));
                MetadataNodeReader metadataReader = manifestReader.Metadata;

                // set <title> to the title to display for the package, using these rules:
                //   1. if there's a Title column value, use it;
                //   2. otherwise, if there's a title specified in the package metadata, use it;
                //   3. otherwise, use the file name without the extension
                if (String.IsNullOrEmpty(file.Title) == false)
                {
                    Title = file.Title;
                }
                else 
                {
                    string titleFromMetadata = metadataReader.GetTitle(CultureInfo.CurrentUICulture);
                    if (string.IsNullOrEmpty(titleFromMetadata) == false)
                    {
                        Title = titleFromMetadata;
                    }
                    else
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                    }
                }

                // set description to the package description specified in metadata, or null if none
                IList<string> descriptions = metadataReader.GetDescriptions(CultureInfo.CurrentUICulture);
                if (descriptions.Count > 0)
                {
                    Description = descriptions[0];
                    if (Description == null)
                    {
                        Description = string.Empty;
                    }
                }
                else
                {
                    Description = string.Empty;
                }

                // populate the drop-down list of organizations; hide the entire row containing that
                // drop-down if there's only one organization
                Organizations = manifestReader.Organizations;
                DefaultOrganizationId = manifestReader.DefaultOrganization.Id;
                PackageFormat = manifestReader.PackageFormat;
            }
        }

        void ValidatePackage()
        {
            PackageDetails details = store.LoadPackageFromStore(Location);

            if (details != null)
            {
                Warnings = details.Warnings;
                Id = details.PackageId;
            }
            else
            {
                PackageValidatorSettings settings = new PackageValidatorSettings(ValidationBehavior.LogWarning, ValidationBehavior.None, ValidationBehavior.Enforce, ValidationBehavior.LogWarning);
                ValidationResults log = PackageValidator.Validate(reader, settings);
                if (log.HasErrors)
                {
                    // Shouldn't have any since enforcing errors.
                    throw new SafeToDisplayException(log, String.Format(CultureInfo.CurrentUICulture, Resources.Properties.AppResources.PackageNotValid, string.Empty));
                }
                else if (log.HasWarnings)
                {
                    using (System.Xml.XmlReader xmlLog = log.ToXml())
                    {
                        Warnings = LearningStoreXml.CreateAndLoad(xmlLog);
                    }
                }
            }
        }
#endregion private methods

#region static members

        /// <summary>Creates a new <see cref="SharePointFileLocation"/> object.</summary>
        /// <param name="web">The web the file is in - not in SPFile properties in SP2007.</param>
        /// <param name="file">The file to get the location for.</param>
        /// <returns>A <see cref="SharePointFileLocation"/>.</returns>
        public static SharePointFileLocation CreateFileLocation(SPWeb web, SPFile file)
        {
            return new SharePointFileLocation(web, file.UniqueId, file.UIVersion);
        }
#endregion static members

#region NoPackageLocation
        /// <summary>Represents a non-location.</summary>
        public static readonly SharePointFileLocation NoPackageLocation = new SharePointFileLocation(Guid.Empty, Guid.Empty, Guid.Empty, 0, DateTime.MinValue);
#endregion NoPackageLocation

    }
}

