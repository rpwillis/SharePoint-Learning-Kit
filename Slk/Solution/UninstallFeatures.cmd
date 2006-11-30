rem -- uninstalls the features in the SharePointLearningKit solution
rem -- this is required to be able to do a retract
@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o uninstallfeature -name SharePointLearningKit -force
%SPDIR%\bin\stsadm -o uninstallfeature -name SharePointLearningKitAdmin -force

rem -- need to figure out what else is needed so I can remove the -force parameters
