rem -- adds the SharePointLearningKit feature directory to WSS
SET SPDIR="c:\program files\common files\microsoft shared\web server extensions\12"
mkdir %SPDIR%\Template\Features\SharePointLearningKit
copy /y feature.xml %SPDIR%\Template\Features\SharePointLearningKit
copy /y elements.xml %SPDIR%\Template\Features\SharePointLearningKit
