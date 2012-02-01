Self Assign Package

The Self Assign Package sample is an example of how to deploy a web part page containing the Self Assign Web Part into a site.  

Summary 

The sample builds a web scoped feature which when activated:

1.  Creates a document library called Learning Objects
2.  Adds a web part page to the Site Pages library.
3.  Adds the self-assign web part to the web part page which looks at the Learning Objects library.

Feature

As the feature is web scoped it can be activated on as many sites in the site collection as you want, creating the artifacts in each one. The feature's title and description are stored in feature\Resources.resx.

When the feature is deactivated it does not remove the arifacts created which is a design decision to avoid deleting content added to the document library. If you wanted to remove everything on deactivation you would need to add a feature receiver which did this.

Due to this behaviour you may want to make it a hidden feature and then activate it either by scripting or stapling it to a site defintion to be activated on new sites created from that defintion. To make the feature hidden add Hidden="TRUE" to feature\feature.xml. 

To activate the feature by running PowerShell on SharePoint 2010 run
    Enable-SPFeature SelfAssignPackage -Url <url to site>
    e.g.
    Enable-SPFeature SelfAssignPackage -Url http://myServer/sites/schoola/Assignments

To activate the feature using stsadm run
    stsadm –o activatefeature –name SelfAssignPackage –url <url to site>
    e.g.
    stsadm –o activatefeature –name SelfAssignPackage –url http://myServer/sites/schoola/Assignments
    

Deactivating then reactivating the feature doesn't make any changes to the site.

Learning Objects Document Library

The document library created is a standard document library and is added to the site's quick launch. If you wanted to base it on another document library content type you need to edit the templateId in feature\LearningObjects.xml. It's name and description are stored in feature\Resources.resx. The document library is created in the root of the site which is the standard for document libraries.

Web Part Page

The web part page added to the Site Pages library is defined in feature\SelfAssign.aspx. This is a very simple web part page with one web part zone called Main. feature\SitePages.xml describes how to add the web part page to the library. The Module Url defines the library to add the web part page to, in this case it is the Site Pages library. If this does not exist then SharePoint will display an error when activating the feature.

After adding the page to the library, the feature then adds the Self Assign Web Part to the page. It sets the ListName property to the name of the document library created in step 1 - pulled from the resource file. As the Site and ViewName properties are not set then the web part will look at the library in the site the page is in and use the default view to retrieve the items.

Building the Solution

To build the feature run msbuild in this directory.
