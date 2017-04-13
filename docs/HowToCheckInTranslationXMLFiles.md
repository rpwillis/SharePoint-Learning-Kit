Congratulations for the initiative to make SLK available to your country! To make this valuable contribution available to the larger SLK community, please read further. This wiki page details the process to make the translated XMLs part of the SLK source tree.

The _Localization Guide_ in the SLK source tree, **\Slk\Tools\LocalizationGuide\Doc**, details the tools available for localizing SLK in your language. Once localization is complete and tested, the set of translated XML files (including the SLKSetting.xml) should be uploaded to the SLK source tree. The procedure for the same is as follows in the mentioned order,

#### 1. Locate your workitem in the IssueTracker
Locate a work item named, ‘Localization Task: <Your language>’, corresponding to the localization task in question. If you cannot locate the workitem corresponding to your language, it could be that the community is not aware of such a development yet. Please leave a message on this discussion thread, [http://www.codeplex.com/SLK/Thread/View.aspx?ThreadId=8757](http://www.codeplex.com/SLK/Thread/View.aspx?ThreadId=8757), which aims to inform all CodePlex users of ongoing localization efforts. Following this, the localization co-ordinator will add a task item to the Issue Tracker corresponding to your localization task.

Add information pertaining to your localization that is of interest to the community to your task item. Communicate the progress of the localization through the workitem. This will enable all interested users to track the progress of localization for their favorite language. 

#### 2. Owners should add themselves to the mailing list
Every localization task should have an identified owner. The owner should send a mail to +slkfb@microsoft.com+ with the subject ‘localization owner: <your language>’ to be added to the mailing list of localization owners. The mailing list will be used for discussions and communications related to core localization issues.

#### 3. Signing the Assignment Agreement
All code submissions to any CodePlex project follow the below mentioned general assignment agreement process to sign an electronic copy affirming your willingness to make a submission to the open source project in question.

An email has to be sent to ssiadmin@microsoft.com by the contributor requesting a copy of the electronic assignment agreement. Please include the name of the project, “SLK” in this case, in the subject of your email. In response to this email, you will receive an email from the CodePlex Shared Source team with the electronic form to sign and a document detailing instructions on using the electronic system. On successful completion of this process, we will receive notification and communicate the same to you through your task item. At this stage, we are ready to receive your submission.

#### 4. Upload the language XMLs
Upload all the translated XMLs to the WorkItem created in Step 1. Please use the following as a check-list to ensure that  your submission is as per the standard SLK localization submission format

* Package your files with the name <LCID>.zip. 
* The package should include,
	* Translated XML files, one for each dll in that release. For the case of the 1.0.799 release, the list of files would be LearningComponents.xml, SharePoint.xml, SharePointLearningKit.xml, Storage.xml. 
	* The translated SlkSetting.xml.dat
	* culture.txt file includes the 'SpecificCulture' name of the language pack
* The XML files should have the correct localization version.

#### 5. Upload the quality write-up
Since contributors have varying commitments with respect to the project, instead of having a quality bar for accepting the submissions, all submissions are required to have information about he quality of the translation. This enables the end user to judge and analyze if it suits their needs.

The quality write-up should at least have the below mentioned information,
* +Translation Percentage:+ This is the percentage of the number of strings that have been translated. Use this tool, [LocPercentageCalculator.zip](HowToCheckInTranslationXMLFiles_LocPercentageCalculator.zip), to calculate the percentage of translation. Usage instructions are included in the ‘readme’ in the tool. Localization owners will calculate the translation percentage using this tool and upload the info on CodePlex. 
* +Crucial Strings Translation Status:+ The localization owners will state whether all the crucial strings (strings mentioned in crucialStrings.txt) have been translated or not
* +Translation Quality:+ If there is a Microsoft Sub related to your localization effort, the Sub will be required to add this information. In the case there is no other such related entity, the translators will be required to add this info. This will be a descriptive message about the quality of the translation from English to the language in question. 
* +Localized UI Quality:+ If there is a Microsoft Sub related to your localization effort, the Sub will be required to add this information. In the case there is no other such related entity, the translators will be required to add this info. This will be a descriptive message indicating whether all UI changes have been taken care of. This parameter is important for all the non-roman languages (JP and AR) whose UI changes during localization. 
* +Name of the reviewer:+ The name of the reviewer and information about whether the reviewer is a translator or some other entity

Examples can be found in existing localization work items.

#### 6. Sanity Testing
After completion of **all** the above mentioned steps, the localization co-ordinator will execute and communicate results of a sanity test of the language pack. Owners should check back for comments and make corrections if necessary till a 'sane' language pack can be produced.

#### 7. Inclusion in the source tree and release of the language pack
A set of dates will be designated for localization releases following every SLK release. For the 1.0.799 release, the dates are 06/14/2007, 07/14/2007 and 08/14/2007. On each of these dates, all the workitems bearing a status of **Ready for Release** will be checked into the SLK source tree (under Slk\Tools\LocalizationGuide\TranslatedXMLs\<lcid of the language>) and product signed and released. To reach a 'Ready for Release' status, all the above mentioned steps should be successfully completed. Owners should work towards moving their workitem to the ‘Ready for Release’ status on/before one of these release dates. 

### Current Languages in Localization

The list of languages in localization and their associated work items are captured in the [LanguagesInLocalization](LanguagesInLocalization) wiki.
