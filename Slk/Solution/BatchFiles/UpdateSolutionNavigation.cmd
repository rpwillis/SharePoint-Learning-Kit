rem -- updates WSS so navigation "breadcrumbs" work for SharePointLearningKit pages
@SET SPDIR="%commonprogramfiles%\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o copyappbincontent
