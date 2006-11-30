@echo off
REM --  Copyright (c) Microsoft Corporation. All rights reserved. 
@echo on

if "%1"=="Clone" goto DoClone
if "%1"=="Clean" goto DoClean
rem -- BAD LINKFILE.BAT COMMAND
goto exit

:DoClone
mkdir App\Frameset
mkdir App\Frameset\Include
mkdir App\Frameset\Theme
mkdir App\Frameset\Images
goto done

:DoClean
rmdir App\Frameset\Theme /S /Q
rmdir App\Frameset\Include /S /Q
rmdir App\Frameset\Images /S /Q
REM rmdir App\Frameset /S /Q
goto done

:done

REM -- FROM: Samples\BasicWebPlayer\Frameset\Include
REM -- TO:   Slk\App\Frameset\Include
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\FramesetMgr.js" "App\Frameset\Include\FramesetMgr.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\Nav.js" "App\Frameset\Include\Nav.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\Toc.js" "App\Frameset\Include\Toc.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\parser.js" "App\Frameset\Include\parser.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\Parser1p2.js" "App\Frameset\Include\Parser1p2.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\Rte1p2Api.js" "App\Frameset\Include\Rte1p2Api.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\Rte2004Api.js" "App\Frameset\Include\Rte2004Api.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\RteApiSite.js" "App\Frameset\Include\RteApiSite.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\typevalidators.js" "App\Frameset\Include\typevalidators.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\typevalidators1p2.js" "App\Frameset\Include\typevalidators1p2.js"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Include\vernum.js" "App\Frameset\Include\vernum.js"

REM -- FROM: Samples\BasicWebPlayer\Frameset\Images
REM -- TO: Slk\App\Frameset\Images
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\BoxOff.gif" "App\Frameset\Images\BoxOff.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\BoxOffCorrect.gif" "App\Frameset\Images\BoxOffCorrect.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\BoxOn.gif" "App\Frameset\Images\BoxOn.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\BoxOnCorrect.gif" "App\Frameset\Images\BoxOnCorrect.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\BoxOnWrong.gif" "App\Frameset\Images\BoxOnWrong.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\ButtonOff.gif" "App\Frameset\Images\ButtonOff.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\ButtonOffCorrect.gif" "App\Frameset\Images\ButtonOffCorrect.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\ButtonOnCorrect.gif" "App\Frameset\Images\ButtonOnCorrect.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\ButtonOnWrong.gif" "App\Frameset\Images\ButtonOnWrong.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\Correct.gif" "App\Frameset\Images\Correct.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Images\Incorrect.gif" "App\Frameset\Images\Incorrect.gif"

REM -- FROM: Samples\BasicWebPlayer\Frameset\Theme
REM -- TO: Slk\App\Frameset\Theme
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\1px.gif" "App\Frameset\Theme\1px.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\Error.gif" "App\Frameset\Theme\Error.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\HeadCornerRt.gif" "App\Frameset\Theme\HeadCornerRt.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\HeadShadow.gif" "App\Frameset\Theme\HeadShadow.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\Info.gif" "App\Frameset\Theme\Info.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\Leaf.gif" "App\Frameset\Theme\Leaf.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\MinusBtn.gif" "App\Frameset\Theme\MinusBtn.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\nav_bg.gif" "App\Frameset\Theme\nav_bg.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\Next.gif" "App\Frameset\Theme\Next.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\NextHover.gif" "App\Frameset\Theme\NextHover.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\PlusBtn.gif" "App\Frameset\Theme\PlusBtn.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\Prev.gif" "App\Frameset\Theme\Prev.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\PrevHover.gif" "App\Frameset\Theme\PrevHover.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\Save.gif" "App\Frameset\Theme\Save.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\SaveHover.gif" "App\Frameset\Theme\SaveHover.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\Styles.css" "App\Frameset\Theme\Styles.css"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\TocClose.gif" "App\Frameset\Theme\TocClose.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\TocClosedTab.gif" "App\Frameset\Theme\TocClosedTab.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\TocCloseHover.gif" "App\Frameset\Theme\TocCloseHover.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\TocOpen.gif" "App\Frameset\Theme\TocOpen.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\TocOpenHover.gif" "App\Frameset\Theme\TocOpenHover.gif"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Theme\TopShadow.gif" "App\Frameset\Theme\TopShadow.gif"

REM -- FROM: Samples\BasicWebPlayer\Frameset
REM -- TO: Slk\App\Frameset
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Bottom.htm" "App\Frameset\Bottom.htm"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\NoScroll.htm" "App\Frameset\NoScroll.htm"
call LinkFile %1 "..\Samples\BasicWebPlayer\Frameset\Title.htm" "App\Frameset\Title.htm"

REM -- FROM: Samples\BasicWebPlayer\App_Code\Frameset
REM -- TO: Slk\Dll
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\ChangeActivityHelper.cs" "Dll\ChangeActivityHelper.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\ContentHelper.cs" "Dll\ContentHelper.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\Encoding.cs" "Dll\Encoding.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\FramesetHelper.cs" "Dll\FramesetHelper.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\FramesetQueryParam.cs" "Dll\FramesetQueryParam.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\FrmPageHelper.cs" "Dll\FrmPageHelper.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\HiddenHelper.cs" "Dll\HiddenHelper.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\Log.cs" "Dll\Log.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\PostableFrameHelper.cs" "Dll\PostableFrameHelper.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\RteDataModelConverter.cs" "Dll\RteDataModelConverter.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\Rte2004DataModelConverter.cs" "Dll\Rte2004DataModelConverter.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\Rte1p2DataModelConverter.cs" "Dll\Rte1p2DataModelConverter.cs"
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\Frameset\TocHelper.cs" "Dll\TocHelper.cs"

REM -- FROM: Samples\BasicWebPlayer\App_GlobalResources
REM -- TO: Slk\Dll
call LinkFile %1 "..\Samples\BasicWebPlayer\App_GlobalResources\FramesetResources.resx" "Dll\FramesetResources.resx"

REM -- FROM: Samples\BasicWebPlayer\App_Code
REM -- TO: Slk\Dll
call LinkFile %1 "..\Samples\BasicWebPlayer\App_Code\HttpModule.cs" "Dll\HttpModule.cs"

:exit

