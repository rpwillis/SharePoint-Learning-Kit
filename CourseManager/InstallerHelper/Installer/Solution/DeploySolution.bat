@SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
%SPDIR%\bin\stsadm -o deploysolution -name Course_Manager_Configure.wsp -url http://localhost/ -immediate -allowGacDeployment
%SPDIR%\bin\stsadm -o execadmsvcjobs