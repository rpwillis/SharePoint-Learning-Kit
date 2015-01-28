call ExtractStrings.cmd
CD TranslatedXMLs
FOR /D %%I in (*) DO UpdateTranslations.CMD %%I
