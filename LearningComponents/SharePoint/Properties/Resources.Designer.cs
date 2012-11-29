//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.5466
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.LearningComponents.SharePoint {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.LearningComponents.SharePoint.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The package cache directory is not accessible. It may not exist or you may not have access to it..
        /// </summary>
        internal static string CacheCachePathNotAccessible {
            get {
                return ResourceManager.GetString("CacheCachePathNotAccessible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Creating the cache directory failed and then the invalid cache folder and/or lock file could not be removed..
        /// </summary>
        internal static string CacheCreateDirCleanupFailed {
            get {
                return ResourceManager.GetString("CacheCreateDirCleanupFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Locking the cache directory failed and then the invalid cache folder and/or lock file could not be removed..
        /// </summary>
        internal static string CacheLockDirCleanupFailed {
            get {
                return ResourceManager.GetString("CacheLockDirCleanupFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid library cache {0}..
        /// </summary>
        internal static string InvalidLibraryCache {
            get {
                return ResourceManager.GetString("InvalidLibraryCache", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid directory specified for the permanent cache..
        /// </summary>
        internal static string PermanentCacheInvalidDirectory {
            get {
                return ResourceManager.GetString("PermanentCacheInvalidDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No directory specified for the permanent cache..
        /// </summary>
        internal static string PermanentCacheNoDirectory {
            get {
                return ResourceManager.GetString("PermanentCacheNoDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not write to lock file of cache directory..
        /// </summary>
        internal static string SPCannotWriteToLockFile {
            get {
                return ResourceManager.GetString("SPCannotWriteToLockFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file {0} was not found..
        /// </summary>
        internal static string SPFileNotFound {
            get {
                return ResourceManager.GetString("SPFileNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The requested file was not found..
        /// </summary>
        internal static string SPFileNotFoundNoName {
            get {
                return ResourceManager.GetString("SPFileNotFoundNoName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The version {0} of file {1} does not exist..
        /// </summary>
        internal static string SPFileVersionNotFound {
            get {
                return ResourceManager.GetString("SPFileVersionNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; does not have the correct format..
        /// </summary>
        internal static string SPFormatInvalid {
            get {
                return ResourceManager.GetString("SPFormatInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The package is not valid and cannot be read. More information: {0}.
        /// </summary>
        internal static string SPInvalidPackage {
            get {
                return ResourceManager.GetString("SPInvalidPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not get a lock on the file within the allowed tries..
        /// </summary>
        internal static string SPLockTriesExceeded {
            get {
                return ResourceManager.GetString("SPLockTriesExceeded", resourceCulture);
            }
        }
    }
}
