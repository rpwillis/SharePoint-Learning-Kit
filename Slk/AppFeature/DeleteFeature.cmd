rem -- deletes the SharePointLearningKit feature directory from WSS
SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
rmdir /s /q %SPDIR%\Template\Features\SharePointLearningKit
