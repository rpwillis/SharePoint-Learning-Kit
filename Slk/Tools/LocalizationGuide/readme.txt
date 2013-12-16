Process for localizing new language

1.  Create a folder for the langauge with its name as the locale id
2.  Create a file in the folder culture.txt which contains the culture
3.  Copy SLK.resx into the folder from \slk\slk\solution\resources\
4.  Copy SlkSettings.xml from \slk\slk\SlkSettings.xml and rename it SlkSettings.xml.dat
5.  Extract strings from dlls into translation format
    a.  Run extractstrings.cmd. This will copy the dlls containing the resources into the bin folder, extract all the strings and put then in the bin\out folder
    b.  Copy the xml files from the bin\out folder into the language folder.
