Process for localizing new language

1.  Create a folder for the langauge with its name as the locale id
2.  Create a file in the folder culture.txt which contains the culture
3.  Copy SLK.resx into the folder from \slk\slk\solution\resources\
4.  Copy SlkSettings.xml from \slk\slk\SlkSettings.xml and rename it SlkSettings.xml.dat
5.  Extract strings from dlls into translation format
    a.  Copy the following dlls into the bin folder
        Microsoft.LearningComponents.dll
        Microsoft.SharePointLearningKit.dll
        Microsoft.LearningComponents.SharePoint.dll
        Microsoft.LearningComponents.Storage.dll
    b.  Run extractstrings.cmd. This will extract all the string and put then in the out folder
    c.  Copy the xml files from the out folder into the language folder.
