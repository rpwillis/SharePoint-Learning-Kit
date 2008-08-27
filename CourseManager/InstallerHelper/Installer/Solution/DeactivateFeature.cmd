SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o deactivatefeature -name Course_Manager_Configure -url http://cicoursemanager:32436 -force
